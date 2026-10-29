namespace BankingFraudDetector.Application.Graphs;

public sealed class GraphProjectionUnavailableException
    : Exception
{
    public GraphProjectionUnavailableException()
        : base("The account graph projection is not available.")
    {
    }
}