using UnityEngine;

namespace AfterYou.CameraSystem
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new(0f, 2f, -10f);
        [SerializeField, Min(0.01f)] private float smoothing = 8f;
        [SerializeField] private Vector2 horizontalLimits = new(-45f, 45f);

        public void SetTarget(Transform value)
        {
            target = value;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var desired = target.position + offset;
            desired.x = Mathf.Clamp(desired.x, horizontalLimits.x, horizontalLimits.y);
            transform.position = Vector3.Lerp(
                transform.position,
                desired,
                1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime));
        }
    }
}
