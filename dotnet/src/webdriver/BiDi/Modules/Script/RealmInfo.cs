using System.Collections.Generic;

namespace OpenQA.Selenium.BiDi.Modules.Script;

// https://github.com/dotnet/runtime/issues/72604
//[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
//[JsonDerivedType(typeof(Window), "window")]
//[JsonDerivedType(typeof(DedicatedWorker), "dedicated-worker")]
//[JsonDerivedType(typeof(SharedWorker), "shared-worker")]
//[JsonDerivedType(typeof(ServiceWorker), "service-worker")]
//[JsonDerivedType(typeof(Worker), "worker")]
//[JsonDerivedType(typeof(PaintWorklet), "paint-worklet")]
//[JsonDerivedType(typeof(AudioWorklet), "audio-worklet")]
//[JsonDerivedType(typeof(Worklet), "worklet")]
public abstract record RealmInfo(BiDi BiDi) : EventArgs(BiDi);

public abstract record BaseRealmInfo(BiDi BiDi, Realm Realm, string Origin) : RealmInfo(BiDi)
{
    public record Window(BiDi BiDi, Realm Realm, string Origin, BrowsingContext.BrowsingContext Context) : BaseRealmInfo(BiDi, Realm, Origin)
    {
        public string? Sandbox { get; set; }
    }

    public record DedicatedWorker(BiDi BiDi, Realm Realm, string Origin, IReadOnlyList<Realm> Owners) : BaseRealmInfo(BiDi, Realm, Origin);

    public record SharedWorker(BiDi BiDi, Realm Realm, string Origin) : BaseRealmInfo(BiDi, Realm, Origin);

    public record ServiceWorker(BiDi BiDi, Realm Realm, string Origin) : BaseRealmInfo(BiDi, Realm, Origin);

    public record Worker(BiDi BiDi, Realm Realm, string Origin) : BaseRealmInfo(BiDi, Realm, Origin);

    public record PaintWorklet(BiDi BiDi, Realm Realm, string Origin) : BaseRealmInfo(BiDi, Realm, Origin);

    public record AudioWorklet(BiDi BiDi, Realm Realm, string Origin) : BaseRealmInfo(BiDi, Realm, Origin);

    public record Worklet(BiDi BiDi, Realm Realm, string Origin) : BaseRealmInfo(BiDi, Realm, Origin);
}
