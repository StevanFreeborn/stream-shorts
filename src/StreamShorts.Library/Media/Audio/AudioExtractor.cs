using FFMpegCore;
using FFMpegCore.Pipes;

using Channel = FFMpegCore.Enums.Channel;

namespace StreamShorts.Library.Media.Audio;

/// <inheritdoc/>
public class AudioExtractor : IAudioExtractor
{
  public async Task<Stream> ExtractMp3FromMp4Async(Stream video)
  {
    if (video is null)
    {
      throw new ArgumentNullException(nameof(video), "Video stream cannot be null");
    }

    if (video.CanRead is false)
    {
      throw new ArgumentException("Video stream must be readable", nameof(video));
    }

    if (video.CanSeek is false)
    {
      throw new ArgumentException("Video stream must be seekable", nameof(video));
    }

    var originalPosition = video.Position;

    try
    {
      using var mp3Stream = new MemoryStream();
      using var mp4Stream = new MemoryStream();

      await video.CopyToAsync(mp4Stream).ConfigureAwait(false);
      mp4Stream.Position = 0;

      var wasExtracted = await FFMpegArguments
        .FromPipeInput(new StreamPipeSource(mp4Stream))
        .OutputToPipe(
          new StreamPipeSink(mp3Stream),
          o => o.DisableChannel(Channel.Video).ForceFormat("mp3")
        )
        .ProcessAsynchronously()
        .ConfigureAwait(false);

      if (wasExtracted is false)
      {
        throw new FailedAudioExtractionException("Failed to extract audio from the video stream.");
      }

      return mp3Stream;
    }
    catch (Exception e) when (e is not FailedAudioExtractionException)
    {
      throw new FailedAudioExtractionException("Failed to extract audio", e);
    }
    finally
    {
      video.Position = originalPosition;
    }
  }
}