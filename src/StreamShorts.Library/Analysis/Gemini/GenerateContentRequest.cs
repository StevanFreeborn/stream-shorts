using System.Text.Json.Serialization;

namespace StreamShorts.Library.Analysis.Gemini;

/// <summary>
/// Represents a request to generate content using Gemini.
/// </summary>
/// <param name="Contents">The content array for the request.</param>
/// <param name="GenerationConfig">The generation configuration.</param>
internal record GenerateContentRequest(
  [property: JsonPropertyName("contents")]
  Content[] Contents,
  [property: JsonPropertyName("generationConfig")]
  GenerationConfig GenerationConfig
);

/// <summary>
/// Represents configuration for content generation.
/// </summary>
/// <param name="ResponseMimeType">The MIME type for the response.</param>
internal record GenerationConfig(
  [property: JsonPropertyName("responseMimeType")]
  string ResponseMimeType
);