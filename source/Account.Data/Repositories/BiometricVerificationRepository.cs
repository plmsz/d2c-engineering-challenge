using System;
using Account.Domain.Entities.Biometrics;
using Account.Domain.Repositories;
using Account.Data.Stores;

namespace Account.Data.Repositories
{
    public class BiometricVerificationRepository : IBiometricVerificationRepository
    {
        private readonly BiometricVerificationStore _store;

        public BiometricVerificationRepository(BiometricVerificationStore store)
        {
            _store = store;
        }

        public (BiometricVerification Verification, bool Created) GetOrCreateByReference(string reference, Func<BiometricVerification> create)
        {
            return _store.GetOrCreateByReference(reference, create);
        }
    }
}