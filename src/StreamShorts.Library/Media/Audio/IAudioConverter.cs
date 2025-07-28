namespace StreamShorts.Library.Media.Audio;

public interface IAudioConverter
{
  Stream ConvertMp3ToWav16(Stream mp3);
}