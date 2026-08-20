using Apha.BatchJobs.Application.DependencyInjection;
using Apha.BatchJobs.Domain.Interfaces;
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

    [Fact]
    public void Domain_Should_Not_Depend_On_Outer_Layers()
    {
        var result = Types
            .InAssembly(typeof(IBatchLockRepository).Assembly)
            .That()
            .ResideInNamespace("Apha.BatchJobs.Domain")
            .ShouldNot()
            .HaveDependencyOnAny(
                "Apha.BatchJobs.Application",
                "Apha.BatchJobs.Infrastructure")
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"Domain must not depend on Application or Infrastructure. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
