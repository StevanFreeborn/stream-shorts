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
    .ConfigureHostConfiguration(static config => config.AddJsonFile("appsettings.json"))
    .ConfigureServices(static (_, services) =>
    {
      services.AddHttpClient();
      services.AddSingleton(AnsiConsole.Console);
      services.AddSingleton<IFileSystem, FileSystem>();
      services.AddSingleton(TimeProvider.System);
      services.AddSingleton<IAudioExtractor, AudioExtractor>();
      services.AddSingleton<ITranscriber, WhisperTranscriber>();
      services.AddSingleton<ITranscriptAnalyzer, GeminiAnalyzer>(static sp =>
      {
        const string modelOptionName = "Model";
        const string keyOptionName = "ApiKey";
        var config = sp.GetRequiredService<IConfiguration>();
        var geminiSection = config.GetSection("Gemini");
        var key = geminiSection[keyOptionName];
        var model = geminiSection[modelOptionName];

        if (string.IsNullOrWhiteSpace(key))
        {
          throw new InvalidOperationException($"{keyOptionName} is not configured in appsettings.json.");
        }

        var clientFactory = sp.GetRequiredService<IHttpClientFactory>();

        return new GeminiAnalyzer(clientFactory, key, model);
      });
      services.AddSingleton<IShortsCreator, ShortsCreator>();
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