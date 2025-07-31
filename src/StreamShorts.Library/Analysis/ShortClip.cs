using System.Text.Json.Serialization;

namespace StreamShorts.Library.Analysis;

public record ShortClip(
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
