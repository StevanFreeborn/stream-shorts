
using NAudio.Wave;

namespace StreamShorts.Library.Media;

/// <summary>
/// Represents a service for processing audio files using NAudio.
/// </summary>
/// <inheritdoc/>
internal class NAudioService : IAudioService
{
  public Stream ConvertMp3ToWav16(Stream mp3)
  {
    using var reader = new Mp3FileReader(mp3);
    var outFormat = new WaveFormat(16000, reader.WaveFormat.Channels);
    using var resampler = new MediaFoundationResampler(reader, outFormat);
    var waveStream = new MemoryStream();
    WaveFileWriter.WriteWavFileToStream(waveStream, resampler);
    waveStream.Position = 0;
    return waveStream;
  }

  public int GetNumberOfWavSegments(Stream wavStream, TimeSpan segmentDuration)
  {
    using var waveReader = new WaveFileReader(wavStream);
    var totalDuration = waveReader.TotalTime;
    var segmentCount = (int)Math.Ceiling(totalDuration.TotalMilliseconds / segmentDuration.TotalMilliseconds);
    wavStream.Position = 0;
    return segmentCount;
  }

  public Stream GetWavSegment(Stream wavStream, int segmentNumber, TimeSpan segmentDuration)
  {
    using var segmentWaveReader = new WaveFileReader(wavStream);
    var segment = segmentWaveReader.ToSampleProvider()
      .Skip(segmentNumber * segmentDuration)
      .Take(segmentDuration);
    var segmentProvider = segment.ToWaveProvider16();
    var segmentStream = new MemoryStream();
    WaveFileWriter.WriteWavFileToStream(segmentStream, segmentProvider);
    segmentStream.Position = 0;
    wavStream.Position = 0;
    return segmentStream;
  }
}