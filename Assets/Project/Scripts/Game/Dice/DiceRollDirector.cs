using System;
using System.Collections;
using System.Collections.Generic;
using JokerGO.Game.Project.Scripts.Game.Fx;
using JokerGO.Game.Project.Scripts.Game.Utils;
using UnityEngine;
using UnityEngine.Pool;

namespace JokerGO.Game.Project.Scripts.Game.Dice
{
    /// <summary>
    /// Arranges N pooled dice in front of the token, staggers their tumbles,
    /// and reports when every die has settled.
    /// </summary>
    public sealed class DiceRollDirector : MonoBehaviour
    {
        private const float SpawnHeight = 2.4f;
        private const float TumbleDuration = 1.15f;
        private const float StaggerStep = 0.09f;
        private const float SettledBeat = 0.35f;
        private const float DismissDuration = 0.22f;

        private readonly List<DieView> activeDice = new List<DieView>();

        /// <summary>Tumbles one die onto each rest position; calls onAllSettled once every die rests.</summary>
        public void Show(IReadOnlyList<int> values, IReadOnlyList<Vector3> restPositions,
            Action onAllSettled, Action<Vector3> onDieImpact = null)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            if (restPositions == null || restPositions.Count != values.Count)
            {
                throw new ArgumentException("One rest position is required per die.", nameof(restPositions));
            }

            DismissImmediately();
            StartCoroutine(ShowRoutine(values, restPositions, onAllSettled, onDieImpact));
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

        private IEnumerator ShowRoutine(IReadOnlyList<int> values, IReadOnlyList<Vector3> restPositions,
            Action onAllSettled, Action<Vector3> onDieImpact)
        {
            List<Coroutine> pending = ListPool<Coroutine>.Get();
            for (int i = 0; i < values.Count; i++)
            {
                Vector3 rest = restPositions[i];
                Vector3 spawn = rest
                                + Vector3.up * SpawnHeight
                                + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0f,
                                    UnityEngine.Random.Range(-0.3f, 0.3f));

                DieView die = PoolManager.Instance.SpawnDie(spawn);
                activeDice.Add(die);

                pending.Add(StartCoroutine(die.TumbleTo(rest, values[i], TumbleDuration, onDieImpact)));

                bool isLastDie = i == values.Count - 1;
                if (!isLastDie)
                {
                    yield return WaitHelper.WaitForSeconds(StaggerStep);
                }
            }

            foreach (Coroutine tumble in pending)
            {
                yield return tumble;
            }

            ListPool<Coroutine>.Release(pending);

            yield return WaitHelper.WaitForSeconds(SettledBeat);
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
