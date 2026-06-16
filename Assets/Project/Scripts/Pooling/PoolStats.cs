using System;
using UnityEngine;

namespace JokerGO.Pooling.Project.Scripts.Pooling
{
    /// <summary>Configuration for one <see cref="Pool{T}"/>: prefab, sizes and pre-generation.</summary>
    [Serializable]
    public struct PoolStats<T> where T : Component
    {
        [field: SerializeField] public T Prefab { get; private set; }
        [field: SerializeField] public int DefaultPoolSize { get; private set; }
        [field: SerializeField] public int MaxPoolSize { get; private set; }
        [field: SerializeField] public bool PreGenerate { get; private set; }

        public PoolStats(T prefab, int defaultPoolSize, int maxPoolSize, bool preGenerate)
        {
            Prefab = prefab;
            DefaultPoolSize = defaultPoolSize;
            MaxPoolSize = maxPoolSize;
            PreGenerate = preGenerate;
        }
    }
}
