namespace StreamShorts.Library.Transcription;

/// <summary>
/// Represents a segment of transcribed audio.
/// </summary>
/// <param name="StartTime">The start time of the segment.</param>
/// <param name="EndTime">The end time of the segment.</param>
/// <param name="Text">The transcribed text of the segment.</param>
public sealed record TranscriptionSegment(
  TimeSpan StartTime,
  TimeSpan EndTime,
  string Text
)
{
  public override string ToString()
  {
    return $"[{StartTime:hh\\:mm\\:ss} - {EndTime:hh\\:mm\\:ss}]: {Text}";
  }
}