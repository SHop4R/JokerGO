using JokerGO.Game.Dice;
using UnityEditor;
using UnityEngine;

namespace JokerGO.Editor
{
    /// <summary>
    /// One-click generation of the project's material and prefab assets
    /// (environment props, the ambient leaves particle, the die) so runtime code
    /// only loads ready assets instead of assembling primitives.
    /// </summary>
    public static class ArtAssetGenerator
    {
        private const string MaterialsFolder = "Assets/Project/Resources/Materials";
        private const string PrefabsFolder = "Assets/Project/Resources/Prefabs";

        [MenuItem("JokerGO/Generate Art Assets")]
        public static void Generate()
        {
            EnsureFolders();

            Material trunk = LitMaterial("Trunk", new Color(0.45f, 0.3f, 0.2f));
            Material leaves = LitMaterial("Leaves", new Color(0.42f, 0.66f, 0.31f));
            Material ground = LitMaterial("Ground", new Color(0.55f, 0.72f, 0.4f));
            Material rock = LitMaterial("Rock", new Color(0.62f, 0.6f, 0.56f));
            LitMaterial("TileA", new Color(0.85f, 0.78f, 0.62f));
            LitMaterial("TileB", new Color(0.72f, 0.64f, 0.48f));
            LitMaterial("Apple", new Color(0.85f, 0.2f, 0.18f));
            LitMaterial("Pear", new Color(0.72f, 0.82f, 0.25f));
            LitMaterial("Strawberry", new Color(0.95f, 0.35f, 0.5f));
            Material dieBody = LitMaterial("DieBody", new Color(0.96f, 0.95f, 0.9f));
            Material particle = ParticleMaterial("ParticleUnlit");

            SavePrefab(BuildTree(trunk, leaves), "Tree");
            SavePrefab(BuildBush(leaves), "Bush");
            SavePrefab(BuildRock(rock), "Rock");
            SavePrefab(BuildGround(ground), "Ground");
            SavePrefab(BuildLeavesParticle(particle), "LeavesParticle");
            SavePrefab(BuildDie(dieBody), "Die");

            AssetDatabase.SaveAssets();
            Debug.Log("[JokerGO] Art assets generated under Assets/Project/Resources.");
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Project/Resources"))
                AssetDatabase.CreateFolder("Assets/Project", "Resources");
            if (!AssetDatabase.IsValidFolder(MaterialsFolder))
                AssetDatabase.CreateFolder("Assets/Project/Resources", "Materials");
            if (!AssetDatabase.IsValidFolder(PrefabsFolder))
                AssetDatabase.CreateFolder("Assets/Project/Resources", "Prefabs");
        }

        private const string ToonShaderAssetPath =
            "Assets/JMO Assets/Toony Colors Pro/Shaders/Hybrid 2/TCP2 Hybrid Shader 2.tcp2shader";
        private const string ToonShaderName = "Toony Colors Pro 2/Hybrid Shader 2";

        private static Material LitMaterial(string assetName, Color color)
        {
            return UpsertMaterial(assetName, FindToonShader(), color);
        }

        private static Shader FindToonShader()
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(ToonShaderAssetPath);
            if (shader == null)
            {
                shader = Shader.Find(ToonShaderName);
            }

            if (shader == null)
            {
                throw new System.InvalidOperationException(
                    "Toony Colors Pro 2 Hybrid Shader 2 not found; is the package imported?");
            }

            return shader;
        }

        private static Material ParticleMaterial(string assetName)
        {
            return UpsertMaterial(assetName,
                Shader.Find("Universal Render Pipeline/Particles/Unlit"), Color.white);
        }

        private static Material UpsertMaterial(string assetName, Shader shader, Color color)
        {
            string path = $"{MaterialsFolder}/{assetName}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                material = new Material(shader) { color = color };
                AssetDatabase.CreateAsset(material, path);
            }
            return material;
        }

        private static void SavePrefab(GameObject temp, string assetName)
        {
            PrefabUtility.SaveAsPrefabAsset(temp, $"{PrefabsFolder}/{assetName}.prefab");
            Object.DestroyImmediate(temp);
        }

        private static GameObject BuildTree(Material trunk, Material leaves)
        {
            var root = new GameObject("Tree");

            GameObject trunkGo = Primitive(PrimitiveType.Cylinder, root.transform, trunk);
            trunkGo.transform.localScale = new Vector3(0.22f, 0.5f, 0.22f);
            trunkGo.transform.localPosition = Vector3.up * 0.5f;

            for (int i = 0; i < 2; i++)
            {
                float layer = 1f - i * 0.28f;
                GameObject canopy = Primitive(PrimitiveType.Sphere, root.transform, leaves);
                canopy.transform.localScale = new Vector3(1.4f, 1.1f, 1.4f) * layer;
                canopy.transform.localPosition = Vector3.up * (1.05f + i * 0.55f);
            }

            return root;
        }

        private static GameObject BuildBush(Material leaves)
        {
            GameObject bush = Primitive(PrimitiveType.Sphere, null, leaves);
            bush.name = "Bush";
            bush.transform.localScale = new Vector3(1.2f, 0.75f, 1.2f);
            return bush;
        }

        private static GameObject BuildRock(Material rock)
        {
            GameObject go = Primitive(PrimitiveType.Cube, null, rock);
            go.name = "Rock";
            go.transform.localScale = new Vector3(1.1f, 0.6f, 0.9f);
            return go;
        }

        private static GameObject BuildGround(Material ground)
        {
            GameObject go = Primitive(PrimitiveType.Plane, null, ground);
            go.name = "Ground";
            return go;
        }

        private static GameObject BuildDie(Material body)
        {
            return DieView.ConstructTemplate(body, faceColor: new Color(0.15f, 0.12f, 0.08f), size: 0.55f);
        }

        private static GameObject BuildLeavesParticle(Material material)
        {
            ParticleSystem system = ParticleBase("LeavesParticle", material,
                new Color(0.74f, 0.85f, 0.4f, 0.85f));

            ParticleSystem.MainModule main = system.main;
            main.loop = true;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.25f, 0.7f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.13f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(7f, 11f);
            main.gravityModifier = 0.035f;
            main.maxParticles = 60;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(14f, 0.5f, 16f);

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 5f;

            ParticleSystem.RotationOverLifetimeModule spin = system.rotationOverLifetime;
            spin.enabled = true;
            spin.z = new ParticleSystem.MinMaxCurve(-2.4f, 2.4f);

            ParticleSystem.NoiseModule noise = system.noise;
            noise.enabled = true;
            noise.strength = 0.35f;
            noise.frequency = 0.3f;

            return system.gameObject;
        }

        private static ParticleSystem ParticleBase(string name, Material material, Color color)
        {
            var go = new GameObject(name);
            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = system.main;
            main.startColor = color;
            main.playOnAwake = false;
            main.loop = false;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            go.GetComponent<ParticleSystemRenderer>().sharedMaterial = material;
            return system;
        }

        private static GameObject Primitive(PrimitiveType type, Transform parent, Material material)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            go.GetComponent<Renderer>().sharedMaterial = material;

            Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }
    }
}
