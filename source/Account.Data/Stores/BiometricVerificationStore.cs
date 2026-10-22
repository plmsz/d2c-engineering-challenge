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

        public void Add(BiometricVerification verification)
        {
            _byId[verification.Id.ToString()] = verification;
            _idByReference[verification.Reference] = verification.Id.ToString();
        }

        public void Update(BiometricVerification verification)
        {
            _byId[verification.Id.ToString()] = verification;
            _idByReference[verification.Reference] = verification.Id.ToString();
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
