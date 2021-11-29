using System;
using System.IO;
using System.Threading.Tasks;
using Hosts.Repository.Engine;
using Hosts.Repository.Models;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Tests
{
    public class AlertTests
    {
        public Repository<Alert> Alerts { get; set; }

        public AlertTests()
        {
            var loggerFactory = (ILoggerFactory)new LoggerFactory();
            
            var logger = loggerFactory.CreateLogger<Repository<Alert>>();
            
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");

            Alerts = new Repository<Alert>(logger, path);
        }

        [Fact]
        public async Task CreatesFileIfDoesntExist()
        {
            var filePath = await Alerts.EnsureFileStore();

            Assert.True(File.Exists(filePath));
        }

        [Fact]
        public async Task CanWriteAndReadToAndFromRepo()
        {
            var id = "ThisIsATestId";

            var alert = new Alert()
            {
                Id = id,
                CreationTime = DateTime.Now,
                Contents = "This.. is a test...",
                HasReported = false,
                ReportedTime = DateTime.MinValue
            };

            await Alerts.Save(alert);

            var foundAlert = await Alerts.GetById(id);

            Assert.NotNull(foundAlert);
        }

        [Fact]
        public async Task CanDeleteFromRepo()
        {
            var discardedId = "ThisIsATestId";
            var keptId = "ThisIsADifferentTestId";

            var discardedAlert = new Alert()
            {
                Id = discardedId,
                CreationTime = DateTime.Now,
                Contents = "This.. is an old test...",
                HasReported = false,
                ReportedTime = DateTime.MinValue
            };

            var keptAlert = new Alert()
            {
                Id = keptId,
                CreationTime = DateTime.Now,
                Contents = "This.. is a new test...",
                HasReported = false,
                ReportedTime = DateTime.MinValue
            };

            await Alerts.Save(discardedAlert);
            await Alerts.Save(keptAlert);

            await Alerts.Delete(discardedAlert);

            var foundDiscarded = await Alerts.GetById(discardedId);
            var foundKept = await Alerts.GetById(keptId);

            Assert.Null(foundDiscarded);
            Assert.NotNull(foundKept);

            await Alerts.Delete(keptAlert);
        }
    }
}
