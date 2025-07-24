namespace StreamShorts.Library.Media.Audio;

public class FailedAudioExtractionException : Exception
{
  public FailedAudioExtractionException() : base()
  {
  }

  public FailedAudioExtractionException(string message) : base(message)
  {
  }

  public FailedAudioExtractionException(string message, Exception innerException) : base(message, innerException)
  {
  }
}