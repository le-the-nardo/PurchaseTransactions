namespace PurchaseTransactions.Api.Domain;

public class Transaction
{
    public Guid Id { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateOnly TransactionDate { get; private set; }
    public decimal PurchaseAmount { get; private set; }

    private Transaction() { }

    public static Transaction Create(string? description, DateOnly transactionDate, decimal purchaseAmount)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("INVALID_DESCRIPTION", "Description is required.");

        if (description.Length > 50)
            throw new DomainException("INVALID_DESCRIPTION", "Description must not exceed 50 characters.");

        if (transactionDate == default)
            throw new DomainException("INVALID_TRANSACTION_DATE", "Transaction date is required.");

        if (purchaseAmount <= 0)
            throw new DomainException("INVALID_PURCHASE_AMOUNT", "Purchase amount must be a positive value.");

        return new Transaction
        {
            Id = Guid.NewGuid(),
            Description = description,
            TransactionDate = transactionDate,
            PurchaseAmount = Math.Round(purchaseAmount, 2, MidpointRounding.AwayFromZero)
        };
    }
}
