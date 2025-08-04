using System.Text.Json.Serialization;

namespace StreamShorts.Library.Analysis.Gemini;

internal record GenerateContentRequest(
  [property: JsonPropertyName("contents")]
  Content[] Contents,
  [property: JsonPropertyName("generationConfig")]
  GenerationConfig GenerationConfig
);

internal record GenerationConfig(
  [property: JsonPropertyName("responseMimeType")]
  string ResponseMimeType
);