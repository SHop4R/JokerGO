using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace JokerGO.Game.Fx
{
    /// <summary>Runtime-built URP post-processing: soft bloom, vignette and warm grading.</summary>
    public static class PostFxBuilder
    {
        public static void Apply()
        {
            var go = new GameObject("PostFX Volume");
            Volume volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.priority = 10f;

            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();

            var bloom = profile.Add<Bloom>();
            bloom.intensity.Override(0.55f);
            bloom.threshold.Override(1.05f);
            bloom.scatter.Override(0.6f);

            var vignette = profile.Add<Vignette>();
            vignette.intensity.Override(0.24f);
            vignette.smoothness.Override(0.42f);

            var colors = profile.Add<ColorAdjustments>();
            colors.saturation.Override(12f);
            colors.contrast.Override(6f);
            colors.postExposure.Override(0.15f);

            volume.profile = profile;

            Camera camera = Camera.main;
            if (camera != null && camera.TryGetComponent(out UniversalAdditionalCameraData data))
            {
                data.renderPostProcessing = true;
            }
        }
    }
}
