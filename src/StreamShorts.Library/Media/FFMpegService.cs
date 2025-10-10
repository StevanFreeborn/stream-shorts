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
    var inputArguments = video is FileStream videoFileStream
      ? FFMpegArguments.FromFileInput(videoFileStream.Name)
      : FFMpegArguments.FromPipeInput(new StreamPipeSource(video));

    Action<FFMpegArgumentOptions> arguments = static o => o.DisableChannel(Channel.Video).ForceFormat("mp3");

    var processor = audio is FileStream audioFileStream
      ? inputArguments.OutputToFile(audioFileStream.Name, addArguments: arguments)
      : inputArguments.OutputToPipe(new StreamPipeSink(audio), addArguments: arguments);
    
    return await processor.ProcessAsynchronously().ConfigureAwait(false);
  }

  public async Task CreateClipFromVideoAsync(
    string sourcePath,
    string destinationPath,
    TimeSpan startTime,
    TimeSpan endTime,
    TimeSpan? buffer = null
  )
  {
    var start = startTime - (buffer ?? TimeSpan.Zero);
    var end = endTime + (buffer ?? TimeSpan.Zero);

    await FFMpeg.SubVideoAsync(sourcePath, destinationPath, start, end)
      .ConfigureAwait(false);
  }
}