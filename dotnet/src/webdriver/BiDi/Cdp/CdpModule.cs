using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OpenQA.Selenium.BiDi.Json.Converters;

namespace OpenQA.Selenium.BiDi.Cdp;

public class CdpModule : Module
{
    private CdpJsonSerializerContext _jsonContext = null!;

    public Task<GetSessionResult> GetSessionAsync(BrowsingContext.BrowsingContext context, GetSessionOptions? options = null, CancellationToken cancellationToken = default)
    {
        var @params = new GetSessionParameters(context);

        return ExecuteCommandAsync(new GetSessionCommand(@params, null), options, _jsonContext.GetSessionCommand, _jsonContext.GetSessionResult, cancellationToken);
    }

    public Task<SendCommandResult> SendCommandAsync(string method, JsonObject parameters, SendCommandOptions? options = null, CancellationToken cancellationToken = default)
    {
        var @params = new SendCommandParameters(method, parameters, options?.Session);

        return ExecuteCommandAsync(new SendCommandCommand(@params, null), options, _jsonContext.SendCommandCommand, _jsonContext.SendCommandResult, cancellationToken);
    }

    public async Task<Subscription> OnEventAsync(Action<CdpEventArgs> handler, SubscriptionOptions? options = null, CancellationToken cancellationToken = default)
    {
        return await SubscribeAsync("goog:cdp", handler, options, _jsonContext.CdpEventArgs, cancellationToken).ConfigureAwait(false);
    }

    protected override void Initialize(IBiDi bidi, JsonSerializerOptions jsonSerializerOptions)
    {
        jsonSerializerOptions.Converters.Add(new BrowsingContextConverter(bidi));

        _jsonContext = new CdpJsonSerializerContext(jsonSerializerOptions);
    }
}

[JsonSerializable(typeof(GetSessionCommand))]
[JsonSerializable(typeof(GetSessionResult))]
[JsonSerializable(typeof(SendCommandCommand))]
[JsonSerializable(typeof(SendCommandResult))]

[JsonSerializable(typeof(CdpEventArgs))]

internal partial class CdpJsonSerializerContext : JsonSerializerContext;