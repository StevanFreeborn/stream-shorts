namespace StreamShorts.Console.Commands;

/// <summary>
/// The default command for processing video streams to create short clips.
/// </summary>
internal sealed class DefaultCommand(
  IFileSystem fileSystem,
  IAnsiConsole console,
  IAudioExtractor audioExtractor,
  ITranscriber transcriber,
  ITranscriptAnalyzer transcriptAnalyzer,
  IShortsCreator shortsCreator,
  TimeProvider timeProvider,
  IConfiguration appConfig,
  ILogger<DefaultCommand> logger
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
  private readonly IConfiguration _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
  private readonly ILogger<DefaultCommand> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

  /// <summary>
  /// Represents the settings for the default command.
  /// </summary>
  internal class Settings : CommandSettings
  {
    /// <summary>
    /// Gets or sets the path to the stream.
    /// </summary>
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
      return ValidationResult.Error($"The specified stream file '{settings.Stream.EscapeMarkup()}' does not exist.");
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
    _console.MarkupLine($"[blue]Processing stream:[/] {settings.Stream.EscapeMarkup()}");
    var videoStream = (FileStream)_fileSystem.File.OpenRead(settings.Stream);

    var now = _timeProvider.GetUtcNow();
    var inputFileName = _fileSystem.Path.GetFileNameWithoutExtension(settings.Stream);
    var baseDirectory = ValidateAndGetBaseOutputDirectory();
    var artifactsOutputDirectory = $"{now:yyyy_MM_dd_HH_mm_ss}_{inputFileName}";
    var outputDirectoryPath = _fileSystem.Path.Combine(baseDirectory, artifactsOutputDirectory);

    _fileSystem.Directory.CreateDirectory(outputDirectoryPath);

    using var audioStream = new FileStream(
      _fileSystem.Path.Combine(outputDirectoryPath, $"{inputFileName}.mp3"),
      FileMode.Create,
      FileAccess.ReadWrite,
      FileShare.ReadWrite,
      4096,
      FileOptions.Asynchronous
    );

    await _console.Status()
      .Spinner(Spinner.Known.Dots)
      .StartAsync("Extracting audio...", async _ => await _audioExtractor.ExtractMp3FromMp4Async(videoStream, audioStream));

    _console.MarkupLine($"[blue]Audio extracted[/] [green]successfully![/]");

    List<TranscriptionSegment> transcriptionSegments = [];

    await _console.Status()
      .Spinner(Spinner.Known.Dots)
      .StartAsync("Transcribing audio...", async ctx =>
      {
        await foreach (var segment in _transcriber.TranscribeAsync(audioStream))
        {
          transcriptionSegments.Add(segment);
          var segmentTimeText = $@"[{segment.StartTime:hh\:mm\:ss} - {segment.EndTime:hh\:mm\:ss}]";
          ctx.Status($"Transcribed segment {segmentTimeText.EscapeMarkup()}");
        }
      });

    _console.MarkupLine($"[blue]Transcription completed[/] [green]successfully![/]");

    await _fileSystem.File.WriteAllTextAsync(
      _fileSystem.Path.Combine(outputDirectoryPath, "transcription.txt"),
      string.Join(Environment.NewLine, transcriptionSegments)
    );

    TranscriptAnalysis? analysis = null;

    await _console.Status()
      .Spinner(Spinner.Known.Dots)
      .StartAsync("Analyzing transcript...", async _ => analysis = await _transcriptAnalyzer.AnalyzeAsync(transcriptionSegments));

    if (analysis is null)
    {
      _console.MarkupLine("[red]Failed[/] to analyze transcript.");
      return (int)ExitCode.FailedToAnalyzeTranscript;
    }

    _console.MarkupLine($"[blue]Transcript analysis completed[/] [green]successfully![/]");

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

    var directoryUri = new Uri(outputDirectoryPath).AbsoluteUri;
    var panel = new Panel($"[blue link={directoryUri}]{artifactsOutputDirectory.EscapeMarkup()}[/]")
    {
      Header = new PanelHeader($"[blue]Shorts created[/] [green]successfully![/]")
    };

    _console.Write(panel);

    return (int)ExitCode.SuccessFullyProcessedStream;
  }

  private string ValidateAndGetBaseOutputDirectory()
  {
    var baseDirectory = _appConfig.GetValue<string>("OutputDirectory");

    if (string.IsNullOrWhiteSpace(baseDirectory))
    {
      return AppContext.BaseDirectory;
    }

    try
    {
      var fullPath = _fileSystem.Path.GetFullPath(baseDirectory);

      if (_fileSystem.Directory.Exists(fullPath) is false)
      {
        _fileSystem.Directory.CreateDirectory(fullPath);
      }

      return fullPath;
    }
    catch (Exception ex) when (
      ex is ArgumentException
        or NotSupportedException
        or PathTooLongException
        or DirectoryNotFoundException
        or UnauthorizedAccessException
    )
    {
      _logger.LogWarning(ex, "The configured output directory '{BaseDirectory}' is invalid. Defaulting to application base directory.", baseDirectory);
      return AppContext.BaseDirectory;
    }
  }

  private enum ExitCode
  {
    FailedToAnalyzeTranscript,
    SuccessFullyProcessedStream,
  }
}