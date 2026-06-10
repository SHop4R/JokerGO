using System;
using JokerGO.Core;
using JokerGO.Game.Board;
using JokerGO.Game.Data;
using JokerGO.Game.Dice;
using JokerGO.Game.Fx;
using JokerGO.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace JokerGO.Game
{
    /// <summary>
    /// Composition root: loads data, builds the domain session and all presentation
    /// objects in code, so the scene only needs this one component.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        public GameSession Session { get; private set; }

        private BoardStyle boardStyle;

        private void OnDestroy()
        {
            boardStyle?.DestroyMaterials();
        }

        private void Start()
        {
            try
            {
                Initialize();
            }
            catch (MapValidationException e)
            {
                Debug.LogError($"[JokerGO] Map error: {e.Message}");
                ErrorScreen.Show(e.Message,
                    "Fix Assets/StreamingAssets/map.json and restart the game.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[JokerGO] Failed to start: {e}");
                ErrorScreen.Show("Something went wrong while starting the game.",
                    "Check the console/log for details and restart the game.");
            }
        }

        private void Initialize()
        {
            IMapSource mapSource = new JsonMapSource();
            ISaveRepository saveRepository = new FileSaveRepository();

            BoardMap map = mapSource.Load();
            (Inventory inventory, int startTile) = SaveDataMapper.FromSaveData(saveRepository.Load(), map.TileCount);
            Session = new GameSession(map, saveRepository, inventory, startTile);

            boardStyle = BoardStyle.CreateDefault();
            var board = new GameObject("Board").AddComponent<BoardBuilder>();
            board.Build(map, boardStyle);

            PlayerTokenView token = PlayerTokenView.Create(
                board.TokenPositionOn(startTile), boardStyle.GetItemMaterial(ItemType.Strawberry));

            FollowCamera followCamera = AttachCamera(token.transform);

            EnsureEventSystem();
            GameHud hud = GameHud.Create(Session);

            var flow = new GameObject("GameFlow");
            var diceDirector = flow.AddComponent<DiceRollDirector>();
            flow.AddComponent<GameFlowPresenter>().Initialize(
                Session, board, token, diceDirector, followCamera, hud, Camera.main);

            EnvironmentBuilder.Build(board.TilePositions);
            PostFxBuilder.Apply();
            if (Camera.main != null)
            {
                ParticleFx.CreateAmbientLeaves(Camera.main.transform);
            }

            Debug.Log($"[JokerGO] Board ready: {map.TileCount} tiles. Player on tile {Session.CurrentTileIndex + 1}.");
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            }
        }

        private static FollowCamera AttachCamera(Transform target)
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                Debug.LogError("[JokerGO] No MainCamera in scene; cannot attach follow camera.");
                return null;
            }

            var follow = camera.GetComponent<FollowCamera>();
            if (follow == null)
            {
                follow = camera.gameObject.AddComponent<FollowCamera>();
            }

            follow.SetTarget(target, snap: true);
            return follow;
        }
    }
}
