using AIChallenge.Models;
using AIChallenge.Services;
using Xunit;

namespace AIChallenge.Tests;

public sealed class ValidatorsTests
{
    [Fact]
    public void IsValidCurp_ReturnsTrue_ForValidStructure()
    {
        string curp = "GODE561231HDFBCD05";

        bool result = Validators.IsValidCurp(curp);

        Assert.True(result);
    }

    [Fact]
    public void IsAdult_ReturnsFalse_ForMinorCustomer()
    {
        DateOnly birthDate = new(2010, 1, 1);
        DateOnly today = new(2026, 1, 1);

        bool result = Validators.IsAdult(birthDate, today);

        Assert.False(result);
    }

    [Fact]
    public void IsKnownAddress_ReturnsTrue_ForSupportedAddress()
    {
        Address address = new("Calle 1", "Colima Centro", "28000", "Colima", "Colima");

        bool result = Validators.IsKnownAddress(address);

        Assert.True(result);
    }

    [Fact]
    public void IsValidCardNumber_ReturnsTrue_ForLuhnValidVisa()
    {
        string cardNumber = "4111111111111111";

        bool result = Validators.IsValidCardNumber(cardNumber);

        Assert.True(result);
    }

    [Fact]
    public void MatchesCardType_ReturnsFalse_WhenBrandDoesNotMatch()
    {
        string cardNumber = "4111111111111111";

        bool result = Validators.MatchesCardType(cardNumber, CardType.Mastercard);

        Assert.False(result);
    }
}
