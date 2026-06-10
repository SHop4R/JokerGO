using UnityEngine;

namespace JokerGO.Game
{
    /// <summary>Smoothly follows a target from behind and above, framed for portrait play.</summary>
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = new Vector3(0f, 6.5f, -5.5f);
        [SerializeField] private Vector3 lookAhead = new Vector3(0f, 0.5f, 2f);
        [SerializeField] private float smoothTime = 0.35f;

        private const float ShakeDecay = 4.5f;
        private const float ShakeFrequency = 28f;

        private Transform target;
        private Vector3 velocity;
        private float shakeStrength;
        private float shakeTime;

        public void SetTarget(Transform newTarget, bool snap)
        {
            target = newTarget;
            if (snap && target != null)
            {
                transform.position = target.position + offset;
                LookAtTarget();
            }
        }

        /// <summary>Adds a decaying positional shake on top of the follow motion.</summary>
        public void AddShake(float strength)
        {
            shakeStrength = Mathf.Max(shakeStrength, strength);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            transform.position = Vector3.SmoothDamp(
                transform.position, target.position + offset, ref velocity, smoothTime);

            if (shakeStrength > 0.001f)
            {
                shakeTime += Time.deltaTime * ShakeFrequency;
                shakeStrength = Mathf.MoveTowards(shakeStrength, 0f, ShakeDecay * shakeStrength * Time.deltaTime);
                transform.position += new Vector3(
                    (Mathf.PerlinNoise(shakeTime, 0.3f) - 0.5f),
                    (Mathf.PerlinNoise(0.7f, shakeTime) - 0.5f),
                    0f) * (2f * shakeStrength);
            }

            LookAtTarget();
        }

        private void LookAtTarget()
        {
            transform.LookAt(target.position + lookAhead);
        }
    }
}
