namespace StreamShorts.Library.Analysis;

public class TranscriptAnalysis(IEnumerable<ShortClip> shortClips)
{
  public IEnumerable<ShortClip> ShortClips { get; init; } = shortClips;
}