
namespace StreamShorts.Library.Media;

/// <summary>
/// Represents a service for processing audio files.
/// </summary>
internal interface IAudioService
{
  /// <summary>
  /// Converts an MP3 stream to a WAV stream with a 16 kHz sample rate.
  /// </summary>
  /// <param name="mp3">The input MP3 stream.</param>
  /// <returns>The output WAV stream.</returns>
  Stream ConvertMp3ToWav16(Stream mp3);

  /// <summary>
  /// Gets the number of segments in a WAV stream based on the specified segment duration.
  /// </summary>
  /// param name="wavStream">The input WAV stream.</param>
  /// <param name="segmentDuration">The duration of each segment.</param>
  /// <returns>The number of segments.</returns>
  int GetNumberOfWavSegments(Stream wavStream, TimeSpan segmentDuration);

  /// <summary>
  /// Gets a segment of a WAV stream based on the specified segment number and duration.
  /// </summary>
  /// param name="wavStream">The input WAV stream.</param>
  /// <param name="segmentNumber">The segment number to retrieve.</param>
  /// <param name="segmentDuration">The duration of each segment.</param>
  /// <returns>The segment stream.</returns>
  Stream GetWavSegment(Stream wavStream, int segmentNumber, TimeSpan segmentDuration);
}