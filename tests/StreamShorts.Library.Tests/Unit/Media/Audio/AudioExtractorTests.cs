using System.Text;

using StreamShorts.Library.Media;

namespace StreamShorts.Library.Tests.Unit.Media.Audio;

public class AudioExtractorTests
{
  private readonly Mock<IFFMpegService> _mockFfmpegService = new();
  private readonly AudioExtractor _sut;

  public AudioExtractorTests()
  {
    _sut = new(_mockFfmpegService.Object);
  }

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

  [Fact]
  public async Task ExtractMp3FromMp4Async_WhenFFMpegServiceThrows_ItShouldThrow()
  {
    var mockStream = new Mock<Stream>();
    mockStream.Setup(static s => s.CanRead).Returns(true);
    mockStream.Setup(static s => s.CanSeek).Returns(true);

    _mockFfmpegService.
      Setup(
        static m => m.ExtractAudioFromVideoAsync(
          It.IsAny<Stream>(),
          It.IsAny<Stream>()
        )
      )
      .ThrowsAsync(new Exception());

    var action = async () => await _sut.ExtractMp3FromMp4Async(mockStream.Object);

    await action.Should().ThrowAsync<FailedAudioExtractionException>();
  }

  [Fact]
  public async Task ExtractMp3FromMp4Async_WhenFFMpegServiceFailsExtraction_ItShouldThrow()
  {
    var mockStream = new Mock<Stream>();
    mockStream.Setup(static s => s.CanRead).Returns(true);
    mockStream.Setup(static s => s.CanSeek).Returns(true);

    _mockFfmpegService.
      Setup(
        static m => m.ExtractAudioFromVideoAsync(
          It.IsAny<Stream>(),
          It.IsAny<Stream>()
        )
      )
      .ReturnsAsync(false);

    var action = async () => await _sut.ExtractMp3FromMp4Async(mockStream.Object);

    await action.Should().ThrowAsync<FailedAudioExtractionException>();
  }

  [Fact]
  public async Task ExtractMp3FromMp4Async_WhenFFMpegServiceSucceedsAtExtractingAudio_ItShouldReturnStream()
  {
    var mockStream = new Mock<Stream>();
    mockStream.Setup(static s => s.CanRead).Returns(true);
    mockStream.Setup(static s => s.CanSeek).Returns(true);

    _mockFfmpegService.
      Setup(
        static m => m.ExtractAudioFromVideoAsync(
          It.IsAny<Stream>(),
          It.IsAny<Stream>()
        )
      )
      .ReturnsAsync(true);

    var result = await _sut.ExtractMp3FromMp4Async(mockStream.Object);

    result.Should().BeAssignableTo<Stream>();
    result.Should().BeOfType<MemoryStream>();
  }

  [Fact]
  public async Task ExtractMp3FromMp4Async_WhenCalled_ItShouldNotMutatePositionOfPassedStream()
  {
    var text = "hello world";
    var textByte = Encoding.UTF8.GetBytes(text);
    var stream = new MemoryStream(textByte);

    var positionToRead = 5;
    var buffer = new byte[5];
    await stream.ReadAsync(buffer.AsMemory(0, positionToRead), TestContext.Current.CancellationToken);

    _mockFfmpegService.
      Setup(
        static m => m.ExtractAudioFromVideoAsync(
          It.IsAny<Stream>(),
          It.IsAny<Stream>()
        )
      )
      .ReturnsAsync(true);

    await _sut.ExtractMp3FromMp4Async(stream);

    stream.Position.Should().Be(positionToRead);
  }
}