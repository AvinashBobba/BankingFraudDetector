using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace BankingFraudDetector.AI.Mcp;

public sealed class AssessmentMcpGateway(
    AssessmentMcpOptions options,
    string connectionString)
{
    public async Task<string> GetEvidenceAsync(
        Guid assessmentId,
        CancellationToken cancellationToken = default)
    {
        if (assessmentId == Guid.Empty)
        {
            throw new ArgumentException(
                "An assessment ID is required.",
                nameof(assessmentId));
        }

        if (!Path.IsPathFullyQualified(options.ServerDllPath) ||
            !File.Exists(options.ServerDllPath))
        {
            throw new InvalidOperationException(
                "An existing absolute MCP server DLL path is required.");
        }

        if (options.TimeoutSeconds <= 0)
        {
            throw new InvalidOperationException(
                "The MCP timeout must be positive.");
        }

        using var timeout =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        timeout.CancelAfter(
            TimeSpan.FromSeconds(options.TimeoutSeconds));

        var environment =
            StdioClientTransportOptions.GetDefaultEnvironmentVariables();

        environment["ConnectionStrings__FraudDatabase"] =
            connectionString;

        var transport = new StdioClientTransport(
            new StdioClientTransportOptions
            {
                Name = "Fraud assessment server",
                Command = "dotnet",
                Arguments = [options.ServerDllPath],
                InheritEnvironmentVariables = false,
                EnvironmentVariables = environment
            });

        await using var client = await McpClient.CreateAsync(
            transport,
            cancellationToken: timeout.Token);

        var tools = await client.ListToolsAsync(
            cancellationToken: timeout.Token);

        var evidenceTool = tools.SingleOrDefault(tool =>
            string.Equals(
                tool.Name.Replace("_", ""),
                "GetAssessmentEvidence",
                StringComparison.OrdinalIgnoreCase));

        if (evidenceTool is null)
        {
            throw new InvalidOperationException(
                "The MCP assessment evidence tool was not found.");
        }

        var result = await client.CallToolAsync(
            evidenceTool.Name,
            new Dictionary<string, object?>
            {
                ["assessmentId"] = assessmentId.ToString()
            },
            cancellationToken: timeout.Token);

        if (result.IsError == true)
        {
            throw new InvalidOperationException(
                "The MCP assessment evidence tool failed.");
        }

        var text = string.Join(
            Environment.NewLine,
            result.Content
                .OfType<TextContentBlock>()
                .Select(block => block.Text));

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException(
                "The MCP evidence tool returned no text.");
        }

        return text;
    }
}