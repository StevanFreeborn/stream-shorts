using System.Text.Json.Serialization;

namespace StreamShorts.Library.Analysis.Gemini;

internal record GenerateContentResponse(
  [property: JsonPropertyName("candidates")]
  Candidate[] Candidates
);

internal record Candidate(
  [property: JsonPropertyName("content")]
  Content Content
);

