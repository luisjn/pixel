using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Movement : MonoBehaviour
{
    private Rigidbody2D _rigidbody;
    private Collision _collision;
    private float _horizontalInput;
    private float _verticalInput;
    private float _horizontalInputRaw;
    private float _verticalInputRaw;
    private bool _groundTouch;
    private bool _hasDashed;
    private Vector2 _facingDirection;
    private Camera _camera;
    public float horizontalMouseInput;
    public float verticalMouseInput;
    public float aimPosition;
    public Vector2 mousePosition;

    [SerializeField] private Transform aim;
    
    [Space]
    [Header("Stats")]
    [SerializeField] private float speed = 7;
    [SerializeField] private float jumpForce = 12;
    [SerializeField] private float slideSpeed = 1;
    [SerializeField] private float wallJumpLerp = 5;
    [SerializeField] private float dashSpeed = 40;
    
    [Space]
    [Header("Booleans")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private bool wallGrab;
    [SerializeField] private bool wallJumped;
    [SerializeField] private bool wallSlide;
    [SerializeField] private bool isDashing;
    
    [Space]
    public int side = 1;
    
    [Space]
    [Header("Polish")]
    public ParticleSystem dashParticle;
    public ParticleSystem jumpParticle;
    public ParticleSystem wallJumpParticle;
    
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _collision = GetComponent<Collision>();
        _camera = Camera.main;
    }

    private void Update()
    {
        _horizontalInput = Input.GetAxis("Horizontal");
        _verticalInput = Input.GetAxis("Vertical");
        _horizontalInputRaw = Input.GetAxisRaw("Horizontal");
        _verticalInputRaw = Input.GetAxisRaw("Vertical");
        horizontalMouseInput = Input.GetAxis("RightStick X");
        verticalMouseInput = Input.GetAxis("RightStick Y");
        var dir = new Vector2(_horizontalInput, _verticalInput);
        var position = transform.position;
        
        Walk(dir);

        mousePosition = Input.mousePosition;
        _facingDirection = _camera.ScreenToWorldPoint(mousePosition) - position;
        aim.position = position + (Vector3)_facingDirection.normalized;

        aimPosition += horizontalMouseInput * 5 * -Time.deltaTime;

        if (_collision.onWall && Input.GetButton("Fire3") && canMove)
        {
            // if(side != _collision.wallSide)
            //     anim.Flip(side*-1);
            wallGrab = true;
            wallSlide = false;
        }

        if (Input.GetButtonUp("Fire3") || !_collision.onWall || !canMove)
        {
            wallGrab = false;
            wallSlide = false;
        }
        
        if (_collision.onGround && !isDashing)
        {
            wallJumped = false;
            GetComponent<BetterJumping>().enabled = true;
        }
        
        if (wallGrab && !isDashing)
        {
            _rigidbody.gravityScale = 0;
            if(_horizontalInput > 0.2f || _horizontalInput < -0.2f)
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, 0);

            var speedModifier = _verticalInput > 0 ? 0.5f : 1;

            _rigidbody.velocity = new Vector2(_rigidbody.velocity.x, _verticalInput * (speed * speedModifier));
        }
        else
        {
            _rigidbody.gravityScale = 3;
        }
        
        if (_collision.onWall && !_collision.onGround)
        {
            if (_horizontalInput != 0 && !wallGrab)
            {
                wallSlide = true;
                WallSlide();
            }
        }
        
        if (!_collision.onWall || _collision.onGround)
            wallSlide = false;

        if (Input.GetButtonDown("Jump"))
        {
            if (_collision.onGround)
                Jump(Vector2.up, false);

            if (_collision.onWall && !_collision.onGround)
                WallJump();
        }
        
        if (Input.GetButtonDown("Fire1") && !_hasDashed)
        {
            if(_horizontalInputRaw != 0 || _verticalInputRaw != 0)
                Dash(_horizontalInputRaw, _verticalInputRaw);
        }
        
        if (_collision.onGround && !_groundTouch)
        {
            GroundTouch();
            _groundTouch = true;
        }
        
        if(!_collision.onGround && _groundTouch)
        {
            _groundTouch = false;
        }

        if (wallGrab || wallSlide || !canMove)
            return;
        
        if(_horizontalInput > 0)
        {
            side = 1;
            // anim.Flip(side);
        }
        if (_horizontalInput < 0)
        {
            side = -1;
            // anim.Flip(side);
        }
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove)
            return;
        
        if (wallGrab)
            return;

        if (!wallJumped)
        {
            _rigidbody.velocity = new Vector2(dir.x * speed, _rigidbody.velocity.y);
        }
        else
        {
            var velocity = _rigidbody.velocity;
            _rigidbody.velocity = Vector2.Lerp(velocity, new Vector2(dir.x * speed, velocity.y), wallJumpLerp * Time.deltaTime);
        }
    }
    
    private void Jump(Vector2 dir, bool wall)
    {
        var particle = wall ? wallJumpParticle : jumpParticle;
        var velocity = _rigidbody.velocity;
        velocity = new Vector2(velocity.x, 0);
        velocity += dir * jumpForce;
        _rigidbody.velocity = velocity;
        particle.Play();
    }
    
    private void WallSlide()
    {
        if (!canMove)
            return;

        var velocity = _rigidbody.velocity;
        var pushingWall = velocity.x > 0 && _collision.onRightWall || velocity.x < 0 && _collision.onLeftWall;
        var push = pushingWall ? 0 : velocity.x;
        
        velocity = new Vector2(velocity.x, -slideSpeed);
        _rigidbody.velocity = velocity;
    }

    private void WallJump()
    {
        if (side == 1 && _collision.onRightWall || side == -1 && !_collision.onRightWall)
        {
            side *= -1;
        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(0.1f));

        var wallDir = _collision.onRightWall ? Vector2.left : Vector2.right;

        Jump(Vector2.up / 1.5f + wallDir / 1.5f, true);

        wallJumped = true;
    }

    private IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    private void GroundTouch()
    {
        _hasDashed = false;
        isDashing = false;
        jumpParticle.Play();
    }
    
    private void Dash(float x, float y)
    {
        _camera.transform.DOComplete();
        _camera.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        FindObjectOfType<RippleEffect>().Emit(_camera.WorldToViewportPoint(transform.position));

        _hasDashed = true;

        // anim.SetTrigger("dash");

        var velocity = _rigidbody.velocity;
        velocity = Vector2.zero;
        var dir = new Vector2(x, y);

        velocity += dir.normalized * dashSpeed;
        _rigidbody.velocity = velocity;
        StartCoroutine(DashWait());
    }
    
    private IEnumerator DashWait()
    {
        StartCoroutine(GroundDash());
        DOVirtual.Float(14, 0, 0.8f, RigidbodyDrag);

        dashParticle.Play();
        _rigidbody.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(0.3f);

        dashParticle.Stop();
        _rigidbody.gravityScale = 3;
        GetComponent<BetterJumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
    }
    
    private IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(0.15f);
        if (_collision.onGround)
            _hasDashed = false;
    }
    
    private void RigidbodyDrag(float x)
    {
        _rigidbody.drag = x;
    }
}
