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

  /// <summary>
  /// Creates a clip from a video file based on the specified start and end times.
  /// </summary>
  /// <param name="sourcePath">The path to the source video file.</param>
  /// <param name="destinationPath">The path where the created clip will be saved.</param>
  /// <param name="startTime">The start time of the clip.</param>
  /// <param name="endTime">The end time of the clip.</param>
  /// <param name="buffer">An optional buffer time to include before and after the clip segment.</param>
  /// <returns>A task that represents the asynchronous operation.</returns>
  Task CreateClipFromVideoAsync(
    string sourcePath,
    string destinationPath,
    TimeSpan startTime,
    TimeSpan endTime,
    TimeSpan? buffer = null
  );
}