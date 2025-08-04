namespace StreamShorts.Library.Analysis;

/// <summary>
/// Represents the analysis of a transcript, containing short clips derived from the transcript.
/// </summary>
public sealed class TranscriptAnalysis(IEnumerable<ShortClip> shortClips)
{
  /// <summary>
  /// Gets the short clips derived from the transcript.
  /// </summary>
  public IEnumerable<ShortClip> ShortClips { get; init; } = shortClips;
}