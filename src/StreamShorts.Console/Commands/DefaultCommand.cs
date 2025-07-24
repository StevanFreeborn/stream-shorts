namespace StreamShorts.Console.Commands;

internal class DefaultCommand(
  IFileSystem fileSystem,
  IAnsiConsole console
) : AsyncCommand<DefaultCommand.Settings>
{
  private readonly IFileSystem _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
  private readonly IAnsiConsole _console = console ?? throw new ArgumentNullException(nameof(console));

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

  public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
  {
    _console.Write($"[green]Processing stream:[/] {settings.Stream}");
    return Task.FromResult(0);
  }
}