using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Soenneker.Graph.Client.Abstract;
using Soenneker.Graph.Users.Abstract;
using Soenneker.Graph.Users.Registrars;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Graph.Users.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class GraphUsersUtilTests : HostedUnitTest
{
    private readonly IGraphUsersUtil _util;

    public GraphUsersUtilTests(Host host) : base(host)
    {
        _util = Resolve<IGraphUsersUtil>(true);
    }

    [Test]
    public void Default()
    {

    }

    [Test]
    public async Task Scoped_utility_keeps_graph_client_singleton()
    {
        var services = new ServiceCollection();

        services.AddGraphUsersUtilAsScoped();

        ServiceDescriptor graphClient = services.Single(descriptor => descriptor.ServiceType == typeof(IGraphClientUtil));
        ServiceDescriptor usersUtil = services.Single(descriptor => descriptor.ServiceType == typeof(IGraphUsersUtil));

        await Assert.That(graphClient.Lifetime).IsEqualTo(ServiceLifetime.Singleton);
        await Assert.That(usersUtil.Lifetime).IsEqualTo(ServiceLifetime.Scoped);
    }
}
