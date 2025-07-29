
using NAudio.Wave;

namespace StreamShorts.Library.Media;

/// <summary>
/// Represents a service for processing audio files using NAudio.
/// </summary>
/// <inheritdoc/>
internal sealed class NAudioService : IAudioService
{
  public Stream ConvertMp3ToWav16(Stream mp3)
  {
    return UseStream(mp3, stream =>
    {
      using var reader = new Mp3FileReader(mp3);
      var outFormat = new WaveFormat(16000, reader.WaveFormat.Channels);
      using var resampler = new MediaFoundationResampler(reader, outFormat);
      var waveStream = new MemoryStream();
      WaveFileWriter.WriteWavFileToStream(waveStream, resampler);
      waveStream.Position = 0;
      return waveStream;
    });
  }

  public int GetNumberOfWavSegments(Stream wavStream, TimeSpan segmentDuration)
  {
    return UseStream(wavStream, stream =>
    {
      using var waveReader = new WaveFileReader(wavStream);
      var totalDuration = waveReader.TotalTime;
      var segmentCount = (int)Math.Ceiling(totalDuration.TotalMilliseconds / segmentDuration.TotalMilliseconds);
      return segmentCount;
    });
  }

  public Stream GetWavSegment(Stream wavStream, int segmentNumber, TimeSpan segmentDuration)
  {
    return UseStream(wavStream, stream =>
    {
      using var segmentWaveReader = new WaveFileReader(wavStream);
      var segment = segmentWaveReader.ToSampleProvider()
        .Skip(segmentNumber * segmentDuration)
        .Take(segmentDuration);
      var segmentProvider = segment.ToWaveProvider16();
      var segmentStream = new MemoryStream();
      WaveFileWriter.WriteWavFileToStream(segmentStream, segmentProvider);
      segmentStream.Position = 0;
      return segmentStream;
    });
  }

  private static T UseStream<T>(Stream stream, Func<Stream, T> action)
  {
    ValidateStream(stream);

    var originalPosition = stream.Position;

    try
    {
      stream.Position = 0;
      return action(stream);
    }
    finally
    {
      stream.Position = originalPosition;
    }
  }

  private static void ValidateStream(Stream stream)
  {
    if (stream == null)
    {
      throw new ArgumentNullException(nameof(stream), $"{nameof(stream)} cannot be null");
    }

    if (stream.CanRead is false)
    {
      throw new ArgumentException($"{nameof(stream)} must be readable", nameof(stream));
    }

    if (stream.CanSeek is false)
    {
      throw new ArgumentException($"{nameof(stream)} must be seekable", nameof(stream));
    }
  }
}