namespace StreamShorts.Library.Media;

internal interface IFFMpegService
{
  Task<bool> ExtractAudioFromVideoAsync(Stream video, Stream audio);
}