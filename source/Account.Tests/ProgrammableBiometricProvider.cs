using Account.Domain.Providers;

namespace Account.Tests;

public class ProgrammableBiometricProvider : IBiometricProvider
{
    private readonly string _reference;
    private readonly string _status;

    public ProgrammableBiometricProvider(string reference, string status)
    {
        _reference = reference;
        _status = status;
    }

    public Task<BiometricProviderResponse> GetByReference(string reference)
    {
        // Return the programmed status only for the specific reference
        if (reference == _reference)
        {
            return Task.FromResult(new BiometricProviderResponse { Status = _status });
        }
        // For other references, default to PENDING
        return Task.FromResult(new BiometricProviderResponse { Status = "PENDING" });
    }
}