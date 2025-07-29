namespace StreamShorts.Library.Media.Audio;

/// <summary>
/// Extracts audio from video files.
/// </summary>
/// <inheritdoc/>
public class AudioExtractor : IAudioExtractor
{
  private readonly IVideoService _videoService = new FFMpegService();

  /// <summary>
  /// Initializes a new instance of the <see cref="AudioExtractor"/> class.
  /// </summary>
  public AudioExtractor()
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="AudioExtractor"/> class with a specified FFMpeg service.
  /// </summary>
  /// <param name="videoService">The video service to use for audio extraction.</param>
  /// <exception cref="ArgumentNullException">Thrown when the FFMpeg service is null.</exception>
  internal AudioExtractor(IVideoService videoService)
  {
    _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService), $"{nameof(videoService)} cannot be null");
  }

  public async Task<Stream> ExtractMp3FromMp4Async(Stream video)
  {
    if (video is null)
    {
      throw new ArgumentNullException(nameof(video), "Video stream cannot be null");
    }

    if (video.CanRead is false)
    {
      throw new ArgumentException("Video stream must be readable", nameof(video));
    }

    if (video.CanSeek is false)
    {
      throw new ArgumentException("Video stream must be seekable", nameof(video));
    }

    var originalPosition = video.Position;

    try
    {
      var mp3Stream = new MemoryStream();
      using var mp4Stream = new MemoryStream();

      await video.CopyToAsync(mp4Stream).ConfigureAwait(false);
      mp4Stream.Position = 0;

      var wasExtracted = await _videoService.ExtractAudioFromVideoAsync(mp4Stream, mp3Stream).ConfigureAwait(false);

      if (wasExtracted is false)
      {
        throw new FailedAudioExtractionException("Failed to extract audio from the video stream.");
      }

      mp3Stream.Position = 0;
      return mp3Stream;
    }
    catch (Exception e) when (e is not FailedAudioExtractionException)
    {
      throw new FailedAudioExtractionException("Failed to extract audio", e);
    }
    finally
    {
      video.Position = originalPosition;
    }
  }
}