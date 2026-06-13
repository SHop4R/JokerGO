using System;
using JokerGO.Core;
using JokerGO.Game.Board;
using JokerGO.Game.Data;
using JokerGO.Game.Dice;
using JokerGO.UI;
using UnityEngine;

namespace JokerGO.Game
{
    /// <summary>
    /// Composition root: loads data, builds the domain session and wires the
    /// pre-authored scene objects (camera rig, player, HUD, systems) together.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        private const int TargetFrameRate = 60;

        [Header("Scene References")]
        [SerializeField] private GameHud hud;
        [SerializeField] private PlayerTokenView token;
        [SerializeField] private CameraDirector cameraDirector;
        [SerializeField] private BoardBuilder board;
        [SerializeField] private EnvironmentBuilder environment;
        [SerializeField] private GameFlowPresenter presenter;
        [SerializeField] private DiceRollDirector diceDirector;

        public GameSession Session { get; private set; }

        private void Awake()
        {
            Application.targetFrameRate = TargetFrameRate;
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
            RequireSceneReferences();

            IMapSource mapSource = new JsonMapSource();
            ISaveRepository saveRepository = new FileSaveRepository();

            BoardMap map = mapSource.Load();
            (Inventory inventory, int startTile) = SaveDataMapper.FromSaveData(saveRepository.Load(), map.TileCount);
            Session = new GameSession(map, saveRepository, inventory, startTile);

            BoardStyle style = BoardStyle.CreateDefault();
            board.Build(map, style);
            token.PlaceAt(board.TokenPositionOn(startTile));
            cameraDirector.SetFollowTarget(token.transform);

            hud.Initialize(Session);
            presenter.Initialize(Session, board, token, diceDirector, cameraDirector, hud, Camera.main);
            environment.Populate(board.TilePositions);

            Debug.Log($"[JokerGO] Board ready: {map.TileCount} tiles. Player on tile {Session.CurrentTileIndex + 1}.");
        }

        private void RequireSceneReferences()
        {
            Require(hud, nameof(hud));
            Require(token, nameof(token));
            Require(cameraDirector, nameof(cameraDirector));
            Require(board, nameof(board));
            Require(environment, nameof(environment));
            Require(presenter, nameof(presenter));
            Require(diceDirector, nameof(diceDirector));
        }

        private static void Require(UnityEngine.Object reference, string fieldName)
        {
            if (reference == null)
            {
                throw new InvalidOperationException(
                    $"Scene reference '{fieldName}' is not assigned; run JokerGO > Author Scene.");
            }
        }

        /// <summary>Editor-time wiring of all scene references.</summary>
        public void AuthorWire(GameHud gameHud, PlayerTokenView playerToken, CameraDirector director,
            BoardBuilder boardBuilder, EnvironmentBuilder environmentBuilder,
            GameFlowPresenter flowPresenter, DiceRollDirector dice)
        {
            hud = gameHud;
            token = playerToken;
            cameraDirector = director;
            board = boardBuilder;
            environment = environmentBuilder;
            presenter = flowPresenter;
            diceDirector = dice;
        }
    }
}
