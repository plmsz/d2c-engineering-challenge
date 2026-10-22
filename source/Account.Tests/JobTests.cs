using Account.Data.Stores;
using Account.Data.Queries;
using Account.Data.Repositories;
using Account.Domain.Providers;
using Account.Domain.Queries.Biometrics;
using Account.Domain.Repositories;
using Account.Domain.Entities.Biometrics;
using Account.Domain.Enums;
using Account.Tasks.Jobs;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Account.Tests;

public class JobTests
{
    [Fact]
    public async Task Job_PendingVerificationApprovedByProvider_BecomesApproved()
    {
        // Arrange
        var store = new BiometricVerificationStore();
        var reference = "ref-job";

        // Create verification with Pending status
        var (verification, created) = store.GetOrCreateByReference(reference, () =>
        {
            var ver = BiometricVerification.New();
            ver.Reference = reference;
            ver.Status = BiometricVerificationStatus.Pending;
            return ver;
        });

        // Create a test provider that returns APPROVED for "ref-job"
        var provider = new ProgrammableBiometricProvider("ref-job", "APPROVED");

        // Set up required dependencies for the job
        var repository = new Account.Data.Repositories.Repository(store);
        var unitOfWork = new Account.Data.UnitOfWork.InMemoryUnitOfWork();
        var query = new BiometricVerificationQuery(store);

        var loggerFactory = LoggerFactory.Create(builder => builder.AddFilter("Microsoft", LogLevel.Warning).AddFilter("System", LogLevel.Warning));
        var logger = loggerFactory.CreateLogger<SyncBiometricVerificationsJob>();

        var job = new SyncBiometricVerificationsJob(
            logger,
            unitOfWork,
            repository,
            provider,
            query);

        // Act
        await job.DoWorkAsync();

        // Assert - the persisted state should have Status Approved
        var persisted = store.GetByReference(reference);
        Assert.NotNull(persisted);
        Assert.Equal(BiometricVerificationStatus.Approved, persisted.Status);
    }
}
