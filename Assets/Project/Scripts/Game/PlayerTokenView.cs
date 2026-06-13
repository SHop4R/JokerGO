using System.Collections;
using JokerGO.Game.Tweening;
using UnityEngine;

namespace JokerGO.Game
{
    /// <summary>The player's capsule token: stands on tiles and hops along movement paths.</summary>
    public sealed class PlayerTokenView : MonoBehaviour
    {
        private const float BodyHeight = 1f;
        private const float HopArcHeight = 0.5f;
        private const float LandSquashFactor = 0.82f;
        private const float LandSquashDuration = 0.09f;
        private const float IdleBobAmplitude = 0.045f;
        private const float IdleBobSpeed = 3.1f;

        private Vector3 restPosition;
        private Vector3 baseScale;
        private bool moving;

        private void Awake()
        {
            baseScale = transform.localScale;
        }

        /// <summary>Editor-time construction of the Player prefab.</summary>
        public static GameObject Author(Material material)
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Player";
            body.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            body.GetComponent<Renderer>().sharedMaterial = material;
            body.AddComponent<PlayerTokenView>();
            return body;
        }

        public void PlaceAt(Vector3 tileAnchor)
        {
            restPosition = StandingPosition(tileAnchor);
            transform.position = restPosition;
        }

        /// <summary>One hop onto a tile; squashes on landing when there is time to show it.</summary>
        public IEnumerator HopTo(Vector3 tileAnchor, float duration, bool squashOnLand)
        {
            moving = true;
            restPosition = StandingPosition(tileAnchor);
            yield return Tween.JumpTo(transform, restPosition, HopArcHeight, duration);
            if (squashOnLand)
            {
                yield return Tween.Squash(transform, LandSquashFactor, LandSquashDuration);
            }

            moving = false;
        }

        /// <summary>Wrap-around entrance: drop from the sky onto a tile with a hard squash.</summary>
        public IEnumerator FallFrom(Vector3 tileAnchor, float height, float duration)
        {
            moving = true;
            transform.localScale = baseScale;
            restPosition = StandingPosition(tileAnchor);
            transform.position = restPosition + Vector3.up * height;

            yield return Tween.MoveTo(transform, restPosition, duration, EaseType.EaseInQuad);
            yield return Tween.Squash(transform, 0.68f, 0.12f);
            moving = false;
        }

        private void Update()
        {
            if (moving)
            {
                return;
            }

            transform.position = restPosition
                                 + Vector3.up * (Mathf.Sin(Time.time * IdleBobSpeed) * IdleBobAmplitude);
        }

        private static Vector3 StandingPosition(Vector3 tileAnchor)
        {
            return tileAnchor + Vector3.up * (BodyHeight * 0.5f);
        }
    }
}
