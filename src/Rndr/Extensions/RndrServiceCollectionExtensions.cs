using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rndr.Input;
using Rndr.Rendering;

namespace Rndr.Extensions;

/// <summary>
/// Extension methods for registering Rndr services with dependency injection.
/// </summary>
public static class RndrServiceCollectionExtensions
{
    /// <summary>
    /// Adds core Rndr services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRndr(this IServiceCollection services)
    {
        // Core services
        services.TryAddSingleton<IClock, SystemClock>();
        services.TryAddSingleton<IInputSource, SystemInputSource>();
        services.TryAddSingleton<IConsoleAdapter, SystemConsoleAdapter>();
        services.TryAddSingleton<IStateStore, InMemoryStateStore>();

        // Options
        services.TryAddSingleton<RndrOptions>();
        services.TryAddSingleton<RndrTheme>();

        // Rendering
        services.TryAddSingleton<ITuiRenderer, ConsoleRenderer>();

        // Event loop
        services.TryAddSingleton<IEventLoop, DefaultEventLoop>();

        return services;
    }

    /// <summary>
    /// Configures the Rndr theme.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure the theme.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRndrTheme(this IServiceCollection services, Action<RndrTheme>? configure = null)
    {
        var theme = new RndrTheme();
        configure?.Invoke(theme);
        services.AddSingleton(theme);
        return services;
    }

    /// <summary>
    /// Configures Rndr runtime options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure the options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddRndrOptions(this IServiceCollection services, Action<RndrOptions>? configure = null)
    {
        var options = new RndrOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        return services;
    }
}

