using StreamShorts.Library.Analysis;

namespace StreamShorts.Library.Media.Video;

/// <summary>
/// Represents a short video clip.
/// </summary>
public record ShortClip(
  ShortCandidate Candidate,
  Stream Segment
);