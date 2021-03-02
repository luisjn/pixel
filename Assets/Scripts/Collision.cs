using UnityEngine;

public class Collision : MonoBehaviour
{
    [Header("Layers")]
    public LayerMask groundLayer;

    [Space]
    public bool onGround;
    public bool onWall;
    public bool onRightWall;
    public bool onLeftWall;
    public int wallSide;
    
    [Space]
    [Header("Collision")]
    [SerializeField] private float collisionRadius = 0.1f;
    [SerializeField] private Vector2 rightOffset, leftOffset, bottomOffset;

    private void Update()
    {
        var position = transform.position;
        onGround = Physics2D.OverlapCircle((Vector2)position + bottomOffset, collisionRadius, groundLayer);
        onWall = Physics2D.OverlapCircle((Vector2)position + rightOffset, collisionRadius, groundLayer) 
                 || Physics2D.OverlapCircle((Vector2)position + leftOffset, collisionRadius, groundLayer);

        onRightWall = Physics2D.OverlapCircle((Vector2)position + rightOffset, collisionRadius, groundLayer);
        onLeftWall = Physics2D.OverlapCircle((Vector2)position + leftOffset, collisionRadius, groundLayer);

        wallSide = onRightWall ? -1 : 1;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        var positions = new [] { rightOffset, leftOffset, bottomOffset };

        var position = transform.position;
        Gizmos.DrawWireSphere((Vector2)position + rightOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)position + leftOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)position  + bottomOffset, collisionRadius);
    }
}