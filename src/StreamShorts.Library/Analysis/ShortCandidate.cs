using System.Text.Json.Serialization;

namespace StreamShorts.Library.Analysis;

/// <summary>
/// Represents a short candidate derived from a transcript.
/// </summary>
public record ShortCandidate(
  [property: JsonPropertyName("title")]
  string Title,
  [property: JsonPropertyName("description")]
  string Description,
  [property: JsonPropertyName("explanation")]
  string Explanation,
  [property: JsonPropertyName("start_time")]
  TimeSpan StartTime,
  [property: JsonPropertyName("end_time")]
  TimeSpan EndTime
);
