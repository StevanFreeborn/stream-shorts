using StreamShorts.Library.Transcription;

namespace StreamShorts.Library.Analysis;

public interface ITranscriptAnalyzer
{
  Task<TranscriptAnalysis> AnalyzeAsync(IEnumerable<TranscriptionSegment> segments);
}