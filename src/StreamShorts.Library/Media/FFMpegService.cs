
using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;

namespace StreamShorts.Library.Media;

/// <summary>
/// Represents a service for processing video files using FFMpeg.
/// </summary>
/// <inheritdoc/>
internal sealed class FFMpegService : IVideoService
{
  public async Task<bool> ExtractAudioFromVideoAsync(Stream video, Stream audio)
  {
    return await FFMpegArguments
      .FromPipeInput(new StreamPipeSource(video))
      .OutputToPipe(
        new StreamPipeSink(audio),
        static o => o.DisableChannel(Channel.Video).ForceFormat("mp3")
      )
      .ProcessAsynchronously()
      .ConfigureAwait(false);
  }

  public async Task<Stream> ExtractClipFromVideoAsync(Stream video, TimeSpan start, TimeSpan end, TimeSpan? buffer = null)
  {
    var startTime = start - (buffer ?? TimeSpan.Zero);
    var endTime = end + (buffer ?? TimeSpan.Zero);
    var outputStream = new MemoryStream();

    await FFMpegArguments
      .FromPipeInput(new StreamPipeSource(video), o => o.Seek(startTime).EndSeek(endTime))
      .OutputToPipe(new StreamPipeSink(outputStream), o => o.CopyChannel().ForceFormat("webm"))
      .ProcessAsynchronously()
      .ConfigureAwait(false);

    outputStream.Position = 0;
    return outputStream;
  }
}