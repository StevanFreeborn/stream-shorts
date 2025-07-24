namespace StreamShorts.Console.Hosting;

internal static class HostBuilderExtensions
{
  public static CommandApp<DefaultCommand> BuildApp(this IHostBuilder builder)
  {
    var registrar = new TypeRegistrar(builder);
    var app = new CommandApp<DefaultCommand>(registrar);

    app.Configure(static c =>
      c.SetExceptionHandler(static (ex, resolver) =>
      {
        var console = resolver?.Resolve(typeof(IAnsiConsole)) as IAnsiConsole;
        console?.WriteLine($"[red]An error occurred while executing the command:[/]");
        console?.WriteException(ex, ExceptionFormats.ShortenEverything);
      })
    );

    return app;
  }
}