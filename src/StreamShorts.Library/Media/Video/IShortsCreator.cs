using StreamShorts.Library.Analysis;

namespace StreamShorts.Library.Media.Video;

/// <summary>
/// Represents a service that creates video shorts.
/// </summary>
public interface IShortsCreator
{
  /// <summary>
  /// Creates video shorts from the provided transcript analysis and video stream.
  /// </summary>
  /// <param name="analysis">The transcript analysis.</param>
  /// <param name="video">The video stream.</param>
  /// <param name="buffer">An optional buffer duration to include before the start time and after the end time of each short.</param>
  /// <returns>An asynchronous enumerable of <see cref="ShortClip"/>.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="analysis"/> is null.</exception>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="video"/> is null.</exception>
  public IAsyncEnumerable<ShortClip> CreateShortsAsync(TranscriptAnalysis analysis, Stream video, TimeSpan? buffer = null);
}