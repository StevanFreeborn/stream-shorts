namespace StreamShorts.Library.Transcription;

public sealed class FailedAudioTranscriptionException : Exception
{
  public FailedAudioTranscriptionException()
  {
  }

  public FailedAudioTranscriptionException(string message) : base(message)
  {
  }

  public FailedAudioTranscriptionException(string message, Exception innerException) : base(message, innerException)
  {
  }
}