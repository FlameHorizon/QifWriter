namespace QifWriter;

public class QifTransaction
{
    public QifTransactionType Type { get; init; }
    public string Payee { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Memo { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public List<SubTransaction> SubTransactions { get; init; } = new();
    public DateOnly Date { get; init; }

    public class SubTransaction
    {
        public string Category { get; init; } = string.Empty;
        public string Extra { get; init; } = string.Empty;
        public decimal Amount { get; init; }
    }
}