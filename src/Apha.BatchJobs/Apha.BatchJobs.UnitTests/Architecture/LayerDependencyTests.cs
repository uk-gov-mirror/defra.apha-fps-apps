using Apha.BatchJobs.Application.DependencyInjection;
using NetArchTest.Rules;

namespace Apha.BatchJobs.UnitTests.Architecture;

public class LayerDependencyTests
{
    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        var result = Types
            .InAssembly(typeof(BatchApplicationServiceExtensions).Assembly)
            .That()
            .ResideInNamespace("Apha.BatchJobs.Application")
            .ShouldNot()
            .HaveDependencyOn("Apha.BatchJobs.Infrastructure")
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"Application must not depend on Infrastructure. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
