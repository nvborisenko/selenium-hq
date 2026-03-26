using System.Text.Json;
using System.Text.Json.Nodes;

namespace OpenQA.Selenium.BiDi.Cdp;

internal sealed class SendCommandCommand(SendCommandParameters @params, JsonObject? extensionData)
    : Command<SendCommandParameters, SendCommandResult>(@params, "goog:cdp.sendCommand", extensionData);

internal sealed record SendCommandParameters(string Method, JsonObject Params, string? Session) : Parameters;

public sealed record SendCommandOptions : CommandOptions
{
    public string? Session { get; init; }
}

public sealed record SendCommandResult(JsonElement Result, string Session) : EmptyResult;
