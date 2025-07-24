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

Log.Logger = new LoggerConfiguration()
  .WriteTo.File(
    formatter: new CompactJsonFormatter(),
    path: Path.Combine(AppContext.BaseDirectory, "logs", "log.jsonl"),
    rollingInterval: RollingInterval.Day
  )
  .Enrich.FromLogContext()
  .MinimumLevel.Verbose()
  .MinimumLevel.Override("Microsoft", LogEventLevel.Fatal)
  .CreateLogger();

try
{
  var appName = Assembly.GetExecutingAssembly().GetName().Name;
  Log.Information("Starting {AppName}", appName);

  await Host.CreateDefaultBuilder(args)
    .ConfigureLogging(static l => l.ClearProviders())
    .ConfigureServices(static (_, services) =>
    {
      services.AddSingleton(AnsiConsole.Console);
      services.AddSingleton<IFileSystem, FileSystem>();
    })
    .BuildApp()
    .RunAsync(args);

  Log.Information("{AppName} has completed successfully.", appName);
}
catch (Exception ex)
{
  Log.Fatal(ex, "An unhandled exception occurred during execution.");
  throw;
}
finally
{
  await Log.CloseAndFlushAsync();
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
var prompt = $$"""
I need your help to transform my YouTube live stream transcript into engaging YouTube Shorts. Act as my content editor and pinpoint **all potential candidate segments** that are perfect for short-form video. I'm looking for clips that are:

  * **Funny:** Moments that will make viewers laugh.
  * **Informative:** Sections packed with valuable information or tips.
  * **Insightful:** Portions offering unique perspectives or 'aha\!' moments.

For each suggested short, please provide:

  * The **start time** of the initial segment and the **end time** of the final segment. The duration of each short should be no longer than 3 minutes, but **aim for durations between 15 seconds and 60 seconds**. However, the short **must be as long as necessary to capture the complete thought or idea**, even if it means exceeding the target range or extending slightly to capture all necessary dialogue.
  * A concise **title** that grabs attention.
  * A brief **description** highlighting the short's content and its appeal.
  * An **explanation** of why this particular segment is suitable for a YouTube Short, focusing on its potential for discoverability and engagement.

Please format your response as a JSON array of objects with the following structure:

```json
{
  ""title"": ""string"",
  "start_time": "string",
  "end_time": "string",
  "description": "string",
  "explanation": "string"
}
```

Here is the transcript of my YouTube live stream:

{{completeTranscription}}
""";

if (string.IsNullOrWhiteSpace(apiKey))
{
  throw new InvalidOperationException("GeminiApiKey is not configured in appsettings.json.");
}

using var client = new HttpClient()
{
  Timeout = TimeSpan.FromMinutes(30)
};

var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";
using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
{
  Content = new StringContent(
    JsonSerializer.Serialize(new
    {
      contents = new[]
      {
        new
        {
          role = "user",
          parts = new[]
          {
            new
            {
              text = prompt
            }
          },
        }
      },
      generationConfig = new
      {
        responseMimeType = "application/json",
      }
    }),
    Encoding.UTF8,
    "application/json"
  )
};
var response = await client.SendAsync(request);
var responseContent = await response.Content.ReadAsStringAsync();
var responseJson = JsonSerializer.Deserialize<LLMResponse>(responseContent);
var candidatesText = responseJson?
  .Candidates?
  .FirstOrDefault()?
  .Content
  .Parts?.FirstOrDefault()?
  .Text;
var analysis = JsonSerializer.Deserialize<List<LLMAnalysis>>(candidatesText ?? string.Empty);

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
  [property: JsonPropertyName("candidates")]
  Candidate[] Candidates
);

record Candidate(
  [property: JsonPropertyName("content")]
  Content Content
);

record Content(
  [property: JsonPropertyName("parts")]
  Part[] Parts
);

record Part(
  [property: JsonPropertyName("text")]
  string Text
);