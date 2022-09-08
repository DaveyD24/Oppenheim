using EventSystem;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;
using static global::BatMathematics;

public class MonkeyController : PlayerController
{
    enum State
    {
        CLING,
        HANG,
        WALK
    }

    private float jumpHeight = 2.0f;
    Vector3 move;

    bool clinging = false;
    ContactPoint contactPoint;

    private Vector3 clingPosition;

    private bool bDidJump = false;
    private float jumpWaitTime = 1;

    protected override void OnEnable()
    {
        base.OnEnable();

        UIEvents.OnShowIntructions += ShowInfo;
        GameEvents.OnDie += Respawn;
    }

    private void Respawn()
    {
        transform.position = startPosition;
        transform.rotation = startRotation;
    }

    private void ShowInfo()
    {
        Oppenheim.SetActive(true);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        UIEvents.OnShowIntructions -= ShowInfo;
        GameEvents.OnDie -= Respawn;
    }

    protected override void Start()
    {
        base.Start();

        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    protected override void Update()
    {
        base.Update();

        if (bDidJump)
        {
            jumpWaitTime -= Time.deltaTime;
            if (jumpWaitTime <= 0)
            {
                jumpWaitTime = 1;
                bDidJump = false;
            }
        }

        if (active)
        {
            if (clinging)
            {
                transform.position = clingPosition;
                //Vector3 desiredPosition = transform.position - Vector3.up;
                //Vector3 gradual = Vector3.Lerp(transform.position, desiredPosition, 0.00125f);
                //transform.position = gradual;
                Rb.velocity = Vector3.zero;
            }

            Rb.useGravity = !clinging;
        }
        
        // Rotate towards Movement.
        if (move != Vector3.zero)
        {
            AlignTransformToMovement(transform, move, RotationSpeed, Vector3.up);
        }
    }

    private void FixedUpdate()
    {
        Rb.MovePosition(Rb.position + (MovementSpeed * Time.fixedDeltaTime * move));
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Only Cling to something if you're off the ground.
        if (collision.transform.CompareTag("Clingable") && !IsGrounded())
        {
                Debug.Log("dsadfs");
                clinging = true;
                clingPosition = collision.collider.ClosestPoint(transform.position);
                contactPoint = collision.GetContact(0);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Only stop clinging with the Object we stop colliding with was
        // a Clingable surface.
        if (collision.transform.CompareTag("Clingable"))
    	{
            clinging = false;
    	}
    }

    protected override void Movement(CallbackContext ctx)
    {
        if (!active)
            return;

        move = ctx.ReadValue<Vector2>();

        // Convert 2D to 3D movement.
        move = new Vector3(move.x, 0f, move.y).normalized;
    }

    protected override void Jump(CallbackContext ctx)
    {
        if (!active)
            return;

        // This check is original and untouched.
        if ((IsGrounded() || clinging) && !bDidJump)
        {
            // Original: Jump with a modified kinematic equation.
            Rb.velocity += new Vector3(0f, Mathf.Sqrt(jumpHeight * -3f * Physics.gravity.y), 0f);
            
            // If we were clinging onto something, we want to jump in the opposite direction
            // as if the Monkey is jumping off the wall.
            if (clinging)
            {
                Rb.velocity += contactPoint.normal;
                clinging = false;

                // Stop any weird rotations.
                Rb.angularVelocity = Vector3.zero;
            }

            bDidJump = true;
            Debug.Log("yeet");
        }
    }

    protected override void PerformAbility(CallbackContext ctx)
    {
        if (!active)
            return;

        // ...
    }

    void OnGUI()
    {
        GUI.Label(new Rect(100, 65, 250, 250), $"Clinging? {(clinging ? "Yes" : "No")}");
    }
}