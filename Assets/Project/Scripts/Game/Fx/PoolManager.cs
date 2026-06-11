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
    /// </summary>
    public sealed class PoolManager : MonoSingleton<PoolManager>
    {
        private Pool<ParticleSystem> _dustPool;
        private Pool<ParticleSystem> _burstPool;
        private Pool<DieView> _dicePool;

        private Transform _dustParent;
        private Transform _burstParent;
        private Transform _diceParent;

        /// <summary>Builds the pools from prefabs; called once by the bootstrap.</summary>
        public void Initialize(ParticleSystem dustPrefab, ParticleSystem burstPrefab, DieView diePrefab)
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
            particle.transform.localScale = Vector3.one * scale;
            particle.Play();

            StartCoroutine(ReturnParticleAfterDuration(particle, () => _dustPool.Return(particle)));
        }

        public void PlayBurst(Vector3 position, Color color)
        {
            ParticleSystem particle = _burstPool.Spawn(position);
            ParticleSystem.MainModule main = particle.main;
            main.startColor = color;
            particle.Play();

            StartCoroutine(ReturnParticleAfterDuration(particle, () => _burstPool.Return(particle)));
        }

        public DieView SpawnDie(Vector3 position) => _dicePool.Spawn(position);

        public void ReturnDie(DieView die)
        {
            die.transform.SetParent(_diceParent);
            _dicePool.Return(die);
        }

        private static IEnumerator ReturnParticleAfterDuration(ParticleSystem ps, Action onReturn)
        {
            yield return WaitHelper.WaitForSeconds(ps.main.duration + ps.main.startLifetime.constantMax);
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
