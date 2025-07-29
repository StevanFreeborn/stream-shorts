using System.Runtime.CompilerServices;

using StreamShorts.Library.Media;

using Whisper.net;
using Whisper.net.Ggml;

namespace StreamShorts.Library.Transcription;

/// <summary>
/// Represents a transcriber that uses Whisper for audio transcription.
/// </summary>
/// <inheritdoc/>
public sealed class WhisperTranscriber : ITranscriber, IDisposable
{
  private readonly IAudioService _audioService = new NAudioService();
  private WhisperProcessor? _whisperProcessor;

  /// <summary>
  /// Initializes a new instance of the <see cref="WhisperTranscriber"/> class.
  /// </summary>
  public WhisperTranscriber()
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="WhisperTranscriber"/> class with a specified audio service.
  /// </summary>
  /// <param name="audioService">The audio service to use for audio processing.</param>
  /// <exception cref="ArgumentNullException">Thrown when the audio service is null.</exception
  internal WhisperTranscriber(IAudioService audioService)
  {
    _audioService = audioService ?? throw new ArgumentNullException(nameof(audioService), $"{nameof(audioService)} cannot be null");
  }

  public async IAsyncEnumerable<TranscriptionSegment> TranscribeAsync(Stream audio, [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    var segmentDuration = TimeSpan.FromMinutes(2);
    var wavStream = _audioService.ConvertMp3ToWav16(audio);
    var numberOfSegments = _audioService.GetNumberOfWavSegments(wavStream, segmentDuration);

    foreach (var segmentNumber in Enumerable.Range(0, numberOfSegments))
    {
      var segmentStream = _audioService.GetWavSegment(wavStream, segmentNumber, segmentDuration);
      var durationOffset = TimeSpan.FromMilliseconds(segmentNumber * segmentDuration.TotalMilliseconds);

      await foreach (var result in ProcessSegmentAsync(segmentStream, cancellationToken).ConfigureAwait(false))
      {
        yield return new TranscriptionSegment(
          result.Start + durationOffset,
          result.End + durationOffset,
          result.Text
        );
      }
    }
  }

  private async IAsyncEnumerable<SegmentData> ProcessSegmentAsync(
    Stream segmentStream,
    [EnumeratorCancellation] CancellationToken cancellationToken)
  {
    if (_whisperProcessor is null)
    {

      using var modelMemoryStream = new MemoryStream();
      var model = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(GgmlType.TinyEn, cancellationToken: cancellationToken).ConfigureAwait(false);
      await model.CopyToAsync(modelMemoryStream, cancellationToken).ConfigureAwait(false);
      var whisperFactory = WhisperFactory.FromBuffer(modelMemoryStream.ToArray());
      _whisperProcessor = whisperFactory.CreateBuilder()
        .WithLanguage("en")
        .Build();
    }

    await foreach (var result in _whisperProcessor.ProcessAsync(segmentStream, cancellationToken).ConfigureAwait(false))
    {
      yield return result;
    }
  }

  public void Dispose()
  {
    _whisperProcessor?.Dispose();
    _whisperProcessor = null;
  }
}