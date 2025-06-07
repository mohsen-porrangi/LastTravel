namespace Order.Domain.ValueObjects;

public record TravelRoute
{
    public int SourceCode { get; }
    public int DestinationCode { get; }
    public string SourceName { get; }
    public string DestinationName { get; }

    public TravelRoute(int sourceCode, int destinationCode, string sourceName, string destinationName)
    {
        if (sourceCode <= 0)
            throw new ArgumentException("Source code must be positive", nameof(sourceCode));

        if (destinationCode <= 0)
            throw new ArgumentException("Destination code must be positive", nameof(destinationCode));

        if (sourceCode == destinationCode)
            throw new ArgumentException("Source and destination cannot be the same");

        if (string.IsNullOrWhiteSpace(sourceName))
            throw new ArgumentException("Source name is required", nameof(sourceName));

        if (string.IsNullOrWhiteSpace(destinationName))
            throw new ArgumentException("Destination name is required", nameof(destinationName));

        SourceCode = sourceCode;
        DestinationCode = destinationCode;
        SourceName = sourceName;
        DestinationName = destinationName;
    }

    public override string ToString() => $"{SourceName} ({SourceCode}) → {DestinationName} ({DestinationCode})";
}