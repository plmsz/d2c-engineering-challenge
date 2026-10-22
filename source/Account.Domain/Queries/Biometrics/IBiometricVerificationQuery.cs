using Account.Domain.Entities.Biometrics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Account.Domain.Queries.Biometrics
{
    public interface IBiometricVerificationQuery
    {
        Task<BiometricVerification> GetByReference(string reference);
        Task<List<BiometricVerification>> GetPending();
    }
}
