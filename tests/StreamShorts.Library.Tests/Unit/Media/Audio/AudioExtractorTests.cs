namespace StreamShorts.Library.Tests.Unit.Media.Audio;

public class AudioExtractorTests
{
  private readonly AudioExtractor _sut = new();

  [Fact]
  public async Task ExtractMp3FromMp4Async_WhenVideoIsNull_ItShouldThrow()
  {
    var action = async () => await _sut.ExtractMp3FromMp4Async(null!);

    await action.Should().ThrowAsync<ArgumentNullException>();
  }

  [Fact]
  public async Task ExtractMp3FromMp4Async_WhenVideoIsNotReadable_ItShouldThrow()
  {
    var mockStream = new Mock<Stream>();
    mockStream.Setup(s => s.CanRead).Returns(false);

    var action = async () => await _sut.ExtractMp3FromMp4Async(mockStream.Object);

    await action.Should().ThrowAsync<ArgumentException>();
  }

  [Fact]
  public async Task ExtractMp3FromMp4Async_WhenVideoIsNotSeekable_ItShouldThrow()
  {
    var mockStream = new Mock<Stream>();
    mockStream.Setup(s => s.CanSeek).Returns(false);

    var action = async () => await _sut.ExtractMp3FromMp4Async(mockStream.Object);

    await action.Should().ThrowAsync<ArgumentException>();
  }
}