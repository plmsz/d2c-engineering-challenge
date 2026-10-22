using Account.Data.Stores;
using Account.Domain.Entities.Biometrics;
using Account.Domain.Queries.Biometrics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Account.Data.Queries
{
    public class BiometricVerificationQuery : IBiometricVerificationQuery
    {
        private readonly BiometricVerificationStore _store;

        public BiometricVerificationQuery(BiometricVerificationStore store)
        {
            _store = store;
        }

        public Task<BiometricVerification> GetByReference(string reference)
        {
            return Task.FromResult(_store.GetByReference(reference));
        }

        public Task<List<BiometricVerification>> GetPending()
        {
            return Task.FromResult(_store.GetPending());
        }
    }
}
