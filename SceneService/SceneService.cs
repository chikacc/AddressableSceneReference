using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace FeatureNotBug {
    public sealed class SceneService : MonoBehaviour {
        readonly List<Scene> _loadedScenes = new();

        public Scene ActiveScene => SceneManager.GetActiveScene();

        void Awake() {
            for (var i = 0; i < SceneManager.sceneCount; ++i) _loadedScenes.Add(SceneManager.GetSceneAt(i));
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        public bool IsSceneLoaded(string path) {
            return _loadedScenes.Exists(x => x.path == path);
        }

        public bool IsSceneLoaded(SceneReference sceneReference) {
            return IsSceneLoaded(sceneReference.Path);
        }

        public UniTask<Scene> LoadSceneAsync(SceneReference sceneReference) {
            return LoadSceneAsync(sceneReference, LoadSceneMode.Additive, true, 100);
        }

        public async UniTask<Scene> LoadSceneAsync(SceneReference sceneReference, LoadSceneMode loadMode,
            bool activateOnLoad, int priority) {
            switch (sceneReference.State) {
                case SceneReferenceState.Regular:
                    var operation = SceneManager.LoadSceneAsync(sceneReference.BuildIndex, loadMode);
                    operation.allowSceneActivation = activateOnLoad;
                    operation.priority = priority;
                    await operation.ToUniTask();
                    break;
                case SceneReferenceState.Addressable:
                    await Addressables.LoadSceneAsync(sceneReference.Address, loadMode, activateOnLoad, priority)
                        .ToUniTask();
                    break;
                default:
                    throw new NotSupportedException($"SceneReferenceState {sceneReference.State} is not supported.");
            }

            return sceneReference.LoadedScene;
        }

        public async UniTask UnloadSceneAsync(SceneReference sceneReference) {
            await SceneManager.UnloadSceneAsync(sceneReference.LoadedScene);
        }

        public void SetActiveScene(Scene scene) {
            SceneManager.SetActiveScene(scene);
        }

        public void SetActiveScene(SceneReference sceneReference) {
            SceneManager.SetActiveScene(sceneReference.LoadedScene);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (mode == LoadSceneMode.Single) {
                _loadedScenes.Clear();
                _loadedScenes.Add(scene);
                return;
            }

            var index = _loadedScenes.FindIndex(x => x.path == scene.path);
            if (index != -1) {
                _loadedScenes[index] = scene;
                return;
            }

            _loadedScenes.Add(scene);
        }

        void OnSceneUnloaded(Scene scene) {
            _loadedScenes.RemoveAll(x => x.path == scene.path);
        }
    }
}
