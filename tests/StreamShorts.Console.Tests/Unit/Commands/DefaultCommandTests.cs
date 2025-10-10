namespace StreamShorts.Console.Tests.Unit.Commands;

using Spectre.Console;

public class DefaultCommandTests
{
  [Theory]
  [InlineData("file[with]brackets.mp4", "file[[with]]brackets.mp4")]
  [InlineData("normal-file.mp4", "normal-file.mp4")]
  [InlineData("file_with_underscores.mp4", "file_with_underscores.mp4")]
  [InlineData("file with spaces.mp4", "file with spaces.mp4")]
  [InlineData("[brackets].mp4", "[[brackets]].mp4")]
  public void EscapeMarkup_WhenCalledWithVariousInputs_ItShouldProperlyEscapeSquareBrackets(string input, string expected)
  {
    var result = input.EscapeMarkup();

    result.Should().Be(expected);
  }
}
