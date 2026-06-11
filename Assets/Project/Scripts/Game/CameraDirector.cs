using Unity.Cinemachine;
using UnityEngine;

namespace JokerGO.Game
{
    /// <summary>
    /// Cinemachine rig: a damped follow camera with a screen-space dead zone (no more
    /// twitching while dice land), an impulse-based shake channel, a dice close-up
    /// camera, and a home camera used before the token's wrap-around sky drop.
    /// The whole rig lives pre-configured in the scene as the CameraRig prefab.
    /// </summary>
    public sealed class CameraDirector : MonoBehaviour
    {
        private const float PitchDegrees = 39f;
        private const float CameraDistance = 9f;
        private const float HomeBlendSeconds = 1.0f;
        private const float DicePitchDegrees = 62f;
        private const float DiceCameraDistance = 5.5f;

        private static readonly Vector3 ComposerDamping = new Vector3(0.7f, 0.8f, 1.1f);
        private static readonly Vector2 DeadZoneSize = new Vector2(0.12f, 0.1f);
        private static readonly Vector3 AimTargetOffset = new Vector3(0f, 0.5f, 0f);

        [SerializeField] private CinemachineCamera followCam;
        [SerializeField] private CinemachineCamera homeCam;
        [SerializeField] private CinemachineCamera diceCam;
        [SerializeField] private Transform homeAnchor;
        [SerializeField] private Transform diceAnchor;
        [SerializeField] private CinemachineImpulseSource impulseSource;

        /// <summary>How long callers should wait for the home glide to finish.</summary>
        public float HomeBlendDuration => HomeBlendSeconds;

        /// <summary>Points the follow camera at the token (called once by the bootstrap).</summary>
        public void SetFollowTarget(Transform target)
        {
            followCam.Follow = target;
        }

        /// <summary>Camera bump for dice impacts and landings; decays via the impulse listener.</summary>
        public void Shake(float strength)
        {
            impulseSource.GenerateImpulse(Vector3.down * strength);
        }

        /// <summary>Glides the view to a world point (used before the wrap-around drop).</summary>
        public void FocusPoint(Vector3 position)
        {
            homeAnchor.position = position;
            homeCam.Priority = 20;
        }

        /// <summary>Hands the view back to the token follow camera.</summary>
        public void ResumeFollow()
        {
            homeCam.Priority = 0;
        }

        /// <summary>Zooms down onto the dice tray so the player sees the thrown values.</summary>
        public void FocusDice(Vector3 trayCenter)
        {
            diceAnchor.position = trayCenter;
            diceCam.Priority = 30;
        }

        /// <summary>Returns from the dice close-up to the token follow camera.</summary>
        public void ResumeFromDice()
        {
            diceCam.Priority = 0;
        }

        /// <summary>Editor-time construction of the full rig under the given root.</summary>
        public static CameraDirector Author(GameObject rigRoot, Camera camera)
        {
            CinemachineBrain brain = camera.GetComponent<CinemachineBrain>();
            if (brain == null)
            {
                brain = camera.gameObject.AddComponent<CinemachineBrain>();
            }

            brain.DefaultBlend = new CinemachineBlendDefinition(
                CinemachineBlendDefinition.Styles.EaseInOut, HomeBlendSeconds);

            var director = rigRoot.AddComponent<CameraDirector>();
            director.followCam = AuthorComposedCamera(rigRoot.transform, "CM FollowCam", 10,
                PitchDegrees, CameraDistance);

            director.homeAnchor = new GameObject("CM HomeAnchor").transform;
            director.homeAnchor.SetParent(rigRoot.transform);
            director.homeCam = AuthorComposedCamera(rigRoot.transform, "CM HomeCam", 0,
                PitchDegrees, CameraDistance);
            director.homeCam.Follow = director.homeAnchor;

            director.diceAnchor = new GameObject("CM DiceAnchor").transform;
            director.diceAnchor.SetParent(rigRoot.transform);
            director.diceCam = AuthorComposedCamera(rigRoot.transform, "CM DiceCam", 0,
                DicePitchDegrees, DiceCameraDistance);
            director.diceCam.Follow = director.diceAnchor;

            director.impulseSource = rigRoot.AddComponent<CinemachineImpulseSource>();
            director.impulseSource.ImpulseDefinition.ImpulseDuration = 0.25f;

            return director;
        }

        private static CinemachineCamera AuthorComposedCamera(Transform parent, string cameraName,
            int priority, float pitchDegrees, float cameraDistance)
        {
            var go = new GameObject(cameraName);
            go.transform.SetParent(parent);
            go.transform.rotation = Quaternion.Euler(pitchDegrees, 0f, 0f);

            var vcam = go.AddComponent<CinemachineCamera>();
            vcam.Priority = priority;

            var composer = go.AddComponent<CinemachinePositionComposer>();
            composer.CameraDistance = cameraDistance;
            composer.Damping = ComposerDamping;
            composer.TargetOffset = AimTargetOffset;
            composer.Composition.DeadZone.Enabled = true;
            composer.Composition.DeadZone.Size = DeadZoneSize;

            go.AddComponent<CinemachineImpulseListener>();
            return vcam;
        }
    }
}
