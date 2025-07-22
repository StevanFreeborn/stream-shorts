using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;

using NAudio.Wave;

using Whisper.net;
using Whisper.net.Ggml;


var stopwatch = new Stopwatch();
stopwatch.Start();
var resourceManager = new ResourceManager("StreamShorts.Console.Resources.Resources", typeof(Program).Assembly);

Console.WriteLine(resourceManager.GetString("WelcomeMessage", CultureInfo.CurrentCulture));

// Step 1: Retrieve stream
if (args.Length is 0)
{
  Console.WriteLine(resourceManager.GetString("StreamNotProvided", CultureInfo.CurrentCulture));
  return;
}

using var mp3Stream = new MemoryStream();
using var mp4Stream = new FileStream(args[0], FileMode.Open, FileAccess.Read);

var wasExtracted = await FFMpegArguments
  .FromPipeInput(new StreamPipeSource(mp4Stream))
  .OutputToPipe(
    new StreamPipeSink(mp3Stream),
    o => o.DisableChannel(Channel.Video).ForceFormat("mp3")
  )
  .ProcessAsynchronously();

if (wasExtracted is false)
{
  Console.WriteLine(resourceManager.GetString("Mp3ExtractionFailed", CultureInfo.CurrentCulture));
  return;
}

// Step 2: Convert MP3 stream to 16khz wave format
mp3Stream.Position = 0;
using var reader = new Mp3FileReader(mp3Stream);
var outFormat = new WaveFormat(16000, reader.WaveFormat.Channels);
using var resampler = new MediaFoundationResampler(reader, outFormat);
using var waveStream = new MemoryStream();
WaveFileWriter.WriteWavFileToStream(waveStream, resampler);

// Step 3: Split the wave stream into 2 minute segments
waveStream.Position = 0;
var segmentDuration = TimeSpan.FromMinutes(2);
var segments = new List<MemoryStream>();
using var waveReader = new WaveFileReader(waveStream);
var segmentCount = (int)Math.Ceiling(waveReader.TotalTime.TotalMilliseconds / segmentDuration.TotalMilliseconds);

Directory.CreateDirectory("segments");

foreach (var i in Enumerable.Range(0, segmentCount))
{
  waveStream.Position = 0;
  using var segmentWaveReader = new WaveFileReader(waveStream);
  var segment = segmentWaveReader.ToSampleProvider()
    .Skip(i * segmentDuration)
    .Take(segmentDuration);
  var segmentProvider = segment.ToWaveProvider16();
  var segmentStream = new MemoryStream();
  WaveFileWriter.WriteWavFileToStream(segmentStream, segmentProvider);
  segmentStream.Position = 0;
  segments.Add(segmentStream);
}

// Step 4: Transcribe each segment using Whisper
using var modelMemoryStream = new MemoryStream();
var model = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.TinyEn);
await model.CopyToAsync(modelMemoryStream);
var whisperFactory = WhisperFactory.FromBuffer(modelMemoryStream.ToArray());
using var whisperProcessor = whisperFactory.CreateBuilder()
  .WithLanguage("en")
  .Build();

var completeTranscription = new StringBuilder();

foreach (var (i, segment) in segments.Select((s, index) => (index, s)))
{
  var durationOffset = TimeSpan.FromMilliseconds(i * segmentDuration.TotalMilliseconds);
  var segmentTranscription = new StringBuilder();

  await foreach (var result in whisperProcessor.ProcessAsync(segment, CancellationToken.None))
  {
    var startTime = result.Start + durationOffset;
    var endTime = result.End + durationOffset;
    segmentTranscription.AppendLine(CultureInfo.CurrentCulture, $"[{startTime:hh\\:mm\\:ss} - {endTime:hh\\:mm\\:ss}] {result.Text}");
  }

  completeTranscription.Append(segmentTranscription);
}

// Step 5: Send the transcription to LLM for analysis
// TODO: Explore this prompt further...seems break
// when transcription is long
var prompt = $"""
You are an expert in identifying engaging segments from YouTube live stream transcriptions that are suitable for creating short videos. You will be provided with a transcription of a YouTube live stream. Your task is to analyze the transcription and identify potential segments that would make compelling short videos.

For each segment you identify, you should create an object with the following attributes:

title: A concise and catchy title for the short video.
description: A brief description of the short video's content.
explanation: Explain why this segment would make a good short video (e.g., it's funny, informative, controversial, etc.).
start_time: The timestamp in the format HH:MM:SS where the segment begins in the original live stream.
end_time: The timestamp in the format HH:MM:SS where the segment ends in the original live stream.
Your response must be a JSON array of these objects. The JSON array should be the only output. Do not include any introductory or explanatory text outside of the JSON array.

Here is the transcription of the YouTube live stream:

{completeTranscription}
""";

using var client = new HttpClient()
{
  Timeout = TimeSpan.FromMinutes(30)
};
using var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:11434/api/generate")
{
  Content = new StringContent(
    JsonSerializer.Serialize(new
    {
      model = "llama3.1:latest",
      prompt,
      stream = false
    }),
    Encoding.UTF8,
    "application/json"
  )
};
var response = await client.SendAsync(request);
var responseContent = await response.Content.ReadAsStringAsync();
var responseJson = JsonSerializer.Deserialize<LLMResponse>(responseContent);
var analysis = JsonSerializer.Deserialize<List<LLMAnalysis>>(responseJson?.Response ?? string.Empty);

if (analysis is null)
{
  Console.WriteLine(resourceManager.GetString("LLMAnalysisFailed", CultureInfo.CurrentCulture));
  return;
}

foreach (var result in analysis)
{
  var fileName = string.Concat(result.Title.Split(Path.GetInvalidFileNameChars()));
  await FFMpeg.SubVideoAsync(
    args[0],
    $"{fileName}.mp4",
    result.StartTime,
    result.EndTime
  );
}


stopwatch.Stop();
Console.WriteLine(stopwatch.Elapsed.Minutes);

// Step 6: Use analysis to generate a short video

record LLMAnalysis(
  [property: JsonPropertyName("title")]
  string Title,
  [property: JsonPropertyName("description")]
  string Description,
  [property: JsonPropertyName("explanation")]
  string Explanation,
  [property: JsonPropertyName("start_time")]
  TimeSpan StartTime,
  [property: JsonPropertyName("end_time")]
  TimeSpan EndTime
);

record LLMResponse(
  [property: JsonPropertyName("response")]
  string Response
);