using System.Text.Json.Serialization;

/// <summary>
/// Represents content with role and parts for Gemini API.
/// </summary>
/// <param name="Role">The role of the content (e.g., "user", "assistant").</param>
/// <param name="Parts">The array of content parts.</param>
internal record Content(
  [property: JsonPropertyName("role")]
  string Role,
  [property: JsonPropertyName("parts")]
  Part[] Parts
);

/// <summary>
/// Represents a part of content for Gemini API.
/// </summary>
/// <param name="Text">The text content of the part.</param>
internal record Part(
  [property: JsonPropertyName("text")]
  string Text
);