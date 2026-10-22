using Account.Data.Stores;
using Account.Domain.Biometrics;
using Account.Domain.Queries.Biometrics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Account.Data.Queries
{
    public class BiometricQuery : IBiometricQuery
    {
        private readonly BiometricStore _store;

        public BiometricQuery(BiometricStore store)
        {
            _store = store;
        }

        public Task<Biometric> GetByReference(string reference)
        {
            return Task.FromResult(_store.GetByReference(reference));
        }

        public Task<List<Biometric>> GetPendingAsync()
        {
            return Task.FromResult(_store.GetPending().ToList());
        }
    }
}