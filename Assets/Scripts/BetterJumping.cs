using UnityEngine;

public class BetterJumping : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    
    [SerializeField] private float fallMultiplier = 3;
    [SerializeField] private float lowJumpMultiplier = 8;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (_rigidbody.velocity.y < 0)
        {
            _rigidbody.velocity += Vector2.up * (Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime);
        } else if (_rigidbody.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            _rigidbody.velocity += Vector2.up * (Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime);
        }
    }
}