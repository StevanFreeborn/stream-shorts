using System.Globalization;
using System.Resources;
using System.Text;

using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;

using NAudio.Wave;

using Whisper.net;
using Whisper.net.Ggml;

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

  await File.AppendAllTextAsync("transcription.txt", segmentTranscription.ToString());
}

// Step 5: Send the transcription to LLM for analysis

// Step 6: Use analysis to generate a short video