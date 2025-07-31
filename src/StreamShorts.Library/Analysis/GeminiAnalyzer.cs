using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using NAudio.CoreAudioApi;

using StreamShorts.Library.Transcription;

namespace StreamShorts.Library.Analysis;

public sealed class GeminiAnalyzer(
  IHttpClientFactory httpClientFactory,
  string apiKey
) : ITranscriptAnalyzer
{
  private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
  private readonly string _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));

  public async Task<TranscriptAnalysis> AnalyzeAsync(IEnumerable<TranscriptionSegment> segments)
  {
    using var client = _httpClientFactory.CreateClient();
    client.Timeout = TimeSpan.FromMinutes(5);

    // TODO: Load prompt from resource file
    var requestUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={_apiKey}";
    using var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
    {
      Content = new StringContent(
        JsonSerializer.Serialize(new
        {
          contents = new[]
          {
            new
            {
              role = "user",
              parts = new[]
              {
                new
                {
                  text = prompt
                }
              },
            }
          },
          generationConfig = new
          {
            responseMimeType = "application/json",
          }
        }),
        Encoding.UTF8,
        "application/json"
      )
    };
    var response = await client.SendAsync(request).ConfigureAwait(false);
    var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
    var responseJson = JsonSerializer.Deserialize<LLMResponse>(responseContent);
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

// TODO: Sort this shit out
record LLMResponse(
  [property: JsonPropertyName("candidates")]
  Candidate[] Candidates
);

record Candidate(
  [property: JsonPropertyName("content")]
  Content Content
);

record Content(
  [property: JsonPropertyName("parts")]
  Part[] Parts
);

record Part(
  [property: JsonPropertyName("text")]
  string Text
);
