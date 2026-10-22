using Account.Data.Stores;
using Account.Data.Queries;
using Account.Data.Repositories;
using Account.Domain.Framework.Layers.Data.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Account.Domain.Providers;
using Account.Data.Providers;
using Account.Domain.Queries.Biometrics;
using Account.Domain.Framework.Layers.Data.Repositories;
using Account.Domain.Responses;
using Account.Domain.Commands.Biometrics;
using Account.Domain.Framework.Layers.Domain.Command;
using Account.Domain.Enums;
using Account.Domain.Repositories;
using Xunit;
using System.Threading;
using System.Threading.Tasks;

namespace Account.Tests;

public class HandlerTests
{
    [Fact]
    public async Task Post_PendingReference_ReturnsPendingWithResultDescription()
    {
        var store = new BiometricVerificationStore();
        var provider = new FakeBiometricProvider();
        var biometricRepository = new BiometricVerificationRepository(store);
        var repository = new Account.Data.Repositories.Repository(store);
        var query = new BiometricVerificationQuery(store);

        var serviceProvider = new ServiceCollection()
            .AddSingleton<IUnitOfWork>(new Account.Data.UnitOfWork.InMemoryUnitOfWork())
            .AddSingleton<IBiometricVerificationQuery>(query)
            .AddSingleton<IRepository>(repository)
            .AddSingleton<IBiometricProvider>(provider)
            .AddSingleton<IBiometricVerificationRepository>(biometricRepository)
            .BuildServiceProvider();

        var handler = new ProcessBiometricVerificationCommandHandler(
            serviceProvider,
            query,
            repository,
            provider,
            biometricRepository);

        var command = new ProcessBiometricVerificationCommand.ProcessBiometricVerificationRequest { Reference = "verification-pending-3" };
        var cmd = new ProcessBiometricVerificationCommand(command);
        var result = await handler.Handle(cmd, CancellationToken.None);

        Assert.Equal("Pending", result.Status);
        Assert.NotNull(result.ResultDescription);
        Assert.NotNull(result.UpdatedAt);
    }
}