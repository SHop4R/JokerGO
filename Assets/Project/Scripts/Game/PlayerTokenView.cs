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

        public static PlayerTokenView Create(Vector3 tileAnchor, Material material)
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "PlayerToken";
            body.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            body.GetComponent<Renderer>().sharedMaterial = material;

            PlayerTokenView view = body.AddComponent<PlayerTokenView>();
            view.PlaceAt(tileAnchor);
            return view;
        }

        public void PlaceAt(Vector3 tileAnchor)
        {
            transform.position = StandingPosition(tileAnchor);
        }

        /// <summary>One hop onto a tile; squashes on landing when there is time to show it.</summary>
        public IEnumerator HopTo(Vector3 tileAnchor, float duration, bool squashOnLand)
        {
            yield return Tween.JumpTo(transform, StandingPosition(tileAnchor), HopArcHeight, duration);
            if (squashOnLand)
            {
                yield return Tween.Squash(transform, LandSquashFactor, LandSquashDuration);
            }
        }

        private static Vector3 StandingPosition(Vector3 tileAnchor)
        {
            return tileAnchor + Vector3.up * (BodyHeight * 0.5f);
        }
    }
}
