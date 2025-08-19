namespace StreamShorts.Console.Hosting;

/// <summary>
/// Provides type registration services for the dependency injection container.
/// </summary>
/// <inheritdoc/>
internal sealed class TypeRegistrar(IHostBuilder builder) : ITypeRegistrar
{
  private readonly IHostBuilder _builder = builder;

  public ITypeResolver Build()
  {
    return new TypeResolver(_builder.Build());
  }

  public void Register(Type service, Type implementation)
  {
    _builder.ConfigureServices((_, services) => services.AddSingleton(service, implementation));
  }

  public void RegisterInstance(Type service, object implementation)
  {
    _builder.ConfigureServices((_, services) => services.AddSingleton(service, implementation));
  }

  public void RegisterLazy(Type service, Func<object> func)
  {
    ArgumentNullException.ThrowIfNull(func);

    _builder.ConfigureServices((_, services) => services.AddSingleton(service, _ => func()));
  }
}