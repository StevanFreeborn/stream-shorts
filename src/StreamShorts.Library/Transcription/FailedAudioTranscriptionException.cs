namespace StreamShorts.Library.Transcription;

/// <summary>
/// Represents an error that occurs when audio transcription fails.
/// </summary>
public sealed class FailedAudioTranscriptionException : Exception
{
  /// <summary>
  /// Initializes a new instance of the <see cref="FailedAudioTranscriptionException"/> class with no parameters.
  /// </summary>
  public FailedAudioTranscriptionException()
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="FailedAudioTranscriptionException"/> class with a specified error message.
  /// </summary>
  /// <param name="message">The error message that explains the reason for the exception.</param>
  public FailedAudioTranscriptionException(string message) : base(message)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="FailedAudioTranscriptionException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
  /// </summary>
  /// <param name="message">The error message that explains the reason for the exception.</param>
  /// <param name="innerException">The exception that is the cause of the current exception.</param>
  public FailedAudioTranscriptionException(string message, Exception innerException) : base(message, innerException)
  {
  }
}