using Soenneker.Graph.Users.Abstract;
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
}
