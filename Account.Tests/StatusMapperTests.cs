using Account.Domain.Providers;
using Account.Domain.Entities;
using Account.Domain.Enums;
using Account.Domain.Framework.Layers.Domain.Validations;
using Xunit;

namespace Account.Tests;

public class StatusMapperTests
{
    [Theory]
    [InlineData("PENDING", BiometricVerificationStatus.Pending)]
    [InlineData("APPROVED", BiometricVerificationStatus.Approved)]
    [InlineData("REJECTED", BiometricVerificationStatus.Rejected)]
    public void Map_ValidStatuses_MapsCorrectly(string providerStatus, BiometricVerificationStatus expectedDomainStatus)
    {
        // Arrange
        var reference = "test-ref";

        // Act
        var result = BiometricProviderStatusMapper.Map(reference, providerStatus);

        // Assert
        Assert.Equal(expectedDomainStatus, result);
    }

    [Theory]
    [InlineData(null, "test-ref")]
    [InlineData("", "test-ref")]
    [InlineData("INKNOWN", "test-ref")]
    public void Map_InvalidStatuses_ThrowsBusinessException(string? providerStatus, string reference)
    {
        // Arrange
        // Act & Assert
        var exception = Assert.Throws<BusinessException>(() =>
            BiometricProviderStatusMapper.Map(reference, providerStatus!));

        // Assert
        Assert.Contains(reference, exception.Message);
    }
}