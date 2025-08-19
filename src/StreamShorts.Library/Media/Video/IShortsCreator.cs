using StreamShorts.Library.Analysis;

namespace StreamShorts.Library.Media.Video;

/// <summary>
/// Represents a service that creates video shorts.
/// </summary>
public interface IShortsCreator
{
  /// <summary>
  /// Creates a video short from the specified source video file based on the provided candidate details.
  /// </summary>
  /// <param name="sourcePath">The path to the source video file.</param>
  /// <param name="candidate">The details of the short candidate.</param>
  /// <param name="destinationPath">The path where the created short will be saved.</param>
  /// <param name="buffer">An optional buffer time to include before and after the short segment.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="sourcePath"/> or <paramref name="destinationPath"/> is null or whitespace.</exception>
  /// <exception cref="ArgumentNullException">Thrown when <paramref name="candidate"/> is null.</exception>
  public Task CreateShortAsync(string sourcePath, ShortCandidate candidate, string destinationPath, TimeSpan? buffer = null);
}