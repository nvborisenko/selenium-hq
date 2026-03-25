using System.Text.Json.Nodes;

namespace OpenQA.Selenium.BiDi.Cdp;

internal sealed class GetSessionCommand(GetSessionParameters @params, JsonObject? extensionData)
    : Command<GetSessionParameters, GetSessionResult>(@params, "goog:cdp.getSession", extensionData);

internal sealed record GetSessionParameters(BrowsingContext.BrowsingContext Context) : Parameters;

public sealed record GetSessionOptions : CommandOptions;

public sealed record GetSessionResult(string Session) : EmptyResult;
