namespace StreamShorts.Library.Media.Audio;

/// <summary>
/// Represents an interface for extracting audio from video streams.
/// </summary>
public interface IAudioExtractor
{
  /// <summary>
  /// Extracts MP3 audio from an MP4 video stream.
  /// </summary>
  /// <param name="video">The input video stream.</param>
  /// <param name="audio">The output audio stream where the extracted MP3 will be written.</param>
  /// <returns>A stream containing the extracted MP3 audio.</returns>
  /// <exception cref="ArgumentNullException">Thrown when the video stream is null.</exception>
  /// <exception cref="ArgumentException">Thrown when the video stream is not readable.</exception>
  /// <exception cref="ArgumentException">Thrown when the video stream is not seekable.</exception>
  /// <exception cref="ArgumentNullException">Thrown when the audio stream is null.</exception>
  /// <exception cref="ArgumentException">Thrown when the audio stream is not writable.</exception>
  /// <exception cref="ArgumentException">Thrown when the audio stream is not seekable.</exception>
  /// <exception cref="FailedAudioExtractionException">Thrown when the audio extraction fails.</exception>
  /// <remarks>The method will preserve the passed video stream's data and position.</remarks>
  Task<Stream> ExtractMp3FromMp4Async(Stream video, Stream audio);
}