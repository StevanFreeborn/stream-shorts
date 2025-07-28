
using NAudio.Wave;

namespace StreamShorts.Library.Media.Audio;

public class AudioConverter : IAudioConverter
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
}