using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Account.Domain.Aggregates.Biometrics;
using Account.Domain.Entities.Biometrics;
using Account.Domain.Enums;
using Account.Domain.Framework.Layers.Data.Repositories;
using Account.Domain.Framework.Layers.Data.UnitOfWork;
using Account.Domain.Providers;
using Account.Domain.Queries.Biometrics;
using System;

namespace Account.Tasks.Jobs
{
    public class SyncBiometricVerificationsJob : IScheduledJob
    {
        private readonly ILogger<SyncBiometricVerificationsJob> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository _repository;
        private readonly IBiometricProvider _provider;
        private readonly IBiometricVerificationQuery _query;

        public SyncBiometricVerificationsJob(
            ILogger<SyncBiometricVerificationsJob> logger,
            IUnitOfWork unitOfWork,
            IRepository repository,
            IBiometricProvider provider,
            IBiometricVerificationQuery query)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _repository = repository;
            _provider = provider;
            _query = query;
        }

        public string Schedule => "0 */2 * * * *";

        public async Task DoWorkAsync()
        {
            _logger.LogInformation("Iniciando SyncBiometricVerificationsJob às {Time}", DateTimeOffset.UtcNow);

            var pending = await _query.GetPending();

            foreach (var verification in pending)
            {
                try
                {
                    _logger.LogInformation("Processando verificacao pendente {Reference}", verification.Reference);

                    var providerResponse = await _provider.GetByReference(verification.Reference);
                    var domainStatus = BiometricProviderStatusMapper.Map(verification.Reference, providerResponse.Status);

                    var current = await _query.GetByReference(verification.Reference);
                    if (current == null)
                    {
                        current = BiometricVerification.New();
                        current.Reference = verification.Reference;
                    }

                    if (current.Status == domainStatus)
                    {
                        _logger.LogInformation("Verificacao {Reference} sem mudanca de status ({Status})", verification.Reference, domainStatus);
                    }
                    else
                    {
                        var previousStatus = current.Status;
                        var aggregate = new BiometricVerificationAggregate();
                        aggregate.ApplyResult(current, domainStatus, providerResponse.Status);
                        await _repository.UpdateAsync(current);

                        _logger.LogInformation("Verificacao {Reference} mudou de {De} para {Para}", verification.Reference, previousStatus, domainStatus);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Falha ao processar verificacao {Reference}", verification.Reference);
                    // Continuar com as outras referencias
                }
            }

            await _unitOfWork.CommitAsync();
            _logger.LogInformation("SyncBiometricVerificationsJob finalizado. Commit realizado.");
        }
    }
}
