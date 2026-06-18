using AIChallenge.Models;
using AIChallenge.Services;
using System.Collections;
using System.Reflection;
using Xunit;

namespace AIChallenge.Tests;

public sealed class ValidatorsAdditionalTests
{
    private static readonly object CatalogIoLock = new();

    [Fact]
    public void IsValidCurp_ReturnsFalse_ForInvalidStructure()
    {
        bool result = Validators.IsValidCurp("INVALID");

        Assert.False(result);
    }

    [Fact]
    public void IsAdult_ReturnsTrue_ForExactly18YearsOld()
    {
        DateOnly birthDate = new(2008, 6, 18);
        DateOnly today = new(2026, 6, 18);

        bool result = Validators.IsAdult(birthDate, today);

        Assert.True(result);
    }

    [Fact]
    public void IsKnownAddress_WithCatalogOverload_ReturnsTrue_WhenCaseInsensitiveMatch()
    {
        IReadOnlyList<Address> catalog =
        [
            new Address(string.Empty, "Roma Norte", "06700", "Cuauhtemoc", "Ciudad de Mexico")
        ];
        Address address = new("Calle 1", "roma norte", "06700", "cuauhtemoc", "ciudad de mexico");

        bool result = Validators.IsKnownAddress(address, catalog);

        Assert.True(result);
    }

    [Fact]
    public void IsKnownAddress_WithCatalogOverload_ReturnsFalse_WhenNoMatch()
    {
        IReadOnlyList<Address> catalog =
        [
            new Address(string.Empty, "Roma Norte", "06700", "Cuauhtemoc", "Ciudad de Mexico")
        ];
        Address address = new("Calle 1", "No Existe", "99999", "Otro", "Estado");

        bool result = Validators.IsKnownAddress(address, catalog);

        Assert.False(result);
    }

    [Theory]
    [InlineData("4111111111111111", CardType.Visa, true)]
    [InlineData("5555555555554444", CardType.Mastercard, true)]
    [InlineData("378282246310005", CardType.Amex, true)]
    [InlineData("4111111111111111", CardType.Amex, false)]
    public void MatchesCardType_CoversBranches(string cardNumber, CardType type, bool expected)
    {
        bool result = Validators.MatchesCardType(cardNumber, type);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void IsValidCardNumber_ReturnsFalse_WhenLengthOutOfRange()
    {
        bool result = Validators.IsValidCardNumber("12");

        Assert.False(result);
    }

    [Fact]
    public void IsValidCardNumber_ReturnsFalse_WhenLuhnFails()
    {
        bool result = Validators.IsValidCardNumber("4111111111111112");

        Assert.False(result);
    }

    [Fact]
    public void IsValidCardNumber_CoversSubtractNineBranch()
    {
        bool result = Validators.IsValidCardNumber("9999999999999");

        Assert.False(result);
    }

    [Theory]
    [InlineData("13/30", false)]
    [InlineData("00/30", false)]
    [InlineData("12/30", true)]
    public void IsValidExpiration_CoversValidAndInvalid(string expiration, bool expected)
    {
        bool result = Validators.IsValidExpiration(expiration);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("123", CardType.Visa, true)]
    [InlineData("1234", CardType.Amex, true)]
    [InlineData("123", CardType.Amex, false)]
    [InlineData("12A", CardType.Visa, false)]
    public void IsValidCvv_CoversBranches(string cvv, CardType type, bool expected)
    {
        bool result = Validators.IsValidCvv(cvv, type);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void MatchesCardType_ReturnsTrue_ForMastercard2SeriesRange()
    {
        bool result = Validators.MatchesCardType("2221000000000009", CardType.Mastercard);

        Assert.True(result);
    }

    [Fact]
    public void MatchesCardType_ReturnsFalse_ForUnknownEnumValue()
    {
        bool result = Validators.MatchesCardType("4111111111111111", (CardType)999);

        Assert.False(result);
    }

    [Fact]
    public void MaskCardNumber_ReturnsLastFourDigits()
    {
        string result = Validators.MaskCardNumber("4111 1111 1111 1111");

        Assert.Equal("**** **** **** 1111", result);
    }

    [Fact]
    public void FingerprintCardNumber_ReturnsStableHashForEquivalentInput()
    {
        string left = Validators.FingerprintCardNumber("4111 1111 1111 1111");
        string right = Validators.FingerprintCardNumber("4111111111111111");

        Assert.Equal(left, right);
    }

    [Fact]
    public void Normalize_TrimAndUpperCasesValue()
    {
        string result = Validators.Normalize("  hola  ");

        Assert.Equal("HOLA", result);
    }

    [Fact]
    public void LoadAddressCatalog_ReturnsFallbackCatalog_WhenFileIsMissing()
    {
        lock (CatalogIoLock)
        {
            _ = Validators.Normalize("init");
            string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
            string file = Path.Combine(dataDir, "address-catalog.txt");
            string backup = file + ".bak";
            Directory.CreateDirectory(dataDir);

            try
            {
                if (File.Exists(backup))
                {
                    File.Delete(backup);
                }

                if (File.Exists(file))
                {
                    File.Move(file, backup);
                }

                IDictionary catalog = InvokeLoadAddressCatalog();

                Assert.True(catalog.Count >= 4);
            }
            finally
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                if (File.Exists(backup))
                {
                    File.Move(backup, file);
                }
            }
        }
    }

    [Fact]
    public void LoadAddressCatalog_IgnoresBlankAndMalformedLines()
    {
        lock (CatalogIoLock)
        {
            _ = Validators.Normalize("init");
            string dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
            string file = Path.Combine(dataDir, "address-catalog.txt");
            string backup = file + ".bak";
            Directory.CreateDirectory(dataDir);

            try
            {
                if (File.Exists(backup))
                {
                    File.Delete(backup);
                }

                if (File.Exists(file))
                {
                    File.Move(file, backup);
                }

                File.WriteAllLines(file,
                [
                    "",
                    "invalid-line",
                    "06700|Roma Norte|Cuauhtemoc|Ciudad de Mexico"
                ]);

                IDictionary catalog = InvokeLoadAddressCatalog();

                Assert.Equal(1, catalog.Count);
                Assert.True(catalog.Contains("06700|Roma Norte|Cuauhtemoc|Ciudad de Mexico"));
            }
            finally
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                if (File.Exists(backup))
                {
                    File.Move(backup, file);
                }
            }
        }
    }

    private static IDictionary InvokeLoadAddressCatalog()
    {
        MethodInfo method = typeof(Validators).GetMethod("LoadAddressCatalog", BindingFlags.NonPublic | BindingFlags.Static)!;
        return (IDictionary)method.Invoke(null, null)!;
    }
}
