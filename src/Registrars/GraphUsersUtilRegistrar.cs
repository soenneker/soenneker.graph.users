using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Graph.Client.Registrars;
using Soenneker.Graph.Users.Abstract;
using Soenneker.Utils.BackgroundQueue.Registrars;

namespace Soenneker.Graph.Users.Registrars;

/// <summary>
/// A utility library for Graph User related operations
/// </summary>
public static class GraphUsersUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IGraphUsersUtil"/> as a singleton service. <para/>
    /// </summary>
    public static IServiceCollection AddGraphUsersUtilAsSingleton(this IServiceCollection services)
    {
        services.AddBackgroundQueueAsSingleton().AddGraphClientUtilAsSingleton().TryAddSingleton<IGraphUsersUtil, GraphUsersUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IGraphUsersUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddGraphUsersUtilAsScoped(this IServiceCollection services)
    {
        services.AddBackgroundQueueAsSingleton().AddGraphClientUtilAsSingleton().TryAddScoped<IGraphUsersUtil, GraphUsersUtil>();

        return services;
    }
}