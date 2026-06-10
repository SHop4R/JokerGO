using System.Collections.Generic;
using JokerGO.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JokerGO.Game
{
    /// <summary>
    /// Temporary scaffolding until the dice input UI lands (Day 3): R rolls two
    /// random dice, T rolls five. Delete together with the UI work.
    /// </summary>
    public sealed class DebugRollInput : MonoBehaviour
    {
        private GameSession session;

        public void Initialize(GameSession gameSession)
        {
            session = gameSession;
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null || session == null)
            {
                return;
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                Roll(2);
            }
            else if (keyboard.tKey.wasPressedThisFrame)
            {
                Roll(5);
            }
        }

        private void Roll(int diceCount)
        {
            var values = new List<int>(diceCount);
            for (int i = 0; i < diceCount; i++)
            {
                values.Add(Random.Range(DiceRules.MinValue, DiceRules.MaxValue + 1));
            }

            RollValidation result = session.TryRoll(values);
            Debug.Log(result.IsValid
                ? $"[JokerGO] Debug roll: {string.Join(", ", values)} (sum {DiceRules.Sum(values)})"
                : $"[JokerGO] Debug roll rejected: {result.Error}");
        }
    }
}
