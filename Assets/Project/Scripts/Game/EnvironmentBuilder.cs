using System.Collections.Generic;
using UnityEngine;

namespace JokerGO.Game
{
    /// <summary>
    /// Procedural orchard dressing around the path: prefab trees, bushes and rocks
    /// scattered deterministically (and path-aware) so every run looks the same.
    /// </summary>
    public sealed class EnvironmentBuilder : MonoBehaviour
    {
        private const int RandomSeed = 7;
        // Clearance is measured along world X from the nearest tile's x; on steep bends the
        // perpendicular distance shrinks (cos of the slope) and the nearest-tile snap adds
        // up to ~0.6 of error, so this is deliberately generous to keep canopies off tiles.
        private const float PathClearance = 3.9f;
        private const float ScatterWidth = 11f;

        private GameObject treePrefab;
        private GameObject bushPrefab;
        private GameObject rockPrefab;
        private IReadOnlyList<Vector3> path;

        public static EnvironmentBuilder Build(IReadOnlyList<Vector3> tilePositions)
        {
            var builder = new GameObject("Environment").AddComponent<EnvironmentBuilder>();
            builder.path = tilePositions;
            builder.treePrefab = Utils.PrefabLibrary.Load("Tree");
            builder.bushPrefab = Utils.PrefabLibrary.Load("Bush");
            builder.rockPrefab = Utils.PrefabLibrary.Load("Rock");
            builder.CreateGround(builder.PathEndZ());
            builder.Scatter(builder.PathEndZ());
            builder.TintWorld();
            return builder;
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

        private void CreateGround(float pathLength)
        {
            GameObject ground = Instantiate(Utils.PrefabLibrary.Load("Ground"), transform);
            ground.name = "Ground";
            ground.transform.localPosition = new Vector3(0f, -0.16f, pathLength * 0.5f);
            ground.transform.localScale = new Vector3(4f, 1f, (pathLength + 30f) / 10f);
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

        private void TintWorld()
        {
            Camera camera = Camera.main;
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.62f, 0.8f, 0.92f);
            }

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = new Color(0.7f, 0.83f, 0.9f);
            RenderSettings.fogStartDistance = 18f;
            RenderSettings.fogEndDistance = 46f;
        }
    }
}
