using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JokerGO.Game.Dice
{
    /// <summary>
    /// Spawns and arranges N dice in front of the camera, staggers their tumbles,
    /// and reports when every die has settled.
    /// </summary>
    public sealed class DiceRollDirector : MonoBehaviour
    {
        private const int DicePerRow = 5;
        private const float DieSize = 0.55f;
        private const float RestSpacing = 0.75f;
        private const float SpawnHeight = 2.4f;
        private const float TumbleDuration = 1.15f;
        private const float StaggerStep = 0.09f;
        private const float SettledBeat = 0.35f;
        private const float DismissDuration = 0.22f;

        private readonly List<DieView> activeDice = new List<DieView>();
        private Material bodyMaterial;

        private void Awake()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                throw new InvalidOperationException("URP Lit shader not found; is URP active?");
            }

            bodyMaterial = new Material(shader) { color = new Color(0.96f, 0.95f, 0.9f) };
        }

        private void OnDestroy()
        {
            if (bodyMaterial != null)
            {
                Destroy(bodyMaterial);
            }
        }

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

        /// <summary>Shrinks the settled dice away (used once movement starts).</summary>
        public void Dismiss()
        {
            foreach (DieView die in activeDice)
            {
                if (die != null)
                {
                    StartCoroutine(die.ShrinkOut(DismissDuration));
                }
            }

            activeDice.Clear();
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

                DieView die = DieView.Create(bodyMaterial, new Color(0.15f, 0.12f, 0.08f), DieSize);
                die.transform.position = rest
                                         + Vector3.up * SpawnHeight
                                         + new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), 0f,
                                             UnityEngine.Random.Range(-0.3f, 0.3f));
                activeDice.Add(die);

                pending.Add(StartCoroutine(die.TumbleTo(rest, values[i], TumbleDuration, onDieImpact)));
                remaining--;
                if (remaining > 0)
                {
                    yield return new WaitForSeconds(StaggerStep);
                }
            }

            foreach (Coroutine tumble in pending)
            {
                yield return tumble;
            }

            yield return new WaitForSeconds(SettledBeat);
            onAllSettled?.Invoke();
        }

        private void DismissImmediately()
        {
            foreach (DieView die in activeDice)
            {
                if (die != null)
                {
                    Destroy(die.gameObject);
                }
            }

            activeDice.Clear();
        }
    }
}
