using System;
using System.Collections;
using System.Collections.Generic;
using JokerGO.Game.Fx;
using UnityEngine;

namespace JokerGO.Game.Dice
{
    /// <summary>
    /// Arranges N pooled dice in front of the token, staggers their tumbles,
    /// and reports when every die has settled.
    /// </summary>
    public sealed class DiceRollDirector : MonoBehaviour
    {
        private const int DicePerRow = 5;
        private const float RestSpacing = 0.75f;
        private const float SpawnHeight = 2.4f;
        private const float TumbleDuration = 1.15f;
        private const float StaggerStep = 0.09f;
        private const float SettledBeat = 0.35f;
        private const float DismissDuration = 0.22f;

        private readonly List<DieView> activeDice = new List<DieView>();

        /// <summary>Plays the tumble for the given values; calls onAllSettled once every die rests.</summary>
        public void Show(IReadOnlyList<int> values, Vector3 trayCenter, Action onAllSettled,
            Action<Vector3> onDieImpact = null)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            DismissImmediately();
            StartCoroutine(ShowRoutine(values, trayCenter, onAllSettled, onDieImpact));
        }

        /// <summary>Shrinks the settled dice away and returns them to the pool.</summary>
        public void Dismiss()
        {
            foreach (DieView die in activeDice)
            {
                if (die != null)
                {
                    StartCoroutine(DismissRoutine(die));
                }
            }

            activeDice.Clear();
        }

        private IEnumerator DismissRoutine(DieView die)
        {
            yield return die.ShrinkOut(DismissDuration);
            PoolManager.Instance.ReturnDie(die);
        }

        private IEnumerator ShowRoutine(IReadOnlyList<int> values, Vector3 trayCenter,
            Action onAllSettled, Action<Vector3> onDieImpact)
        {
            int rows = Mathf.CeilToInt(values.Count / (float)DicePerRow);
            int remaining = values.Count;

            var pending = new List<Coroutine>(values.Count);
            for (int i = 0; i < values.Count; i++)
            {
                int row = i / DicePerRow;
                int column = i % DicePerRow;
                int diceInRow = Mathf.Min(DicePerRow, values.Count - row * DicePerRow);

                Vector3 rest = trayCenter
                               + Vector3.right * ((column - (diceInRow - 1) * 0.5f) * RestSpacing)
                               + Vector3.forward * ((row - (rows - 1) * 0.5f) * RestSpacing);

                Vector3 spawn = rest
                                + Vector3.up * SpawnHeight
                                + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0f,
                                    UnityEngine.Random.Range(-0.3f, 0.3f));

                DieView die = PoolManager.Instance.SpawnDie(spawn);
                activeDice.Add(die);

                pending.Add(StartCoroutine(die.TumbleTo(rest, values[i], TumbleDuration, onDieImpact)));
                remaining--;
                if (remaining > 0)
                {
                    yield return Utils.WaitHelper.WaitForSeconds(StaggerStep);
                }
            }

            foreach (Coroutine tumble in pending)
            {
                yield return tumble;
            }

            yield return Utils.WaitHelper.WaitForSeconds(SettledBeat);
            onAllSettled?.Invoke();
        }

        private void DismissImmediately()
        {
            foreach (DieView die in activeDice)
            {
                if (die != null)
                {
                    PoolManager.Instance.ReturnDie(die);
                }
            }

            activeDice.Clear();
        }
    }
}
