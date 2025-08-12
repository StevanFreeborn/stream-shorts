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

  public async IAsyncEnumerable<ShortClip> CreateShortsAsync(TranscriptAnalysis analysis, Stream video, TimeSpan? buffer = null)
  {
    if (analysis is null)
    {
      throw new ArgumentNullException(nameof(analysis), $"{nameof(analysis)} cannot be null");
    }

    if (video is null)
    {
      throw new ArgumentNullException(nameof(video), $"{nameof(video)} cannot be null");
    }

    foreach (var candidate in analysis.Candidates)
    {
      var clip = await _videoService.ExtractClipFromVideoAsync(video, candidate.StartTime, candidate.EndTime, buffer)
        .ConfigureAwait(false);

      yield return new ShortClip(
        candidate,
        clip
      );
    }
  }
}