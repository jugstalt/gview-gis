using gView.Framework.Core.Data;
using gView.Pipe.Abstraction;
using gView.Pipe.Abstraction.Nodes;
using gView.Pipe.Abstraction.Secrets;
using gView.Pipe.Abstraction.Serialization;
using gView.Pipe.Abstraction.Storage;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace gView.Pipe.Plugins.Gis;

/// <summary>
/// The feature classes of the selected data source as choice list for the parameter <c>featureClass</c> (gview-pipe ADR 0044).
/// The editor asks while the node is edited, so there is no node context: the credential comes from the services
/// (the store and the secret resolver are public Pipe contracts). Any problem gives an empty list, so the editor offers no choice.
/// </summary>
static internal class GisFeatureClassOptions
{
    /// <param name="parameter">The parameter the editor asks for; only <paramref name="featureClassKey"/> has options.</param>
    static public async Task<IReadOnlyList<PipeParameterOption>> GetAsync(IServiceProvider? services, string parameter, JsonObject parameters,
        string featureClassKey, string credentialKey, CancellationToken cancellationToken)
    {
        if (!parameter.Equals(featureClassKey, StringComparison.OrdinalIgnoreCase)
            || services is null
            || parameters[credentialKey] is not JsonValue idValue
            || !idValue.TryGetValue<string>(out var credentialId)
            || string.IsNullOrWhiteSpace(credentialId)
            || credentialId.Contains("{{"))
        {
            return [];
        }

        try
        {
            var credential = await CredentialAsync(services, credentialId, cancellationToken);
            using var dataset = await GisDatasets.OpenAsync(credential);
            return (await dataset.Elements())
                .Select(e => e.Class)
                .OfType<IFeatureClass>()
                .Select(fc => fc.Name)
                .Order(StringComparer.OrdinalIgnoreCase)
                .Select(name => new PipeParameterOption(name))
                .ToList();
        }
        catch (Exception ex) when (ex is PipeException or InvalidOperationException or IOException or JsonException or NotSupportedException
                                       or System.Data.Common.DbException or ArgumentException)
        {
            return [];
        }
    }

    /// <summary>Reads a stored credential like the node context does, including secrets of external sources.</summary>
    static public async Task<GisDatasourceCredential> CredentialAsync(IServiceProvider services, string credentialId, CancellationToken cancellationToken)
    {
        var store = services.GetService(typeof(IPipeCredentialStore)) as IPipeCredentialStore
            ?? throw new PipeException("The credential store is not available");
        var credential = await store.GetAsync(credentialId, cancellationToken)
            ?? throw new PipeException($"Credential '{credentialId}' not found");

        var resolver = services.GetService(typeof(IPipeSecretResolver)) as IPipeSecretResolver;
        foreach (var (field, node) in credential.Data.ToList())
        {
            if (node is JsonValue json && json.TryGetValue<string>(out var text) && PipeSecretReference.TryParse(text, out var reference))
            {
                credential.Data[field] = resolver is null
                    ? throw new PipeException("Secrets of external sources are not available")
                    : await resolver.ResolveAsync(reference, cancellationToken);
            }
        }

        return credential.Data.Deserialize<GisDatasourceCredential>(PipeJson.Options) ?? new GisDatasourceCredential();
    }
}
