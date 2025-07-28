namespace StreamShorts.Console.Commands;

internal class DefaultCommand(
  IFileSystem fileSystem,
  IAnsiConsole console,
  IAudioExtractor audioExtractor,
  IAudioConverter audioConverter
) : AsyncCommand<DefaultCommand.Settings>
{
  private readonly IFileSystem _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
  private readonly IAnsiConsole _console = console ?? throw new ArgumentNullException(nameof(console));
  private readonly IAudioExtractor _audioExtractor = audioExtractor ?? throw new ArgumentNullException(nameof(audioExtractor));
  private readonly IAudioConverter _audioConverter = audioConverter ?? throw new ArgumentNullException(nameof(audioConverter));

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
        audioStream = await _audioExtractor.ExtractMp3FromMp4Async(videoStream).ConfigureAwait(false);
      });

    if (audioStream is null)
    {
      _console.MarkupLine("[red]Failed[/] to extract audio from the stream.");
      return 1;
    }

    _console.MarkupLine($"[blue]Audio extracted[/] [green]successfully![/]");

    Stream? wavStream = null;

    _console.Status()
      .Spinner(Spinner.Known.Dots)
      .Start("Converting audio...", ctx =>
      {
        wavStream = _audioConverter.ConvertMp3ToWav16(audioStream);
      });

    if (wavStream is null)
    {
      _console.MarkupLine("[red]Failed[/] to convert audio to WAV format.");
      return 2;
    }

    _console.MarkupLine($"[blue]Audio converted[/] [green]successfully![/]");

    return 0;
  }
}