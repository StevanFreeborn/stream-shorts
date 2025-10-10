namespace StreamShorts.Library.Media.Audio;

/// <summary>
/// Extracts audio from video files.
/// </summary>
/// <inheritdoc/>
public sealed class AudioExtractor : IAudioExtractor
{
  private readonly IVideoService _videoService = new FFMpegService();

  /// <summary>
  /// Initializes a new instance of the <see cref="AudioExtractor"/> class.
  /// </summary>
  public AudioExtractor()
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="AudioExtractor"/> class with a specified <see cref="IVideoService"/>.
  /// </summary>
  /// <param name="videoService">The video service to use for audio extraction.</param>
  /// <exception cref="ArgumentNullException">Thrown when the video service is null.</exception>
  internal AudioExtractor(IVideoService videoService)
  {
    _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService), $"{nameof(videoService)} cannot be null");
  }

  public async Task<Stream> ExtractMp3FromMp4Async(Stream video, Stream audio)
  {
    if (video is null)
    {
      throw new ArgumentNullException(nameof(video), "Video stream cannot be null");
    }

    if (IsVideoStreamUsable(video) is false)
    {
      throw new ArgumentException("Stream must be readable and seekable", nameof(video));
    }

    if (audio is null)
    {
      throw new ArgumentNullException(nameof(audio), "Audio stream cannot be null");
    }

    if (IsAudioStreamUsable(audio) is false)
    {
      throw new ArgumentException("Stream must be writable and seekable", nameof(audio));
    }

    var originalPosition = video.Position;

    try
    {
      var wasExtracted = await _videoService.ExtractAudioFromVideoAsync(video, audio).ConfigureAwait(false);

      if (wasExtracted is false)
      {
        throw new FailedAudioExtractionException("Failed to extract audio from the video stream.");
      }

      audio.Position = 0;
      return audio;
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

  private static bool IsVideoStreamUsable(Stream stream)
  {
    return stream.CanRead && stream.CanSeek;
  }

  private static bool IsAudioStreamUsable(Stream stream)
  {
    return stream.CanWrite && stream.CanSeek;
  }
}