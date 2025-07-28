namespace StreamShorts.Library.Tests.Data;

internal static class TestData
{
  private const string TestVideoFile = "video.mp4";
  private const string ExtractedAudioFile = "extracted_audio.mp3";

  public static FileStream GetTestVideo()
  {
    var filePath = GetTestFilePath(TestVideoFile);
    return File.OpenRead(filePath);
  }

  public static FileStream GetExtractedAudio()
  {
    var filePath = GetTestFilePath(ExtractedAudioFile);
    return File.OpenRead(filePath);
  }

  private static string GetTestFilePath(string fileName)
  {
    return Path.Combine(Directory.GetCurrentDirectory(), "Data", "Files", fileName);
  }
}