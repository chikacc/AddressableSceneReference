using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AddressablesExtensions;

[Serializable]
#if UNITY_EDITOR
public sealed class SceneReference : AssetReferenceT<SceneAsset> {
#else
public sealed class SceneReference : AssetReference {
#endif
    public SceneReference(string guid) : base(guid) { }

    public bool IsLoaded() {
        var key = RuntimeKey;
        var type = typeof(SceneInstance);
        foreach (var locator in Addressables.ResourceLocators)
            if (locator.Locate(key, type, out var locations)) {
                var location = locations[0];
                return SceneManager.GetSceneByPath(location.InternalId).isLoaded;
            }

        return false;
    }
}
