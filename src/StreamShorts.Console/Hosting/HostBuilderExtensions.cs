namespace StreamShorts.Console.Hosting;

/// <summary>
/// Provides extension methods for building command applications from host builders.
/// </summary>
internal static class HostBuilderExtensions
{
  /// <summary>
  /// Builds a command application from the host builder.
  /// </summary>
  /// <param name="builder">The host builder.</param>
  /// <returns>A configured command application.</returns>
  public static CommandApp<DefaultCommand> BuildApp(this IHostBuilder builder)
  {
    var registrar = new TypeRegistrar(builder);
    var app = new CommandApp<DefaultCommand>(registrar);

    app.Configure(static c =>
      c.SetExceptionHandler(static (ex, resolver) =>
      {
        var console = resolver?.Resolve(typeof(IAnsiConsole)) as IAnsiConsole ?? AnsiConsole.Console;
        console.MarkupLine($"[red]An error occurred while executing the command:[/]");
        console.WriteException(ex, ExceptionFormats.ShortenEverything);
      })
    );

    return app;
  }
}