using JokerGO.Core.Project.Scripts.Core;
using JokerGO.Game.Project.Scripts.Game;
using JokerGO.Game.Project.Scripts.Game.Board;
using JokerGO.Game.Project.Scripts.Game.Dice;
using JokerGO.Game.Project.Scripts.Game.Fx;
using JokerGO.UI.Project.Scripts.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace JokerGO.Editor.Project.Scripts.Editor
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

        private const string EtfxDustPath = "Assets/Epic Toon FX/Prefabs/Environment/Dust/DustDirtyPoof.prefab";
        private const string EtfxBurstPath = "Assets/Epic Toon FX/Prefabs/Interactive/Stars/StarPoof.prefab";

        [MenuItem("JokerGO/Author Scene")]
        public static void Author()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            Scene scene = EditorSceneManager.OpenScene(ScenePath);

            RemoveAuthored("GameBootstrap", "CameraRig", "Player", "HUD", "Systems", "Ground", "Main Camera", "EventSystem");

            BoardStyle style = BoardStyle.CreateDefault();

            GameObject tilePrefab = AuthorPrefab(TileView.Author(style), "Tile");
            GameObject playerPrefab = AuthorPrefab(PlayerTokenView.Author(style.GetItemMaterial(ItemType.Strawberry)), "Player");
            GameObject hudPrefab = AuthorPrefab(GameHud.Author().gameObject, "HUD");
            GameObject rigPrefab = AuthorPrefab(AuthorCameraRig(), "CameraRig");
            GameObject systemsPrefab = AuthorPrefab(AuthorSystems(tilePrefab), "Systems");

            GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab, scene);
            GameObject hud = (GameObject)PrefabUtility.InstantiatePrefab(hudPrefab, scene);
            GameObject rig = (GameObject)PrefabUtility.InstantiatePrefab(rigPrefab, scene);
            GameObject systems = (GameObject)PrefabUtility.InstantiatePrefab(systemsPrefab, scene);
            GameObject ground = (GameObject)PrefabUtility.InstantiatePrefab(LoadPrefab("Ground"), scene);
            ground.name = "Ground";

            EnvironmentBuilder environment = systems.GetComponentInChildren<EnvironmentBuilder>();
            environment.AuthorWire(LoadPrefab("Tree"), LoadPrefab("Bush"), LoadPrefab("Rock"), ground.transform);

            GameBootstrap bootstrap = new GameObject("GameBootstrap").AddComponent<GameBootstrap>();
            bootstrap.AuthorWire(
                hud.GetComponent<GameHud>(),
                player.GetComponent<PlayerTokenView>(),
                rig.GetComponent<CameraDirector>(),
                systems.GetComponentInChildren<BoardBuilder>(),
                environment,
                systems.GetComponentInChildren<GameFlowPresenter>(),
                systems.GetComponentInChildren<DiceRollDirector>());

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
                
                if (existing) 
                    Object.DestroyImmediate(existing);
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
            GameObject root = new("CameraRig");

            GameObject cameraGo = new("Main Camera")
            {
                tag = "MainCamera"
            };

            cameraGo.transform.SetParent(root.transform);
            cameraGo.transform.position = new(0f, 7f, -7f);

            Camera camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new(0.62f, 0.8f, 0.92f);
            cameraGo.AddComponent<AudioListener>();

            UniversalAdditionalCameraData urpData = cameraGo.AddComponent<UniversalAdditionalCameraData>();
            urpData.renderPostProcessing = true;

            GameObject leaves = Object.Instantiate(LoadPrefab("LeavesParticle"), cameraGo.transform);
            leaves.name = "AmbientLeaves";
            leaves.transform.localPosition = new(0f, 6f, 4f);
            ParticleSystem.MainModule leavesMain = leaves.GetComponent<ParticleSystem>().main;
            leavesMain.playOnAwake = true;

            CameraDirector.Author(root, camera);
            return root;
        }

        private static GameObject AuthorSystems(GameObject tilePrefab)
        {
            GameObject root = new("Systems");

            PoolManager pool = new GameObject("PoolManager").AddComponent<PoolManager>();
            pool.transform.SetParent(root.transform);
            pool.AuthorWire(
                AssetDatabase.LoadAssetAtPath<GameObject>(EtfxDustPath).GetComponent<ParticleSystem>(),
                AssetDatabase.LoadAssetAtPath<GameObject>(EtfxBurstPath).GetComponent<ParticleSystem>(),
                LoadPrefab("Die").GetComponent<DieView>());

            GameObject flow = new("GameFlow");
            flow.transform.SetParent(root.transform);
            flow.AddComponent<DiceRollDirector>();
            flow.AddComponent<GameFlowPresenter>();

            BoardBuilder board = new GameObject("Board").AddComponent<BoardBuilder>();
            board.transform.SetParent(root.transform);
            board.AuthorWire(tilePrefab);

            EnvironmentBuilder environment = new GameObject("Environment").AddComponent<EnvironmentBuilder>();
            environment.transform.SetParent(root.transform);
            environment.AuthorWire(LoadPrefab("Tree"), LoadPrefab("Bush"), LoadPrefab("Rock"), null);

            GameObject eventSystem = new("EventSystem");
            eventSystem.transform.SetParent(root.transform);
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();

            return root;
        }

        private static void AuthorAmbience()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new(0.7f, 0.83f, 0.9f);
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance = 46f;
        }
    }
}
