using StreamShorts.Library.Analysis;

namespace StreamShorts.Library.Media.Video;

/// <summary>
/// Represents a service that creates video shorts.
/// </summary>
/// <inheritdoc/>
public class ShortsCreator : IShortsCreator
{
  private readonly IVideoService _videoService = new FFMpegService();

  public ShortsCreator()
  {
  }

  internal ShortsCreator(IVideoService videoService)
  {
    _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService), $"{nameof(videoService)} cannot be null");
  }

  public async Task CreateShortAsync(string sourcePath, ShortCandidate candidate, string destinationPath, TimeSpan? buffer = null)
  {
    if (string.IsNullOrWhiteSpace(sourcePath))
    {
      throw new ArgumentNullException(nameof(sourcePath), $"{nameof(sourcePath)} cannot be null or whitespace");
    }
    
    if (candidate is null)
    {
      throw new ArgumentNullException(nameof(candidate), $"{nameof(candidate)} cannot be null");
    }
    
    if (string.IsNullOrWhiteSpace(destinationPath))
    {
      throw new ArgumentNullException(nameof(destinationPath), $"{nameof(destinationPath)} cannot be null or whitespace");
    }
    
    await _videoService.CreateClipFromVideoAsync(sourcePath, destinationPath, candidate.StartTime, candidate.EndTime, buffer)
      .ConfigureAwait(false);
  }
}