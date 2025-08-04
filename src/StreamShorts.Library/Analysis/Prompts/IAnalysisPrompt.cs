using StreamShorts.Library.Transcription;

namespace StreamShorts.Library.Analysis.Prompts;

/// <summary>
/// Defines the contract for analysis prompts used in transcript analysis.
/// </summary>
public interface IAnalysisPrompt
{
  /// <summary>
  /// Generates a prompt based on the provided transcript segments.
  /// </summary>
  /// <param name="transcript">The transcript segments to analyze.</param>
  /// <returns>A formatted prompt string for analysis.</returns>
  string GetPrompt(IEnumerable<TranscriptionSegment> transcript);
}