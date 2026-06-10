using System.Collections.Generic;
using UnityEngine;

namespace JokerGO.Game
{
    /// <summary>
    /// Procedural orchard dressing around the path: ground, trees, bushes and rocks,
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

        private Material trunkMaterial;
        private Material leavesMaterial;
        private Material groundMaterial;
        private Material rockMaterial;
        private IReadOnlyList<Vector3> path;

        public static EnvironmentBuilder Build(IReadOnlyList<Vector3> tilePositions)
        {
            var builder = new GameObject("Environment").AddComponent<EnvironmentBuilder>();
            builder.path = tilePositions;
            builder.CreateMaterials();
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

        private void OnDestroy()
        {
            foreach (Material material in new[] { trunkMaterial, leavesMaterial, groundMaterial, rockMaterial })
            {
                if (material != null)
                {
                    Destroy(material);
                }
            }
        }

        private void CreateMaterials()
        {
            trunkMaterial = CreateLit(new Color(0.45f, 0.3f, 0.2f));
            leavesMaterial = CreateLit(new Color(0.42f, 0.66f, 0.31f));
            groundMaterial = CreateLit(new Color(0.55f, 0.72f, 0.4f));
            rockMaterial = CreateLit(new Color(0.62f, 0.6f, 0.56f));
        }

        private void CreateGround(float pathLength)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(transform, false);
            ground.transform.localPosition = new Vector3(0f, -0.16f, pathLength * 0.5f);
            ground.transform.localScale = new Vector3(4f, 1f, (pathLength + 30f) / 10f);
            ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;
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
                CreateTree(position, 0.8f + (float)random.NextDouble() * 0.7f);
            }
            else if (kind < 0.8)
            {
                CreateBush(position, 0.5f + (float)random.NextDouble() * 0.5f);
            }
            else
            {
                CreateRock(position, 0.3f + (float)random.NextDouble() * 0.45f);
            }
        }

        private void CreateTree(Vector3 position, float size)
        {
            var root = new GameObject("Tree");
            root.transform.SetParent(transform, false);
            root.transform.position = position;

            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.transform.SetParent(root.transform, false);
            trunk.transform.localScale = new Vector3(0.22f, 0.5f, 0.22f) * size;
            trunk.transform.localPosition = Vector3.up * (0.5f * size);
            trunk.GetComponent<Renderer>().sharedMaterial = trunkMaterial;

            // Two stacked spheres read as a stylized low-poly canopy.
            for (int i = 0; i < 2; i++)
            {
                var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                canopy.transform.SetParent(root.transform, false);
                float layer = 1f - i * 0.28f;
                canopy.transform.localScale = new Vector3(1.4f, 1.1f, 1.4f) * (size * layer);
                canopy.transform.localPosition = Vector3.up * (size * (1.05f + i * 0.55f));
                canopy.GetComponent<Renderer>().sharedMaterial = leavesMaterial;
            }
        }

        private void CreateBush(Vector3 position, float size)
        {
            var bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bush.name = "Bush";
            bush.transform.SetParent(transform, false);
            bush.transform.position = position + Vector3.up * (size * 0.3f);
            bush.transform.localScale = new Vector3(1.2f, 0.75f, 1.2f) * size;
            bush.GetComponent<Renderer>().sharedMaterial = leavesMaterial;
        }

        private void CreateRock(Vector3 position, float size)
        {
            var rock = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rock.name = "Rock";
            rock.transform.SetParent(transform, false);
            rock.transform.position = position + Vector3.up * (size * 0.25f);
            rock.transform.localScale = new Vector3(1.1f, 0.6f, 0.9f) * size;
            rock.transform.localRotation = Quaternion.Euler(0f, size * 137f, 0f);
            rock.GetComponent<Renderer>().sharedMaterial = rockMaterial;
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

        private static Material CreateLit(Color color)
        {
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { color = color };
            return material;
        }
    }
}
