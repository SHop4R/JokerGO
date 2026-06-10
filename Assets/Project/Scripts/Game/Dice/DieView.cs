using System.Collections;
using JokerGO.Game.Tweening;
using TMPro;
using UnityEngine;

namespace JokerGO.Game.Dice
{
    /// <summary>
    /// One die: a cube with numbered faces and a scripted tumble that always
    /// settles showing the requested value. No physics — fully deterministic.
    /// </summary>
    public sealed class DieView : MonoBehaviour
    {
        private const float FaceLabelInset = 0.005f;
        private const float FlyPhase = 0.45f;
        private const float BouncePhase = 0.35f;

        public static DieView Create(Material bodyMaterial, Color faceColor, float size)
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Die";
            body.transform.localScale = Vector3.one * size;
            body.GetComponent<Renderer>().sharedMaterial = bodyMaterial;

            for (int value = 1; value <= 6; value++)
            {
                AddFaceLabel(body.transform, value, faceColor);
            }

            return body.AddComponent<DieView>();
        }

        /// <summary>Arc to the rest position, bounce twice, settle with the value face-up.</summary>
        public IEnumerator TumbleTo(Vector3 restPosition, int value, float duration)
        {
            Quaternion finalRotation = DieFaceLayout.RotationShowing(value, Random.Range(0f, 360f));
            Vector3 spinAxis = Random.onUnitSphere;
            float spinSpeed = Random.Range(540f, 900f);

            Vector3 start = transform.position;
            float flyDuration = duration * FlyPhase;
            float bounceDuration = duration * BouncePhase;
            float settleDuration = duration * (1f - FlyPhase - BouncePhase);

            // Fly in on an arc while spinning freely.
            float elapsed = 0f;
            while (elapsed < flyDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / flyDuration);
                Vector3 flat = Vector3.Lerp(start, restPosition, Easing.Evaluate(EaseType.EaseOutQuad, t));
                transform.position = flat + Vector3.up * (1.1f * 4f * t * (1f - t));
                transform.rotation = Quaternion.AngleAxis(spinSpeed * Time.deltaTime, spinAxis) * transform.rotation;
                yield return null;
            }

            // Two shrinking bounces; the spin decays and starts blending toward the target face.
            Quaternion bounceStartRotation = transform.rotation;
            elapsed = 0f;
            while (elapsed < bounceDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / bounceDuration);
                float bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2f)) * Mathf.Lerp(0.4f, 0.05f, t);
                transform.position = restPosition + Vector3.up * bounce;
                transform.rotation = Quaternion.Slerp(
                    bounceStartRotation, finalRotation, Easing.Evaluate(EaseType.EaseInQuad, t * 0.7f));
                yield return null;
            }

            // Settle: lock position, finish the rotation blend.
            yield return Tween.RotateTo(transform, finalRotation, settleDuration, EaseType.EaseOutBack);
            transform.position = restPosition;
            transform.rotation = finalRotation;
        }

        public IEnumerator ShrinkOut(float duration)
        {
            yield return Tween.ScaleTo(transform, Vector3.zero, duration, EaseType.EaseInQuad);
            Destroy(gameObject);
        }

        private static void AddFaceLabel(Transform body, int value, Color color)
        {
            var go = new GameObject($"Face {value}");
            go.transform.SetParent(body, false);

            Vector3 normal = DieFaceLayout.NormalOf(value);
            go.transform.localPosition = normal * (0.5f + FaceLabelInset);

            // TMP text reads correctly when its -Z axis matches the face normal.
            Vector3 up = Mathf.Abs(normal.y) > 0.5f ? Vector3.forward : Vector3.up;
            go.transform.localRotation = Quaternion.LookRotation(-normal, up);

            var tmp = go.AddComponent<TextMeshPro>();
            // The 6 is underlined like on real dice, so a random yaw can't read as a 9.
            tmp.text = value == 6 ? "<u>6</u>" : value.ToString();
            tmp.fontSize = 5f;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.rectTransform.sizeDelta = new Vector2(1f, 1f);
        }
    }
}
