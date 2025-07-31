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
      services.AddSingleton<IAudioExtractor, AudioExtractor>();
      services.AddSingleton<ITranscriber, WhisperTranscriber>();
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

// if (string.IsNullOrWhiteSpace(apiKey))
// {
//   throw new InvalidOperationException("GeminiApiKey is not configured in appsettings.json.");
// }

// using var client = new HttpClient()
// {
//   Timeout = TimeSpan.FromMinutes(30)
// };

// var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={apiKey}";
// using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
// {
//   Content = new StringContent(
//     JsonSerializer.Serialize(new
//     {
//       contents = new[]
//       {
//         new
//         {
//           role = "user",
//           parts = new[]
//           {
//             new
//             {
//               text = prompt
//             }
//           },
//         }
//       },
//       generationConfig = new
//       {
//         responseMimeType = "application/json",
//       }
//     }),
//     Encoding.UTF8,
//     "application/json"
//   )
// };
// var response = await client.SendAsync(request);
// var responseContent = await response.Content.ReadAsStringAsync();
// var responseJson = JsonSerializer.Deserialize<LLMResponse>(responseContent);
// var candidatesText = responseJson?
//   .Candidates?
//   .FirstOrDefault()?
//   .Content
//   .Parts?.FirstOrDefault()?
//   .Text;
// var analysis = JsonSerializer.Deserialize<List<LLMAnalysis>>(candidatesText ?? string.Empty);

// if (analysis is null)
// {
//   Console.WriteLine(resourceManager.GetString("LLMAnalysisFailed", CultureInfo.CurrentCulture));
//   return;
// }

// foreach (var result in analysis)
// {
//   var fileName = string.Concat(result.Title.Split(Path.GetInvalidFileNameChars()));
//   await FFMpeg.SubVideoAsync(
//     args[0],
//     $"{fileName}.mp4",
//     result.StartTime,
//     result.EndTime
//   );
// }

// // Step 6: Use analysis to generate a short video

// record LLMAnalysis(
//   [property: JsonPropertyName("title")]
//   string Title,
//   [property: JsonPropertyName("description")]
//   string Description,
//   [property: JsonPropertyName("explanation")]
//   string Explanation,
//   [property: JsonPropertyName("start_time")]
//   TimeSpan StartTime,
//   [property: JsonPropertyName("end_time")]
//   TimeSpan EndTime
// );

// record LLMResponse(
//   [property: JsonPropertyName("candidates")]
//   Candidate[] Candidates
// );

// record Candidate(
//   [property: JsonPropertyName("content")]
//   Content Content
// );

// record Content(
//   [property: JsonPropertyName("parts")]
//   Part[] Parts
// );

// record Part(
//   [property: JsonPropertyName("text")]
//   string Text
// );