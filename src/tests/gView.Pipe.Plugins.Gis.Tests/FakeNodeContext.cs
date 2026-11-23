using gView.Pipe.Abstraction.Credentials;
using gView.Pipe.Abstraction.Data;
using gView.Pipe.Abstraction.Execution;
using gView.Pipe.Abstraction.Expressions;
using gView.Pipe.Abstraction.Nodes;
using gView.Pipe.Abstraction.Serialization;
using gView.Pipe.Abstraction.Variables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis.Tests;

/// <summary>
/// Just enough of a node context to run a node: parameters are bound as they are (no expressions),
/// credentials come from a dictionary. The Pipe engine isn't available here.
/// </summary>
internal sealed class FakeNodeContext(object parameters, IReadOnlyDictionary<string, IReadOnlyList<JsonObject>> inputs, Dictionary<string, object> credentials) : IPipeNodeContext
{
    public FakeNodeContext(object parameters, IReadOnlyList<JsonObject> input, Dictionary<string, object> credentials)
        : this(parameters, new Dictionary<string, IReadOnlyList<JsonObject>> { [PipePorts.Main] = input }, credentials) { }

    public string WorkflowId => "test";
    public string WorkflowName => "test";
    public string ExecutionId => "test";
    public string NodeId => "node";
    public string NodeName => "node";
    public string Environment => "DEFAULT";
    public PipeResolvedVariables Variables => throw new NotSupportedException();
    public IReadOnlyCollection<string> InputPorts => inputs.Keys.ToList();

    public IReadOnlyList<PipeItem> GetInput(string port = PipePorts.Main)
        => inputs.TryGetValue(port, out var input) ? input.Select(j => new PipeItem((JsonObject)j.DeepClone())).ToList() : [];

    public JsonObject RawParameters { get; } = JsonSerializer.SerializeToNode(parameters, PipeJson.Options)!.AsObject();

    public TParameters GetParameters<TParameters>(PipeItem? item = null, int itemIndex = 0) where TParameters : class, new()
        => RawParameters.Deserialize<TParameters>(PipeJson.Options)!;

    public JsonNode? Evaluate(string template, PipeItem? item = null, int itemIndex = 0) => throw new NotSupportedException();
    public PipeExpressionScope CreateExpressionScope(PipeItem? item = null, int itemIndex = 0) => throw new NotSupportedException();
    public Task<PipeCredential> GetCredentialAsync(string credentialId, CancellationToken cancellationToken = default) => throw new NotSupportedException();

    public Task<TData> GetCredentialAsync<TData>(string credentialId, CancellationToken cancellationToken = default) where TData : class, new()
        => Task.FromResult((TData)credentials[credentialId]);

    public IPipeBinaryStore BinaryStore => throw new NotSupportedException();
    public Task<PipeExecution> ExecuteWorkflowAsync(string workflowId, IReadOnlyList<PipeItem> items, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    public ILogger Logger => NullLogger.Instance;
    public IServiceProvider Services => throw new NotSupportedException();

    static public async Task<List<JsonObject>> RunAsync(IPipeNode node, object parameters, Dictionary<string, object> credentials, params JsonObject[] input)
    {
        var result = await node.ExecuteAsync(new FakeNodeContext(parameters, input, credentials), CancellationToken.None);
        return result.Outputs[PipePorts.Main].Select(i => i.Json).ToList();
    }

    /// <summary>All output ports, e.g. the Error output of Write Features.</summary>
    static public async Task<IReadOnlyDictionary<string, List<JsonObject>>> RunPortsAsync(IPipeNode node, object parameters, Dictionary<string, object> credentials, params JsonObject[] input)
    {
        var result = await node.ExecuteAsync(new FakeNodeContext(parameters, input, credentials), CancellationToken.None);
        return result.Outputs.ToDictionary(o => o.Key, o => o.Value.Select(i => i.Json).ToList());
    }

    static public async Task<List<JsonObject>> RunAsync(IPipeNode node, object parameters, IReadOnlyDictionary<string, IReadOnlyList<JsonObject>> inputs)
    {
        var result = await node.ExecuteAsync(new FakeNodeContext(parameters, inputs, new Dictionary<string, object>()), CancellationToken.None);
        return result.Outputs[PipePorts.Main].Select(i => i.Json).ToList();
    }
}
