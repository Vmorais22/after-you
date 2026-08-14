using UnityEngine;

namespace AfterYou.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerController : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float movementSpeed = 5f;

        private Rigidbody2D body;
        private Vector2 input;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1f);
        }

        private void FixedUpdate()
        {
            body.MovePosition(body.position + input * (movementSpeed * Time.fixedDeltaTime));
        }
    }
}
