namespace BankingFraudDetector.Application.Graphs;

public sealed record ConnectionPathNode(string Type, string Identifier);

public sealed record AccountConnectionPath(
    Guid SourceAccountId,
    Guid TargetAccountId,
    IReadOnlyList<ConnectionPathNode> Nodes);
