using StreamShorts.Library.Transcription;

namespace StreamShorts.Library.Analysis;

/// <summary>
/// Defines the contract for transcript analyzers that process segments of a transcript and produce an analysis result containing short clips.
/// </summary>
public interface ITranscriptAnalyzer
{
  /// <summary>
  /// Analyzes the provided transcript segments and generates a transcript analysis result.
  /// </summary>
  /// <param name="segments">The transcript segments to analyze.</param>
  /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="TranscriptAnalysis"/> containing the short clips derived from the transcript.</returns>
  /// <exception cref="FailedTranscriptAnalysisException">Thrown when the analysis fails due to an error.</exception>
  Task<TranscriptAnalysis> AnalyzeAsync(IEnumerable<TranscriptionSegment> segments);
}