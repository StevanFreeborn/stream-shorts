using System.Globalization;
using System.Text;

using StreamShorts.Library.Transcription;

namespace StreamShorts.Library.Analysis.Prompts;

/// <summary>
/// Default implementation of the analysis prompt for generating YouTube Shorts.
/// </summary>
/// <inheritdoc/>
internal sealed class DefaultAnalysisPrompt : IAnalysisPrompt
{
  private static readonly CompositeFormat Prompt = CompositeFormat.Parse(@"
  I need your help to transform my YouTube live stream transcript into engaging YouTube Shorts. Act as my content editor and pinpoint **all potential candidate segments** that are perfect for short-form video. I'm looking for clips that are:
  
    - **Funny:** Moments that will make viewers laugh.
    - **Informative:** Sections packed with valuable information or tips.
    - **Insightful:** Portions offering unique perspectives or 'aha!' moments.

  For each suggested short, please provide:

    - The **start time** of the initial segment and the **end time** of the final segment. The duration of each short should be no longer than 3 minutes, but **aim for durations between 15 seconds and 60 seconds**. However, the short **must be as long as necessary to capture the complete thought or idea**, even if it means exceeding the target range or extending slightly to capture all necessary dialogue.
    - A concise **title** that grabs attention.
    - A brief **description** highlighting the short's content and its appeal.
    - An **explanation** of why this particular segment is suitable for a YouTube Short, focusing on its potential for discoverability and engagement.

  Please format your response as a JSON array of objects with the following structure:

  ```json
  {{
    ""title"": ""string"",
    ""start_time"": ""string"",
    ""end_time"": ""string"",
    ""description"": ""string"",
    ""explanation"": ""string""
  }}
  ```

  Here is the transcript of my YouTube live stream:

  {0}
  ");

  public string GetPrompt(IEnumerable<TranscriptionSegment> segments)
  {
    var transcript = string.Join(Environment.NewLine, segments);
    return string.Format(CultureInfo.InvariantCulture, Prompt, transcript);
  }
}