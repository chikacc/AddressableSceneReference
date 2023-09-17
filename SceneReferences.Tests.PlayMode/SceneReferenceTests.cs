using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.AddressableAssets;
#if UNITY_EDITOR
using AddressablesExtensions;
using EditorTools;
using FluentAssertions;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine.SceneManagement;
#endif

namespace SceneReferences.Tests;

public sealed class SceneReferenceTests {
#if UNITY_EDITOR
    const string CacheFolder = "Assets/TestCache";

    AddressableAssetGroup _group;
    string _scenePath;
#endif

    [SetUp]
    public async Task Setup() {
#if UNITY_EDITOR
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var groupName = $"TestGroup{GUID.Generate()}";
        _group = settings.CreateGroup(groupName, false, false, true, null, typeof(BundledAssetGroupSchema));
        AssetDatabaseUtility.CreateDirectory(CacheFolder);
        var activeScene = SceneManager.GetActiveScene();
        _scenePath = AssetDatabase.GenerateUniqueAssetPath($"{CacheFolder}/TestScene.unity");
        AssetDatabase.CopyAsset(activeScene.path, _scenePath);
        settings.CreateOrMoveEntry(AssetDatabase.AssetPathToGUID(_scenePath), _group, false, false);
#endif
        await Addressables.InitializeAsync();
    }

    [TearDown]
    public void TearDown() {
#if UNITY_EDITOR
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        settings.RemoveGroup(_group);
        AssetDatabaseUtility.DeleteDirectory(CacheFolder);
#endif
    }

    [Test]
    public async Task IsLoaded() {
#if UNITY_EDITOR
        var guid = AssetDatabase.AssetPathToGUID(_scenePath);
        var sceneReference = new SceneReference(guid);
        sceneReference.IsLoaded().Should().BeFalse();
        await sceneReference.LoadSceneAsync(LoadSceneMode.Additive);
        sceneReference.IsLoaded().Should().BeTrue();
#else
        Assert.Ignore("This test is only valid in the editor.");
#endif
    }
}
