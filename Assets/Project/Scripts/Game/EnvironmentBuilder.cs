using System.Collections.Generic;
using UnityEngine;

namespace JokerGO.Game
{
    /// <summary>
    /// Procedural orchard dressing around the path: prefab trees, bushes and rocks
    /// scattered deterministically (and path-aware) so every run looks the same.
    /// Lives in the scene with its prefab and ground references pre-assigned.
    /// </summary>
    public sealed class EnvironmentBuilder : MonoBehaviour
    {
        private const int RandomSeed = 7;
        // Clearance is measured along world X from the nearest tile's x; on steep bends the
        // perpendicular distance shrinks (cos of the slope) and the nearest-tile snap adds
        // up to ~0.6 of error, so this is deliberately generous to keep canopies off tiles.
        private const float PathClearance = 3.9f;
        private const float ScatterWidth = 11f;

        [SerializeField] private GameObject treePrefab;
        [SerializeField] private GameObject bushPrefab;
        [SerializeField] private GameObject rockPrefab;
        [SerializeField] private Transform ground;

        private IReadOnlyList<Vector3> path;

        /// <summary>Sizes the ground to the path and scatters the props; called by the bootstrap.</summary>
        public void Populate(IReadOnlyList<Vector3> tilePositions)
        {
            path = tilePositions;
            float endZ = PathEndZ();

            ground.localPosition = new Vector3(0f, -0.16f, endZ * 0.5f);
            ground.localScale = new Vector3(4f, 1f, (endZ + 30f) / 10f);

            Scatter(endZ);
        }

        /// <summary>Editor-time wiring of prop prefabs and the scene ground instance.</summary>
        public void AuthorWire(GameObject tree, GameObject bush, GameObject rock, Transform groundInstance)
        {
            treePrefab = tree;
            bushPrefab = bush;
            rockPrefab = rock;
            ground = groundInstance;
        }

        private float PathEndZ() => path.Count == 0 ? 0f : path[path.Count - 1].z;

        /// <summary>Path x at a given z, from the nearest tile (the serpentine is monotonic in z).</summary>
        private float PathCenterX(float z)
        {
            float bestX = 0f;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < path.Count; i++)
            {
                float distance = Mathf.Abs(path[i].z - z);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestX = path[i].x;
                }
            }

            return bestX;
        }

        private void Scatter(float pathLength)
        {
            var random = new System.Random(RandomSeed);
            for (float z = -6f; z < pathLength + 10f; z += 1.9f)
            {
                PlaceProp(random, z);
            }
        }

        private void PlaceProp(System.Random random, float z)
        {
            float side = random.Next(2) == 0 ? -1f : 1f;
            float offset = side * Mathf.Lerp(PathClearance, ScatterWidth, (float)random.NextDouble());
            float propZ = z + (float)random.NextDouble() * 1.4f;
            var position = new Vector3(PathCenterX(propZ) + offset, 0f, propZ);

            double kind = random.NextDouble();
            if (kind < 0.55)
            {
                Place(treePrefab, position, 0.8f + (float)random.NextDouble() * 0.7f, random);
            }
            else if (kind < 0.8)
            {
                Place(bushPrefab, position, 0.5f + (float)random.NextDouble() * 0.5f, random,
                    yOffsetFactor: 0.3f);
            }
            else
            {
                Place(rockPrefab, position, 0.3f + (float)random.NextDouble() * 0.45f, random,
                    yOffsetFactor: 0.25f);
            }
        }

        private void Place(GameObject prefab, Vector3 position, float size, System.Random random,
            float yOffsetFactor = 0f)
        {
            GameObject prop = Instantiate(prefab, transform);
            prop.transform.position = position + Vector3.up * (size * yOffsetFactor);
            prop.transform.localScale = prefab.transform.localScale * size;
            prop.transform.localRotation = Quaternion.Euler(0f, (float)random.NextDouble() * 360f, 0f);
        }
    }
}
