using Unity.Cinemachine;
using UnityEngine;

namespace JokerGO.Game
{
    /// <summary>
    /// Cinemachine rig: a damped follow camera with a screen-space dead zone (no more
    /// twitching while dice land), an impulse-based shake channel, and a second camera
    /// used to glide home before the token's wrap-around sky drop.
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

        /// <summary>How long callers should wait for the home glide to finish.</summary>
        public float HomeBlendDuration => HomeBlendSeconds;

        private CinemachineCamera followCam;
        private CinemachineCamera homeCam;
        private CinemachineCamera diceCam;
        private Transform homeAnchor;
        private Transform diceAnchor;
        private CinemachineImpulseSource impulseSource;

        public static CameraDirector Create(Transform followTarget)
        {
            var director = new GameObject("CameraDirector").AddComponent<CameraDirector>();

            Camera camera = Camera.main;
            if (camera != null && camera.GetComponent<CinemachineBrain>() == null)
            {
                CinemachineBrain brain = camera.gameObject.AddComponent<CinemachineBrain>();
                brain.DefaultBlend = new CinemachineBlendDefinition(
                    CinemachineBlendDefinition.Styles.EaseInOut, HomeBlendSeconds);
            }

            director.followCam = director.BuildComposedCamera("CM FollowCam", followTarget, 10);

            director.homeAnchor = new GameObject("CM HomeAnchor").transform;
            director.homeAnchor.SetParent(director.transform);
            director.homeCam = director.BuildComposedCamera("CM HomeCam", director.homeAnchor, 0);

            // Steeper, closer camera that frames the dice tray while they tumble.
            director.diceAnchor = new GameObject("CM DiceAnchor").transform;
            director.diceAnchor.SetParent(director.transform);
            director.diceCam = director.BuildComposedCamera("CM DiceCam", director.diceAnchor, 0,
                DicePitchDegrees, DiceCameraDistance);

            director.impulseSource = director.gameObject.AddComponent<CinemachineImpulseSource>();
            director.impulseSource.ImpulseDefinition.ImpulseDuration = 0.25f;

            return director;
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

        private CinemachineCamera BuildComposedCamera(string cameraName, Transform target, int priority,
            float pitchDegrees = PitchDegrees, float cameraDistance = CameraDistance)
        {
            var go = new GameObject(cameraName);
            go.transform.SetParent(transform);
            go.transform.rotation = Quaternion.Euler(pitchDegrees, 0f, 0f);

            var vcam = go.AddComponent<CinemachineCamera>();
            vcam.Follow = target;
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
