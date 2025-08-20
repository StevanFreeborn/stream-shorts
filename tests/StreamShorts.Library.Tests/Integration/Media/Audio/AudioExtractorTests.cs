namespace StreamShorts.Library.Tests.Integration.Media.Audio;

internal class AudioExtractorTests
{
  private readonly AudioExtractor _sut = new();

  [Fact]
  public async Task ExtractMp3FromMp4Async_WhenCalled_ItShouldExtractAudio()
  {
    using var testVideo = TestData.GetTestVideo();
    using var extractedAudio = TestData.GetExtractedAudio();

    var result = await _sut.ExtractMp3FromMp4Async(testVideo);

    var audioBytes = await ConvertStreamToBytesAsync(extractedAudio);
    var resultBytes = await ConvertStreamToBytesAsync(result);

    resultBytes.Should().Equal(audioBytes);
  }

  private static async Task<byte[]> ConvertStreamToBytesAsync(Stream stream)
  {
    using var ms = new MemoryStream();
    await stream.CopyToAsync(ms);
    return ms.ToArray();
  }
}