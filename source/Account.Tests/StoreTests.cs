using Account.Data.Repositories;
using Account.Data.Stores;
using Account.Domain.Entities.Biometrics;
using Account.Domain.Enums;
using Xunit;

namespace Account.Tests;

public class StoreTests
{
    [Fact]
    public async Task GetOrCreateByReference_ConcurrentCalls_CreatesSingleVerification()
    {
        // Arrange
        var store = new BiometricVerificationStore();
        var repository = new BiometricVerificationRepository(store);
        var reference = "ref-concorrente";

        // Act - dispatch 10 concurrent calls
        var tasks = Enumerable.Range(0, 10)
            .Select(_ => Task.Run(() => repository.GetOrCreateByReference(reference, () =>
            {
                var ver = BiometricVerification.New();
                ver.Reference = reference;
                ver.Status = BiometricVerificationStatus.Pending;
                return ver;
            }))).ToList();

        var results = await Task.WhenAll(tasks);

        // Assert - all returns have the same Id
        var firstId = results[0].Verification.Id.ToString();
        foreach (var result in results)
        {
            Assert.Equal(firstId, result.Verification.Id.ToString());
        }

        // Assert - store shows a single verification
        var pending = store.GetPending();
        Assert.Single(pending);

        var byRef = store.GetByReference(reference);
        Assert.NotNull(byRef);
        Assert.Equal(firstId, byRef.Id.ToString());
    }
}