using System;
using Account.Domain.Entities.Biometrics;

namespace Account.Domain.Repositories
{
    public interface IBiometricVerificationRepository
    {
        (BiometricVerification Verification, bool Created) GetOrCreateByReference(string reference, Func<BiometricVerification> create);
    }
}