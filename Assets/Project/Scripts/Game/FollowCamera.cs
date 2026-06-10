using UnityEngine;

namespace JokerGO.Game
{
    /// <summary>Smoothly follows a target from behind and above, framed for portrait play.</summary>
    public sealed class FollowCamera : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = new Vector3(0f, 6.5f, -5.5f);
        [SerializeField] private Vector3 lookAhead = new Vector3(0f, 0.5f, 2f);
        [SerializeField] private float smoothTime = 0.35f;

        private Transform target;
        private Vector3 velocity;

        public void SetTarget(Transform newTarget, bool snap)
        {
            target = newTarget;
            if (snap && target != null)
            {
                transform.position = target.position + offset;
                LookAtTarget();
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            transform.position = Vector3.SmoothDamp(
                transform.position, target.position + offset, ref velocity, smoothTime);
            LookAtTarget();
        }

        private void LookAtTarget()
        {
            transform.LookAt(target.position + lookAhead);
        }
    }
}
