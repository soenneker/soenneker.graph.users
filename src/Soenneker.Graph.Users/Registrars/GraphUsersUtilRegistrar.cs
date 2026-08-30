using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Graph.Client.Registrars;
using Soenneker.Graph.Users.Abstract;
using Soenneker.Utils.BackgroundQueue.Registrars;

namespace Soenneker.Graph.Users.Registrars;

/// <summary>
/// Registers Graph user operations over the application-wide Graph client and background queue.
/// </summary>
public static class GraphUsersUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IGraphUsersUtil"/> as a singleton service. <para/>
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddGraphUsersUtilAsSingleton(this IServiceCollection services)
    {
        services.AddBackgroundQueueAsSingleton().AddGraphClientUtilAsSingleton().TryAddSingleton<IGraphUsersUtil, GraphUsersUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IGraphUsersUtil"/> as a scoped service. <para/>
    /// </summary>
    /// <param name="services">Service collection that receives the registration.</param>
    /// <returns>The same service collection, so additional registrations can be chained.</returns>
    public static IServiceCollection AddGraphUsersUtilAsScoped(this IServiceCollection services)
    {
        services.AddBackgroundQueueAsSingleton().AddGraphClientUtilAsSingleton().TryAddScoped<IGraphUsersUtil, GraphUsersUtil>();

        return services;
    }
}
