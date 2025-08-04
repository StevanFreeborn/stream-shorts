using System.Text;
using System.Text.Json;

using StreamShorts.Library.Analysis.Prompts;
using StreamShorts.Library.Transcription;

namespace StreamShorts.Library.Analysis.Gemini;

/// <summary>
/// Represents an analyzer that uses Gemini to analyze transcript segments and generate short clips.
/// </summary>
/// <inheritdoc/>
public sealed class GeminiAnalyzer(
  IHttpClientFactory httpClientFactory,
  string apiKey
  ) : ITranscriptAnalyzer
{
  private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
  private readonly string _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
  private readonly IAnalysisPrompt _prompt = new DefaultAnalysisPrompt();

  public GeminiAnalyzer(
    IHttpClientFactory httpClientFactory,
    string apiKey,
    IAnalysisPrompt prompt
  ) : this(httpClientFactory, apiKey)
  {
    _prompt = prompt ?? throw new ArgumentNullException(nameof(prompt));
  }

  public async Task<TranscriptAnalysis> AnalyzeAsync(IEnumerable<TranscriptionSegment> segments)
  {
    using var client = _httpClientFactory.CreateClient();
    client.Timeout = TimeSpan.FromMinutes(5);

    var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={_apiKey}";
    var generateContentRequest = new GenerateContentRequest(
      [
        new Content(
          Role: "user",
          Parts:[ new Part(Text: _prompt.GetPrompt(segments)) ]
        )
      ],
      new GenerationConfig(ResponseMimeType: "application/json")
    );
    using var requestContent = new StringContent(
      JsonSerializer.Serialize(generateContentRequest),
      Encoding.UTF8,
      "application/json"
    );
    using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
    {
      Content = requestContent
    };

    var response = await client.SendAsync(request).ConfigureAwait(false);
    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    var responseJson = JsonSerializer.Deserialize<GenerateContentResponse>(responseContent);
    var candidatesText = responseJson?
      .Candidates?
      .FirstOrDefault()?
      .Content
      .Parts?.FirstOrDefault()?
      .Text;

    var clips = JsonSerializer.Deserialize<List<ShortClip>>(candidatesText ?? string.Empty);
    return new TranscriptAnalysis(clips ?? []);
  }
}



