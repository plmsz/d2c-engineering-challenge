using Account.Domain.Enums;
using Account.Domain.Entities.Biometrics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Account.Data.Stores
{
    public class BiometricVerificationStore
    {
        private readonly ConcurrentDictionary<string, BiometricVerification> _byId = new ConcurrentDictionary<string, BiometricVerification>();
        private readonly ConcurrentDictionary<string, string> _idByReference = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly object _sync = new object();

        public void Add(BiometricVerification verification)
        {
            lock (_sync)
            {
                _byId[verification.Id.ToString()] = verification;
                _idByReference[verification.Reference] = verification.Id.ToString();
            }
        }

        public void Update(BiometricVerification verification)
        {
            lock (_sync)
            {
                _byId[verification.Id.ToString()] = verification;
                _idByReference[verification.Reference] = verification.Id.ToString();
            }
        }

        public (BiometricVerification Verification, bool Created) GetOrCreateByReference(string reference, Func<BiometricVerification> create)
        {
            lock (_sync)
            {
                if (string.IsNullOrWhiteSpace(reference))
                    return (null, false);

                if (_idByReference.TryGetValue(reference, out var id))
                {
                    if (_byId.TryGetValue(id, out var existing))
                        return (existing, false);
                    else
                    {
                        // Reference tracked but entity removed, recreate
                        var newVer = create();
                        _byId[newVer.Id.ToString()] = newVer;
                        _idByReference[reference] = newVer.Id.ToString();
                        return (newVer, true);
                    }
                }
                else
                {
                    // Create new verification
                    var newVer = create();
                    _byId[newVer.Id.ToString()] = newVer;
                    _idByReference[reference] = newVer.Id.ToString();
                    return (newVer, true);
                }
            }
        }

        public BiometricVerification GetByReference(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
                return null;

            if (!_idByReference.TryGetValue(reference, out var id))
                return null;

            _byId.TryGetValue(id, out var verification);
            return verification;
        }

        public List<BiometricVerification> GetPending()
        {
            return _byId.Values.Where(v => v.Status == BiometricVerificationStatus.Pending).ToList();
        }
    }
}
