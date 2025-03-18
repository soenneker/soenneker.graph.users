using Soenneker.Graph.Users.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;

namespace Soenneker.Graph.Users.Tests;

[Collection("Collection")]
public class GraphUsersUtilTests : FixturedUnitTest
{
    private readonly IGraphUsersUtil _util;

    public GraphUsersUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IGraphUsersUtil>(true);
    }

    [Fact]
    public void Default()
    {

    }
}
