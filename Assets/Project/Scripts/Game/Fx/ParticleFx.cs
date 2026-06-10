using UnityEngine;

namespace JokerGO.Game.Fx
{
    /// <summary>One-shot and ambient particle systems built entirely from code.</summary>
    public static class ParticleFx
    {
        private static Material particleMaterial;

        /// <summary>Radial burst of colored shards (item collect, celebrations).</summary>
        public static void Burst(Vector3 position, Color color, int count = 26)
        {
            ParticleSystem system = CreateSystem("FX Burst", position, color);

            ParticleSystem.MainModule main = system.main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.6f, 5.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.09f, 0.22f);
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.7f);
            main.gravityModifier = 1.4f;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.15f;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

            FinishOneShot(system, 1.2f);
        }

        /// <summary>Soft ground puff (landings, dice impacts).</summary>
        public static void Dust(Vector3 position, float scale = 1f)
        {
            Color dust = new Color(0.92f, 0.87f, 0.74f, 0.65f);
            ParticleSystem system = CreateSystem("FX Dust", position, dust);

            ParticleSystem.MainModule main = system.main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.7f * scale, 1.6f * scale);
            main.startSize = new ParticleSystem.MinMaxCurve(0.16f * scale, 0.34f * scale);
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
            main.gravityModifier = -0.05f;

            ParticleSystem.ShapeModule shape = system.shape;
            shape.shapeType = ParticleSystemShapeType.Hemisphere;
            shape.radius = 0.18f * scale;

            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)14) });

            ParticleSystem.ColorOverLifetimeModule fade = system.colorOverLifetime;
            fade.enabled = true;
            fade.color = FadeOutGradient(dust);

            FinishOneShot(system, 0.9f);
        }

        /// <summary>Gentle falling leaves around the play area; follows the given transform.</summary>
        public static ParticleSystem CreateAmbientLeaves(Transform follow)
        {
            Color leaf = new Color(0.74f, 0.85f, 0.4f, 0.85f);
            ParticleSystem system = CreateSystem("FX Ambient Leaves", follow.position, leaf);
            system.transform.SetParent(follow, worldPositionStays: false);
            system.transform.localPosition = new Vector3(0f, 6f, 4f);

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

            system.Play();
            return system;
        }

        private static ParticleSystem CreateSystem(string name, Vector3 position, Color color)
        {
            var go = new GameObject(name);
            go.transform.position = position;

            var system = go.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            ParticleSystem.MainModule main = system.main;
            main.startColor = color;
            main.playOnAwake = false;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = SharedParticleMaterial();
            return system;
        }

        private static Material SharedParticleMaterial()
        {
            if (particleMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                particleMaterial = new Material(shader);
            }

            return particleMaterial;
        }

        private static void FinishOneShot(ParticleSystem system, float life)
        {
            system.Play();
            Object.Destroy(system.gameObject, life);
        }

        private static Gradient FadeOutGradient(Color color)
        {
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new[]
                {
                    new GradientAlphaKey(color.a, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            return gradient;
        }
    }
}
