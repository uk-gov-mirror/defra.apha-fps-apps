using System;
using Apha.BatchJobs.Application.Jobs.HealthCheck;
using Apha.BatchJobs.Domain.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Apha.BatchJobs.UnitTests.HealthCheck
{
    public class HealthCheckJobHandlerTests
    {
        [Fact]
        public void Constructor_SetsPropertiesAndThrowsOnNulls()
        {
            var logger = new Mock<ILogger<HealthCheckJobHandler>>().Object;
            var options = Options.Create(new BatchJobSettings());

            var handler = new HealthCheckJobHandler(logger, options);
            Assert.Equal("HealthCheck", handler.Name);
            Assert.Equal("NoWriteValidation", handler.IdempotencyStrategy);
            Assert.Null(handler.ScheduleExpression);
            Assert.Equal("On-demand health check (no schedule)", handler.ScheduleDescription);
            Assert.Equal(300, handler.MaxExecutionSeconds);

            Assert.Throws<ArgumentNullException>(() => new HealthCheckJobHandler(null!, options));
            var handlerWithNullOptions = new HealthCheckJobHandler(logger, null!);
            Assert.Equal("HealthCheck", handlerWithNullOptions.Name);
        }

        // Add more tests for execution logic if/when ExecuteAsync is available
    }
}
