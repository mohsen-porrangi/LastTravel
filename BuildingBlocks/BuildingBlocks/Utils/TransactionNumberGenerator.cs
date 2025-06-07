using BuildingBlocks.Contracts;

namespace BuildingBlocks.Utils;
public class TransactionNumberGenerator : ITransactionNumberGenerator
{
    private static readonly object _lock = new();
    private static long _sequenceNumber = 0;

    public string Generate()
    {
        var now = DateTime.UtcNow;
        var dateStr = now.ToString("yyyyMMdd");
        var timeStr = now.ToString("HHmmss");

        long sequence;
        lock (_lock)
        {
            _sequenceNumber = (_sequenceNumber + 1) % 10000;
            sequence = _sequenceNumber;
        }

        return $"TXN-{dateStr}-{timeStr}-{sequence:D4}";
    }
}