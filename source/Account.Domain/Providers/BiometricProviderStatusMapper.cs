using Account.Domain.Enums;
using Account.Domain.Framework.Layers.Domain.Validations;
using System;

namespace Account.Domain.Providers
{
    public static class BiometricProviderStatusMapper
    {
        public static BiometricVerificationStatus Map(string reference, string status)
        {
            if (status == null)
                throw new BusinessException($"{reference}: status nulo");

            if (status == "")
                throw new BusinessException($"{reference}: status vazio");

            return status switch
            {
                "PENDING" => BiometricVerificationStatus.Pending,
                "APPROVED" => BiometricVerificationStatus.Approved,
                "REJECTED" => BiometricVerificationStatus.Rejected,
                _ => throw new BusinessException($"{reference}: status '{status}' recebido")
            };
        }
    }
}