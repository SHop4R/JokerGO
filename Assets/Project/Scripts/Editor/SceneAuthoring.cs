using JokerGO.Game;
using JokerGO.Game.Board;
using JokerGO.Game.Dice;
using JokerGO.Game.Fx;
using JokerGO.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JokerGO.Editor
{
    /// <summary>
    /// One-click scene authoring: generates all prefabs (camera rig, player, HUD,
    /// systems, tile) and places pre-configured instances into SampleScene, so
    /// nothing single-instance is created or composed at runtime.
    /// </summary>
    public static class SceneAuthoring
    {
        private const string ScenePath = "Assets/Project/Scenes/SampleScene.unity";
        private const string PrefabsFolder = "Assets/Project/Resources/Prefabs";
        private const string PostFxProfilePath = "Assets/Project/Resources/PostFxProfile.asset";

        [MenuItem("JokerGO/Author Scene")]
        public static void Author()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            ArtAssetGenerator.Generate();
            var scene = EditorSceneManager.OpenScene(ScenePath);

            RemoveAuthored("GameBootstrap", "CameraRig", "Player", "HUD", "Systems",
                "Ground", "Main Camera", "EventSystem");

            BoardStyle style = BoardStyle.CreateDefault();

            GameObject tilePrefab = AuthorPrefab(TileView.Author(style), "Tile");
            GameObject playerPrefab = AuthorPrefab(
                PlayerTokenView.Author(style.GetItemMaterial(Core.ItemType.Strawberry)), "Player");
            GameObject hudPrefab = AuthorPrefab(GameHud.Author().gameObject, "HUD");
            GameObject rigPrefab = AuthorPrefab(AuthorCameraRig(), "CameraRig");
            GameObject systemsPrefab = AuthorPrefab(AuthorSystems(tilePrefab), "Systems");

            var player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            var hud = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab, scene);
            var rig = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab, scene);
            var systems = (GameObject)PrefabUtility.InstantiatePrefab(systemsPrefab, scene);
            var ground = (GameObject)PrefabUtility.InstantiatePrefab(
                LoadPrefab("Ground"), scene);
            ground.name = "Ground";

            // Scene-instance wiring (cross-prefab references live as instance overrides).
            var environment = systems.GetComponentInChildren<EnvironmentBuilder>();
            environment.AuthorWire(LoadPrefab("Tree"), LoadPrefab("Bush"), LoadPrefab("Rock"),
                ground.transform);

            var bootstrap = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
            bootstrap.AuthorWire(
                hud.GetComponent<GameHud>(),
                player.GetComponent<PlayerTokenView>(),
                rig.GetComponent<CameraDirector>(),
                systems.GetComponentInChildren<BoardBuilder>(),
                environment,
                systems.GetComponentInChildren<GameFlowPresenter>(),
                systems.GetComponentInChildren<DiceRollDirector>());

            AuthorPostFx();
            AuthorAmbience();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("[JokerGO] Scene authored: prefabs generated and instances wired.");
        }

        private static void RemoveAuthored(params string[] names)
        {
            foreach (string objectName in names)
            {
                GameObject existing = GameObject.Find(objectName);
                if (existing != null)
                {
                    Object.DestroyImmediate(existing);
                }
            }
        }

        private static GameObject AuthorPrefab(GameObject temp, string assetName)
        {
            string path = $"{PrefabsFolder}/{assetName}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
            Object.DestroyImmediate(temp);
            return prefab;
        }

        private static GameObject LoadPrefab(string assetName)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsFolder}/{assetName}.prefab");
        }

        private static GameObject AuthorCameraRig()
        {
            var root = new GameObject("CameraRig");

            var cameraGo = new GameObject("Main Camera");
            cameraGo.tag = "MainCamera";
            cameraGo.transform.SetParent(root.transform);
            cameraGo.transform.position = new Vector3(0f, 7f, -7f);

            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.62f, 0.8f, 0.92f);
            cameraGo.AddComponent<AudioListener>();

            var urpData = cameraGo.AddComponent<UniversalAdditionalCameraData>();
            urpData.renderPostProcessing = true;

            // Ambient leaves ride with the camera; playOnAwake so no code is needed.
            GameObject leaves = Object.Instantiate(LoadPrefab("LeavesParticle"), cameraGo.transform);
            leaves.name = "AmbientLeaves";
            leaves.transform.localPosition = new Vector3(0f, 6f, 4f);
            ParticleSystem.MainModule leavesMain = leaves.GetComponent<ParticleSystem>().main;
            leavesMain.playOnAwake = true;

            CameraDirector.Author(root, camera);
            return root;
        }

        private static GameObject AuthorSystems(GameObject tilePrefab)
        {
            var root = new GameObject("Systems");

            var pool = new GameObject("PoolManager").AddComponent<PoolManager>();
            pool.transform.SetParent(root.transform);
            pool.AuthorWire(
                LoadPrefab("DustParticle").GetComponent<ParticleSystem>(),
                LoadPrefab("BurstParticle").GetComponent<ParticleSystem>(),
                LoadPrefab("Die").GetComponent<DieView>());

            var flow = new GameObject("GameFlow");
            flow.transform.SetParent(root.transform);
            flow.AddComponent<DiceRollDirector>();
            flow.AddComponent<GameFlowPresenter>();

            var board = new GameObject("Board").AddComponent<BoardBuilder>();
            board.transform.SetParent(root.transform);
            board.AuthorWire(tilePrefab);

            var environment = new GameObject("Environment").AddComponent<EnvironmentBuilder>();
            environment.transform.SetParent(root.transform);
            environment.AuthorWire(LoadPrefab("Tree"), LoadPrefab("Bush"), LoadPrefab("Rock"), null);

            var eventSystem = new GameObject("EventSystem");
            eventSystem.transform.SetParent(root.transform);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            return root;
        }

        private static void AuthorPostFx()
        {
            AssetDatabase.DeleteAsset(PostFxProfilePath);
            var profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile, PostFxProfilePath);

            var bloom = profile.Add<Bloom>();
            bloom.intensity.Override(0.55f);
            bloom.threshold.Override(1.05f);
            bloom.scatter.Override(0.6f);

            var vignette = profile.Add<Vignette>();
            vignette.intensity.Override(0.24f);
            vignette.smoothness.Override(0.42f);

            var colors = profile.Add<ColorAdjustments>();
            colors.saturation.Override(12f);
            colors.contrast.Override(6f);
            colors.postExposure.Override(0.15f);
            EditorUtility.SetDirty(profile);

            GameObject volumeGo = GameObject.Find("Global Volume");
            if (volumeGo == null)
            {
                volumeGo = new GameObject("Global Volume");
                volumeGo.AddComponent<Volume>().isGlobal = true;
            }

            Volume volume = volumeGo.GetComponent<Volume>();
            volume.profile = profile;
            EditorUtility.SetDirty(volume);
        }

        private static void AuthorAmbience()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.7f, 0.83f, 0.9f);
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance = 46f;
        }
    }
}
