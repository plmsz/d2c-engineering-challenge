using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Account.Domain.Aggregates.Biometrics;
using Account.Domain.Entities.Biometrics;
using Account.Domain.Enums;
using Account.Domain.Framework.Layers.Data.Repositories;
using Account.Domain.Framework.Layers.Domain.Command;
using Account.Domain.Framework.Layers.Domain.Validations;
using Account.Domain.Providers;
using Account.Domain.Queries.Biometrics;
using Account.Domain.Responses;
using Account.Domain.Repositories;

namespace Account.Domain.Commands.Biometrics
{
    public class ProcessBiometricVerificationCommand : IRequest<BiometricVerificationResponse>
    {
        public ProcessBiometricVerificationRequest Request { get; }

        public ProcessBiometricVerificationCommand(ProcessBiometricVerificationRequest request)
        {
            Request = request;
        }

        public class ProcessBiometricVerificationRequest
        {
            [Required(ErrorMessage = "Campo obrigatorio")]
            public string Reference { get; set; }
        }
    }

    public class ProcessBiometricVerificationCommandHandler : CommandHandlerBase<BiometricVerification, ProcessBiometricVerificationCommand, BiometricVerificationResponse, BiometricVerificationAggregate>
    {
        private readonly IBiometricVerificationQuery _query;
        private readonly IRepository _repository;
        private readonly IBiometricProvider _provider;
        private readonly IBiometricVerificationRepository _repositoryBiometric;

        public ProcessBiometricVerificationCommandHandler(
            IServiceProvider serviceProvider,
            IBiometricVerificationQuery query,
            IRepository repository,
            IBiometricProvider provider,
            IBiometricVerificationRepository repositoryBiometric) : base(serviceProvider)
        {
            _query = query;
            _repository = repository;
            _provider = provider;
            _repositoryBiometric = repositoryBiometric;
        }

        protected override async Task<CommandHandlerResponse<BiometricVerification>> HandleRequest(ProcessBiometricVerificationCommand command, BiometricVerificationAggregate aggregate, CancellationToken cancellationToken)
        {
            if (command?.Request == null || string.IsNullOrWhiteSpace(command.Request.Reference))
                throw new BusinessException("Referencia da verificacao e obrigatoria.");

            var (current, created) = _repositoryBiometric.GetOrCreateByReference(command.Request.Reference, () =>
            {
                var newVer = BiometricVerification.New();
                newVer.Reference = command.Request.Reference;
                newVer.Status = BiometricVerificationStatus.Pending;
                return newVer;
            });

            var providerResponse = await _provider.GetByReference(command.Request.Reference);
            var domainStatus = BiometricProviderStatusMapper.Map(command.Request.Reference, providerResponse.Status);

            if (created || current.Status != domainStatus)
            {
                aggregate.ApplyResult(current, domainStatus, providerResponse.Status);
                await _repository.UpdateAsync(current);
            }

            return CommandHandlerResponse<BiometricVerification>.UpdateResult(current.Id, current);
        }

        protected override BiometricVerificationResponse Map(BiometricVerification entity)
        {
            return BiometricVerificationResponse.From(entity);
        }
    }
}