namespace StreamShorts.Console.Commands;

internal sealed class DefaultCommand(
  IFileSystem fileSystem,
  IAnsiConsole console,
  IAudioExtractor audioExtractor,
  ITranscriber transcriber
) : AsyncCommand<DefaultCommand.Settings>
{
  private readonly IFileSystem _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
  private readonly IAnsiConsole _console = console ?? throw new ArgumentNullException(nameof(console));
  private readonly IAudioExtractor _audioExtractor = audioExtractor ?? throw new ArgumentNullException(nameof(audioExtractor));
  private readonly ITranscriber _transcriber = transcriber ?? throw new ArgumentNullException(nameof(transcriber));

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
      .StartAsync("Extracting audio...", async ctx =>
      {
        audioStream = await _audioExtractor.ExtractMp3FromMp4Async(videoStream);
      });

    if (audioStream is null)
    {
      _console.MarkupLine("[red]Failed[/] to extract audio from the stream.");
      return 1;
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

    return 0;
  }
}