using System.Diagnostics.CodeAnalysis;

using FFMpegCore;
using FFMpegCore.Enums;
using FFMpegCore.Pipes;

namespace StreamShorts.Library.Media;

[ExcludeFromCodeCoverage]
internal class FFMpegService : IFFMpegService
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
}