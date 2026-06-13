using System;
using System.Collections;
using JokerGO.Game.Dice;
using JokerGO.Game.ObjectPool;
using JokerGO.Game.Utils;
using UnityEngine;

namespace JokerGO.Game.Fx
{
    /// <summary>
    /// Central pool owner: dust/burst particles and dice are spawned from pools and
    /// returned automatically, so gameplay never calls Instantiate/Destroy directly.
    /// Lives pre-configured in the scene with its prefab references assigned.
    /// </summary>
    public sealed class PoolManager : MonoSingleton<PoolManager>
    {
        [Header("Pooled Prefabs")]
        [SerializeField] private ParticleSystem dustPrefab;
        [SerializeField] private ParticleSystem burstPrefab;
        [SerializeField] private DieView diePrefab;

        private Pool<ParticleSystem> _dustPool;
        private Pool<ParticleSystem> _burstPool;
        private Pool<DieView> _dicePool;

        private Transform _dustParent;
        private Transform _burstParent;
        private Transform _diceParent;

        private void Awake()
        {
            _dustParent = CreateParent("--- Dust Particles ---");
            _burstParent = CreateParent("--- Burst Particles ---");
            _diceParent = CreateParent("--- Dice ---");

            _dustPool = new(new PoolStats<ParticleSystem>(dustPrefab, 8, 24, true), _dustParent);
            _burstPool = new(new PoolStats<ParticleSystem>(burstPrefab, 2, 8, true), _burstParent);
            _dicePool = new(new PoolStats<DieView>(diePrefab, 5, 20, true), _diceParent);
        }

        public void PlayDust(Vector3 position, float scale = 1f)
        {
            ParticleSystem particle = _dustPool.Spawn(position);
            EnsureHierarchyScaling(particle);
            particle.transform.localScale = Vector3.one * scale;
            particle.Play();

            StartCoroutine(ReturnParticleAfterDuration(particle, () => _dustPool.Return(particle)));
        }

        public void PlayBurst(Vector3 position, Color color)
        {
            ParticleSystem particle = _burstPool.Spawn(position);
            EnsureHierarchyScaling(particle);

            foreach (ParticleSystem system in particle.GetComponentsInChildren<ParticleSystem>())
            {
                ParticleSystem.MainModule main = system.main;
                main.startColor = color;
            }

            particle.Play();

            StartCoroutine(ReturnParticleAfterDuration(particle, () => _burstPool.Return(particle)));
        }

        /// <summary>Third-party effects ship with varied scaling modes; hierarchy scaling
        /// lets one pooled prefab serve small and large moments via transform scale.</summary>
        private static void EnsureHierarchyScaling(ParticleSystem root)
        {
            foreach (ParticleSystem system in root.GetComponentsInChildren<ParticleSystem>())
            {
                ParticleSystem.MainModule main = system.main;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            }
        }

        public DieView SpawnDie(Vector3 position) => _dicePool.Spawn(position);

        public void ReturnDie(DieView die)
        {
            die.transform.SetParent(_diceParent);
            _dicePool.Return(die);
        }

        /// <summary>Editor-time wiring of the pooled prefab references.</summary>
        public void AuthorWire(ParticleSystem dust, ParticleSystem burst, DieView die)
        {
            dustPrefab = dust;
            burstPrefab = burst;
            diePrefab = die;
        }

        private static IEnumerator ReturnParticleAfterDuration(ParticleSystem ps, Action onReturn)
        {
            float longest = 0f;
            foreach (ParticleSystem system in ps.GetComponentsInChildren<ParticleSystem>())
            {
                longest = Mathf.Max(longest, system.main.duration + system.main.startLifetime.constantMax);
            }

            yield return WaitHelper.WaitForSeconds(longest);
            onReturn.Invoke();
        }

        private Transform CreateParent(string parentName)
        {
            Transform parent = new GameObject(parentName).transform;
            parent.SetParent(transform);
            return parent;
        }
    }
}
