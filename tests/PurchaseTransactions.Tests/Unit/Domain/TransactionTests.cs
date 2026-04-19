using PurchaseTransactions.Api.Domain;

namespace PurchaseTransactions.Tests.Unit.Domain;

public class TransactionTests
{
    private static readonly DateOnly ValidDate = new(2024, 6, 15);

    [Fact]
    public void Create_WithValidInputs_ReturnsTransaction()
    {
        var transaction = Transaction.Create("Coffee at Starbucks", ValidDate, 5.75m);

        Assert.NotEqual(Guid.Empty, transaction.Id);
        Assert.Equal("Coffee at Starbucks", transaction.Description);
        Assert.Equal(ValidDate, transaction.TransactionDate);
        Assert.Equal(5.75m, transaction.PurchaseAmount);
    }

    [Fact]
    public void Create_AssignsUniqueIdEachTime()
    {
        var t1 = Transaction.Create("A", ValidDate, 1m);
        var t2 = Transaction.Create("B", ValidDate, 2m);

        Assert.NotEqual(t1.Id, t2.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidDescription_ThrowsDomainException(string? description)
    {
        var ex = Assert.Throws<DomainException>(() => Transaction.Create(description, ValidDate, 1m));
        Assert.Equal("INVALID_DESCRIPTION", ex.ErrorCode);
    }

    [Fact]
    public void Create_WithDescriptionExceeding50Chars_ThrowsDomainException()
    {
        var description = new string('x', 51);

        var ex = Assert.Throws<DomainException>(() => Transaction.Create(description, ValidDate, 1m));
        Assert.Equal("INVALID_DESCRIPTION", ex.ErrorCode);
    }

    [Fact]
    public void Create_WithDescriptionExactly50Chars_Succeeds()
    {
        var description = new string('x', 50);

        var transaction = Transaction.Create(description, ValidDate, 1m);

        Assert.Equal(description, transaction.Description);
    }

    [Fact]
    public void Create_WithDefaultTransactionDate_ThrowsDomainException()
    {
        var ex = Assert.Throws<DomainException>(() => Transaction.Create("Desc", default, 1m));
        Assert.Equal("INVALID_TRANSACTION_DATE", ex.ErrorCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void Create_WithNonPositiveAmount_ThrowsDomainException(double amount)
    {
        var ex = Assert.Throws<DomainException>(() => Transaction.Create("Desc", ValidDate, (decimal)amount));
        Assert.Equal("INVALID_PURCHASE_AMOUNT", ex.ErrorCode);
    }

    [Theory]
    [InlineData(10.555, 10.56)]
    [InlineData(10.554, 10.55)]
    [InlineData(10.005, 10.01)]
    [InlineData(10.001, 10.00)]
    public void Create_RoundsPurchaseAmountToNearestCent(double input, double expected)
    {
        var transaction = Transaction.Create("Desc", ValidDate, (decimal)input);

        Assert.Equal((decimal)expected, transaction.PurchaseAmount);
    }
}
