using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rndr.Extensions;

namespace Rndr;

/// <summary>
/// Builder for configuring and creating TUI applications.
/// </summary>
public sealed class TuiAppBuilder
{
    /// <summary>
    /// Gets the underlying host application builder.
    /// </summary>
    public HostApplicationBuilder HostBuilder { get; }

    /// <summary>
    /// Gets the service collection for dependency injection configuration.
    /// </summary>
    public IServiceCollection Services => HostBuilder.Services;

    internal TuiAppBuilder(string[] args)
    {
        HostBuilder = Host.CreateApplicationBuilder(args);

        // Register core Rndr services
        Services.AddRndr();
    }

    /// <summary>
    /// Builds the configured application.
    /// </summary>
    /// <returns>A configured TuiApp instance.</returns>
    public TuiApp Build()
    {
        var host = HostBuilder.Build();
        return new TuiApp(host);
    }
}

