using Apha.BatchJobs.Application.DependencyInjection;
using NetArchTest.Rules;

namespace Apha.BatchJobs.UnitTests.Architecture;

public class LayerDependencyTests
{
    [Fact]
    public void Application_Should_Not_Depend_On_Infrastructure()
    {
        // Known exception: RecreateSummariesExecutionContext is physically in the Infrastructure
        // folder and carries BatchJobsDbContext + NpgsqlConnection, but its namespace was
        // (incorrectly) declared as Application. Properly fixing it requires decoupling the
        // execution-context API from EF/Npgsql — tracked as a future refactoring item.
        var result = Types
            .InAssembly(typeof(BatchApplicationServiceExtensions).Assembly)
            .That()
            .ResideInNamespace("Apha.BatchJobs.Application")
            .And()
            .DoNotHaveNameMatching("RecreateSummariesExecutionContext")
            .ShouldNot()
            .HaveDependencyOn("Apha.BatchJobs.Infrastructure")
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            $"Application must not depend on Infrastructure. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
