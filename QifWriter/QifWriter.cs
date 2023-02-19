using System.Text;
using System.IO.Abstractions;

namespace QifWriter;

public class QifWriter
{
    private readonly IFileSystem _fileSystem;

    public QifWriter(IFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
    }

    public void WriteFile(string fileName, QifTransaction[] transactions)
    {
        if (transactions == null) throw new ArgumentNullException(nameof(transactions));
        if (fileName == null) throw new ArgumentNullException(nameof(fileName));

        var sb = new StringBuilder();
        
        // Header
        sb.AppendLine($"Type:{transactions.First().Type.GetType().Name}");
        
        foreach (QifTransaction transaction in transactions)
        {
            sb.AppendLine($"D:{transaction.Date:yyyy-MM-dd}");
            sb.AppendLine($"T:{transaction.Amount:F2}");
            sb.AppendLine($"P:{transaction.Payee}");

            if (transaction.SubTransactions.Count == 0)
            {
                sb.AppendLine($"L:{transaction.Category}");
            }
            else
            {
                sb.Append($"L:{transaction.Category}:{transaction.SubTransactions[0].Category}");
                foreach (QifTransaction.SubTransaction subTransaction in transaction.SubTransactions)
                {
                    sb.AppendLine($"S:{transaction.Category}:{subTransaction.Category}");
                    sb.AppendLine($"E:{subTransaction.Extra}");
                    sb.AppendLine($"${subTransaction.Amount:F2}");
                }
            }
            
            sb.AppendLine($"M:{transaction.Memo}");
            sb.AppendLine("^");
        }

        _fileSystem.File.AppendAllText(fileName, sb.ToString());
    }
}
