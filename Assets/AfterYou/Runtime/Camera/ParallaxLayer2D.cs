using UnityEngine;

namespace AfterYou.CameraSystem
{
    [DefaultExecutionOrder(200)]
    public sealed class ParallaxLayer2D : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField, Range(0f, 1f)] private float horizontalFollow = 0.5f;
        [SerializeField, Range(0f, 1f)] private float verticalFollow;

        private Vector3 initialLayerPosition;
        private Vector3 initialCameraPosition;
        private bool initialized;

        public void Configure(Transform cameraTarget, float horizontalFactor, float verticalFactor)
        {
            cameraTransform = cameraTarget;
            horizontalFollow = Mathf.Clamp01(horizontalFactor);
            verticalFollow = Mathf.Clamp01(verticalFactor);
            Initialize();
        }

        private void Start()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            if (!initialized)
            {
                Initialize();
            }

            if (!initialized)
            {
                return;
            }

            var cameraDelta = cameraTransform.position - initialCameraPosition;
            transform.position = initialLayerPosition + new Vector3(
                cameraDelta.x * horizontalFollow,
                cameraDelta.y * verticalFollow,
                0f);
        }

        private void Initialize()
        {
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            if (cameraTransform == null)
            {
                return;
            }

            initialLayerPosition = transform.position;
            initialCameraPosition = cameraTransform.position;
            initialized = true;
        }
    }
}
