using System.Text.Json;

using StreamShorts.Library.Media.Video;

namespace StreamShorts.Console.Commands;

internal sealed class DefaultCommand(
  IFileSystem fileSystem,
  IAnsiConsole console,
  IAudioExtractor audioExtractor,
  ITranscriber transcriber,
  ITranscriptAnalyzer transcriptAnalyzer,
  IShortsCreator shortsCreator,
  TimeProvider timeProvider
) : AsyncCommand<DefaultCommand.Settings>
{
  private readonly JsonSerializerOptions _jsonSerializerOptions = new()
  {
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
  };
  private readonly IFileSystem _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
  private readonly IAnsiConsole _console = console ?? throw new ArgumentNullException(nameof(console));
  private readonly IAudioExtractor _audioExtractor = audioExtractor ?? throw new ArgumentNullException(nameof(audioExtractor));
  private readonly ITranscriber _transcriber = transcriber ?? throw new ArgumentNullException(nameof(transcriber));
  private readonly ITranscriptAnalyzer _transcriptAnalyzer = transcriptAnalyzer ?? throw new ArgumentNullException(nameof(transcriptAnalyzer));
  private readonly IShortsCreator _shortsCreator = shortsCreator ?? throw new ArgumentNullException(nameof(shortsCreator));
  private readonly TimeProvider _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

  internal class Settings : CommandSettings
  {
    [CommandArgument(0, "[Stream]")]
    [Description("The path to the stream")]
    public string Stream { get; init; } = string.Empty;
  }

  public override ValidationResult Validate(CommandContext context, Settings settings)
  {
    if (string.IsNullOrWhiteSpace(settings.Stream))
    {
      return ValidationResult.Error("Stream path must be provided.");
    }

    if (_fileSystem.File.Exists(settings.Stream) is false)
    {
      return ValidationResult.Error($"The specified stream file '{settings.Stream}' does not exist.");
    }

    var fileExtension = _fileSystem.Path.GetExtension(settings.Stream).ToUpperInvariant();

    if (fileExtension != ".MP4")
    {
      return ValidationResult.Error("The specified stream file must be an .mp4 file.");
    }

    return base.Validate(context, settings);
  }

  public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
  {
    _console.MarkupLine($"[blue]Processing stream:[/] {settings.Stream}");
    var videoStream = _fileSystem.File.OpenRead(settings.Stream);

    Stream? audioStream = null;

    await _console.Status()
      .Spinner(Spinner.Known.Dots)
      .StartAsync("Extracting audio...", async _ =>
      {
        audioStream = await _audioExtractor.ExtractMp3FromMp4Async(videoStream);
      });

    if (audioStream is null)
    {
      _console.MarkupLine("[red]Failed[/] to extract audio from the stream.");
      return (int)ExitCode.FailedToExtractAudio;
    }

    _console.MarkupLine($"[blue]Audio extracted[/] [green]successfully![/]");

    List<TranscriptionSegment> transcriptionSegments = [];

    await _console.Status()
      .Spinner(Spinner.Known.Dots)
      .StartAsync("Transcribing audio...", async ctx =>
      {
        await foreach (var segment in _transcriber.TranscribeAsync(audioStream))
        {
          transcriptionSegments.Add(segment);
          var segmentTimeText = $"[{segment.StartTime:hh\\:mm\\:ss} - {segment.EndTime:hh\\:mm\\:ss}]";
          ctx.Status($"Transcribed segment {segmentTimeText.EscapeMarkup()}");
        }
      });

    _console.MarkupLine($"[blue]Transcription completed[/] [green]successfully![/]");

    await _fileSystem.File.WriteAllTextAsync(
      _fileSystem.Path.Combine(AppContext.BaseDirectory, "transcription.txt"),
      string.Join(Environment.NewLine, transcriptionSegments)
    );

    TranscriptAnalysis? analysis = null;

    await _console.Status()
      .Spinner(Spinner.Known.Dots)
      .StartAsync("Analyzing transcript...", async _ =>
      {
        analysis = await _transcriptAnalyzer.AnalyzeAsync(transcriptionSegments);
      });

    if (analysis is null)
    {
      _console.MarkupLine("[red]Failed[/] to analyze transcript.");
      return (int)ExitCode.FailedToAnalyzeTranscript;
    }

    _console.MarkupLine($"[blue]Transcript analysis completed[/] [green]successfully![/]");

    var now = _timeProvider.GetUtcNow();
    var inputFileName = _fileSystem.Path.GetFileNameWithoutExtension(settings.Stream);
    var outputDirectoryPath = _fileSystem.Path.Combine(
      AppContext.BaseDirectory,
      $"{now:yyyy_MM_dd_HH_mm_ss}_{inputFileName}"
    );

    _fileSystem.Directory.CreateDirectory(outputDirectoryPath);

    await _fileSystem.File.WriteAllTextAsync(
      _fileSystem.Path.Combine(outputDirectoryPath, "analysis.json"),
      JsonSerializer.Serialize(analysis, _jsonSerializerOptions)
    );

    await _console.Status()
      .Spinner(Spinner.Known.Dots)
      .StartAsync("Creating shorts...", async ctx =>
      {
        foreach (var candidate in analysis.Candidates)
        {
          ctx.Status($"Creating short: {candidate.Title.EscapeMarkup()}");
          var safeFileName = string.Concat(candidate.Title.Split(_fileSystem.Path.GetInvalidFileNameChars()));
          var candidatePath = _fileSystem.Path.Combine(outputDirectoryPath, $"{safeFileName}.mp4");
          await _shortsCreator.CreateShortAsync(settings.Stream, candidate, candidatePath);
        }
      });

    _console.MarkupLine($"[blue]Shorts created[/] [green]successfully![/]");

    return 0;
  }

  private enum ExitCode
  {
    FailedToExtractAudio,
    FailedToAnalyzeTranscript,
  }
}