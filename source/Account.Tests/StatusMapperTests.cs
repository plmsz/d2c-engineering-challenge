using Account.Domain.Enums;
using Account.Domain.Framework.Layers.Domain.Validations;
using Account.Domain.Providers;
using Xunit;

namespace Account.Tests;

public class StatusMapperTests
{
    [Theory]
    [InlineData("PENDING", BiometricVerificationStatus.Pending)]
    [InlineData("APPROVED", BiometricVerificationStatus.Approved)]
    [InlineData("REJECTED", BiometricVerificationStatus.Rejected)]
    public void Map_WithValidStatuses_ReturnsExpectedBiometricStatus(string inputStatus, BiometricVerificationStatus expected)
    {
        // Act
        var result = BiometricProviderStatusMapper.Map("test-ref", inputStatus);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("UNKNOWN")]
    public void Map_WithInvalidStatuses_ThrowsBusinessException(string inputStatus)
    {
        // Act & Assert
        Assert.Throws<BusinessException>(() => BiometricProviderStatusMapper.Map("test-ref", inputStatus));
    }

    [Fact]
    public void Map_NullOrEmptyOrWhitespaceStatus_ThrowsBusinessException()
    {
        // Act & Assert
        Assert.Throws<BusinessException>(() => BiometricProviderStatusMapper.Map("test-ref", null!));
        Assert.Throws<BusinessException>(() => BiometricProviderStatusMapper.Map("test-ref", ""));
        Assert.Throws<BusinessException>(() => BiometricProviderStatusMapper.Map("test-ref", "   "));
    }

    [Fact]
    public void Map_UnknownStatus_ThrowsBusinessException()
    {
        // Act & Assert
        Assert.Throws<BusinessException>(() => BiometricProviderStatusMapper.Map("test-ref", "INKNOWN"));
    }
}