using System.Text.Json.Serialization;

internal record Content(
  [property: JsonPropertyName("role")]
  string Role,
  [property: JsonPropertyName("parts")]
  Part[] Parts
);

internal record Part(
  [property: JsonPropertyName("text")]
  string Text
);