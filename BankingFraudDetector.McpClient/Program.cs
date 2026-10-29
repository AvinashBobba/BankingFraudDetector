using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

if (args.Length != 2 ||
    !Guid.TryParse(args[1], out var assessmentId) ||
    assessmentId == Guid.Empty)
{
    Console.Error.WriteLine(
        "Usage: <server-dll-path> <assessment-id>");
    return;
}

var serverPath = Path.GetFullPath(args[0]);

if (!File.Exists(serverPath))
{
    Console.Error.WriteLine($"Server DLL not found: {serverPath}");
    return;
}

using var timeout = new CancellationTokenSource(
    TimeSpan.FromSeconds(60));

var transport = new StdioClientTransport(
    new StdioClientTransportOptions
    {
        Name = "Fraud assessment server",
        Command = "dotnet",
        Arguments = [serverPath]
    });

await using var client = await McpClient.CreateAsync(
    transport,
    cancellationToken: timeout.Token);

var tools = await client.ListToolsAsync(
    cancellationToken: timeout.Token);

Console.WriteLine("Available tools:");

foreach (var tool in tools)
{
    Console.WriteLine($"{tool.Name}: {tool.Description}");
}

var assessmentTool = tools.SingleOrDefault(tool =>
    string.Equals(
        tool.Name.Replace("_", ""),
        "GetAssessmentEvidence",
        StringComparison.OrdinalIgnoreCase));

if (assessmentTool is null)
{
    throw new InvalidOperationException(
        "The assessment evidence tool was not discovered.");
}

var result = await client.CallToolAsync(
    assessmentTool.Name,
    new Dictionary<string, object?>
    {
        ["assessmentId"] = assessmentId.ToString()
    },
    cancellationToken: timeout.Token);

Console.WriteLine(
    result.IsError == true ? "Tool failed:" : "Tool result:");

foreach (var content in result.Content.OfType<TextContentBlock>())
{
    Console.WriteLine(content.Text);
}