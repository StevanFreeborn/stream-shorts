namespace StreamShorts.Library.Analysis;

public sealed class FailedTranscriptAnalysisException : Exception
{
  public FailedTranscriptAnalysisException()
  {
  }

  public FailedTranscriptAnalysisException(string message) : base(message)
  {
  }

  public FailedTranscriptAnalysisException(string message, Exception innerException) : base(message, innerException)
  {
  }
}
