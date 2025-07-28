
namespace StreamShorts.Library.Transcription;

public class WhisperTranscriber : ITranscriber
{
  public IAsyncEnumerable<TranscriptionSegment> TranscribeAsync(Stream audio)
  {
    throw new NotImplementedException();
  }
}