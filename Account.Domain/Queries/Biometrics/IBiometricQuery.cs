using Account.Domain.Biometrics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Account.Domain.Queries.Biometrics
{
    public interface IBiometricQuery
    {
        Task<Biometric> GetByReference(string reference);
        Task<List<Biometric>> GetPendingAsync();
    }
}