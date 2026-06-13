using System;
using System.Collections.Generic;

namespace JokerGO.Core
{
    /// <summary>Turn phases. Presentation drives pacing; rules live in <see cref="GameSession"/>.</summary>
    public enum SessionState
    {
        Idle,
        Rolling,
        Moving
    }

    /// <summary>
    /// The rules engine for one play session. UI calls <see cref="TryRoll"/>, views animate
    /// what the raised events describe, then report back via the Notify* methods.
    /// </summary>
    public sealed class GameSession
    {
        public SessionState State { get; private set; } = SessionState.Idle;
        public int CurrentTileIndex { get; private set; }
        public Inventory Inventory { get; private set; }
        public BoardMap Map { get; }

        /// <summary>Roll accepted; carries the typed dice values to animate.</summary>
        public event Action<IReadOnlyList<int>> RollStarted;

        /// <summary>Dice shown; carries the tile indices the token must hop through.</summary>
        public event Action<IReadOnlyList<int>> MoveStarted;

        /// <summary>Token arrived; carries the tile it landed on.</summary>
        public event Action<MapTile> TileLanded;

        /// <summary>Reward collected; carries the new inventory and what was gained.</summary>
        public event Action<Inventory, ItemStack> ItemsCollected;

        /// <summary>Turn finished; the session accepts rolls again.</summary>
        public event Action TurnEnded;

        private readonly ISaveRepository saveRepository;
        private IReadOnlyList<int> pendingPath;

        public GameSession(BoardMap map, ISaveRepository saveRepository, Inventory inventory, int startTileIndex)
        {
            Map = map ?? throw new ArgumentNullException(nameof(map));
            this.saveRepository = saveRepository ?? throw new ArgumentNullException(nameof(saveRepository));
            Inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));

            if (startTileIndex < 0 || startTileIndex >= map.TileCount)
            {
                throw new ArgumentOutOfRangeException(nameof(startTileIndex), startTileIndex,
                    $"Start tile must be within [0, {map.TileCount - 1}].");
            }

            CurrentTileIndex = startTileIndex;
        }

        public RollValidation TryRoll(IReadOnlyList<int> diceValues)
        {
            if (State != SessionState.Idle)
            {
                return RollValidation.Fail("A turn is already in progress.");
            }

            RollValidation validation = DiceRules.Validate(diceValues);
            if (!validation.IsValid)
            {
                return validation;
            }

            int steps = DiceRules.Sum(diceValues);
            pendingPath = MovementCalculator.ComputePath(CurrentTileIndex, steps, Map.TileCount);
            State = SessionState.Rolling;
            RollStarted?.Invoke(diceValues);
            return RollValidation.Success;
        }

        /// <summary>Called by the dice view once the dice have settled.</summary>
        public void NotifyDiceShown()
        {
            RequireState(SessionState.Rolling, nameof(NotifyDiceShown));
            State = SessionState.Moving;
            MoveStarted?.Invoke(pendingPath);
        }

        /// <summary>Called by the token view once it has hopped the whole path.</summary>
        public void NotifyMoveCompleted()
        {
            RequireState(SessionState.Moving, nameof(NotifyMoveCompleted));

            CurrentTileIndex = pendingPath[pendingPath.Count - 1];
            pendingPath = null;

            MapTile landed = Map[CurrentTileIndex];
            TileLanded?.Invoke(landed);

            if (landed.HasReward)
            {
                ItemStack reward = landed.Reward.Value;
                Inventory = Inventory.Add(reward);
                ItemsCollected?.Invoke(Inventory, reward);
            }

            try
            {
                saveRepository.Save(SaveDataMapper.ToSaveData(Inventory, CurrentTileIndex));
            }
            finally
            {
                State = SessionState.Idle;
                TurnEnded?.Invoke();
            }
        }

        private void RequireState(SessionState expected, string operation)
        {
            if (State != expected)
            {
                throw new InvalidOperationException(
                    $"{operation} is only valid in the {expected} state, but the session is {State}.");
            }
        }
    }
}
