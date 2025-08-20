namespace StreamShorts.Console.Tests.Unit.Hosting;

public class TypeResolverTests
{
  [Fact]
  public void Constructor_WhenCalledWithNullHost_ItShouldThrowArgumentNullException()
  {
    var action = static () => new TypeResolver(null!);

    action.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Resolve_WhenTypeIsNull_ItShouldReturnNull()
  {
    var mockHost = new Mock<IHost>();
    using var resolver = new TypeResolver(mockHost.Object);

    var result = resolver.Resolve(null);

    result.Should().BeNull();
  }

  [Fact]
  public void Resolve_WhenCalledWithRegisteredType_ItShouldReturnAnInstance()
  {
    var services = new ServiceCollection();
    services.AddSingleton(new TestService());

    var mockHost = new Mock<IHost>();

    mockHost
      .Setup(static h => h.Services)
      .Returns(services.BuildServiceProvider());

    using var resolver = new TypeResolver(mockHost.Object);

    var result = resolver.Resolve(typeof(TestService));

    result.Should().NotBeNull();
    result.Should().BeOfType<TestService>();
  }

  [Fact]
  public void Resolve_WhenCalledWithUnregisteredType_ItShouldReturnNull()
  {
    var services = new ServiceCollection();
    var mockHost = new Mock<IHost>();

    mockHost
      .Setup(static h => h.Services)
      .Returns(services.BuildServiceProvider());

    using var resolver = new TypeResolver(mockHost.Object);

    var result = resolver.Resolve(typeof(TestService));

    result.Should().BeNull();
  }

  [Fact]
  public void Dispose_WhenCalled_ItShouldAlsoDisposeHost()
  {
    var mockHost = new Mock<IHost>();
    var resolver = new TypeResolver(mockHost.Object);

    resolver.Dispose();

    mockHost.Verify(static h => h.Dispose(), Times.Once);
  }

  private sealed class TestService { }
}