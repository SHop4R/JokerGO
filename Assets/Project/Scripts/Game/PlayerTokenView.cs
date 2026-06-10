using UnityEngine;

namespace JokerGO.Game
{
    /// <summary>Placeholder player token: a capsule standing on the current tile (hops arrive on Day 2).</summary>
    public sealed class PlayerTokenView : MonoBehaviour
    {
        private const float BodyHeight = 1f;

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
            transform.position = tileAnchor + Vector3.up * (BodyHeight * 0.5f);
        }
    }
}
