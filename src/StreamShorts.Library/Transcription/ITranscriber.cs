namespace StreamShorts.Library.Transcription;

/// <summary>
/// Represents a transcriber interface for audio transcription.
/// </summary>
public interface ITranscriber
{
  /// <summary>
  /// Transcribes the audio stream into text segments.
  /// </summary>
  /// <param name="audio">The audio stream to transcribe.</param>
  /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
  /// <returns>An <see cref="IAsyncEnumerable{T}"/> where T is <see cref="TranscriptionSegment"/>.</returns>
  IAsyncEnumerable<TranscriptionSegment> TranscribeAsync(Stream audio, CancellationToken cancellationToken = default);
}