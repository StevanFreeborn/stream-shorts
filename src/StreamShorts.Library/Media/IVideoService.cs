namespace StreamShorts.Library.Media;

/// <summary>
/// Represents a service for processing video files.
/// </summary>
internal interface IVideoService
{
  /// <summary>
  /// Extracts audio from a video stream and writes it to an audio stream.
  /// </summary>
  /// <param name="video">The input video stream.</param>
  /// <param name="audio">The output audio stream.</param>
  /// <returns>A task that represents the asynchronous operation. The task result indicates whether the extraction was successful.</returns>
  Task<bool> ExtractAudioFromVideoAsync(Stream video, Stream audio);
}