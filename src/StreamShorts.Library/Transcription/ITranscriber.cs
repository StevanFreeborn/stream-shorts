namespace StreamShorts.Library.Transcription;

public interface ITranscriber
{
  IAsyncEnumerable<TranscriptionSegment> TranscribeAsync(Stream audio);
}

public record TranscriptionSegment(
  TimeSpan StartTime,
  TimeSpan EndTime,
  string Text
);