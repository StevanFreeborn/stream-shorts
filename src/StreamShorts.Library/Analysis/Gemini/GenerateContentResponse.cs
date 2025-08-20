using System.Text.Json.Serialization;

namespace StreamShorts.Library.Analysis.Gemini;

/// <summary>
/// Represents a response from the Gemini content generation API.
/// </summary>
/// <param name="Candidates">The array of candidate responses.</param>
internal record GenerateContentResponse(
  [property: JsonPropertyName("candidates")]
  Candidate[] Candidates
);

/// <summary>
/// Represents a candidate response from content generation.
/// </summary>
/// <param name="Content">The generated content.</param>
internal record Candidate(
  [property: JsonPropertyName("content")]
  Content Content
);