using System;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace JokerGO.Game.ObjectPool
{
    /// <summary>
    /// Component pool over <see cref="UnityEngine.Pool.ObjectPool{T}"/>: spawns activate,
    /// returns deactivate, and <see cref="IPoolable"/> implementors get reset callbacks.
    /// </summary>
    public class Pool<T> where T : Component
    {
        private readonly IObjectPool<T> _pool;
        private readonly T _prefab;
        private readonly Transform _poolParent;

        public Pool(PoolStats<T> stats, Transform parent = null)
        {
            if (!stats.Prefab)
                throw new ArgumentNullException(nameof(stats.Prefab));

            _prefab = stats.Prefab;
            _poolParent = parent;

            int defaultCapacity = stats.DefaultPoolSize == 0 ? 1 : stats.DefaultPoolSize;
            int maxSize = stats.MaxPoolSize <= stats.DefaultPoolSize ? defaultCapacity * 2 : stats.MaxPoolSize;

            _pool = new ObjectPool<T>(
                OnObjectCreate,
                OnObjectGet,
                OnObjectRelease,
                OnObjectDestroy,
                true,
                defaultCapacity,
                maxSize);

            if (stats.PreGenerate)
                PreGenerate(stats.DefaultPoolSize);
        }

        public T Spawn(Vector3 position)
        {
            T obj = _pool.Get();
            obj.transform.position = position;
            return obj;
        }

        public void Return(T obj) => _pool.Release(obj);

        private T OnObjectCreate()
        {
            T instance = Object.Instantiate(_prefab);

            if (_poolParent)
                instance.transform.SetParent(_poolParent);

            return instance;
        }

        private static void OnObjectGet(T obj)
        {
            obj.gameObject.SetActive(true);

            if (obj is IPoolable poolable)
                poolable.OnSpawn();
        }

        private static void OnObjectRelease(T obj)
        {
            if (obj is IPoolable poolable)
                poolable.OnReturn();

            obj.gameObject.SetActive(false);
        }

        private static void OnObjectDestroy(T obj) => Object.Destroy(obj.gameObject);

        private void PreGenerate(int amount)
        {
            T[] generated = new T[amount];

            for (int i = 0; i < amount; i++)
            {
                generated[i] = _pool?.Get();
            }

            foreach (T obj in generated)
            {
                _pool?.Release(obj);
            }
        }
    }
}
