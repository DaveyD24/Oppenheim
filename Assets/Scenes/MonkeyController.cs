using EventSystem;
using UnityEngine;
using static global::BatMathematics;
using static UnityEngine.InputSystem.InputAction;

public class MonkeyController : PlayerController
{

    private float jumpHeight = 2.0f;
    private Vector3 move;

    private Animator animator;
    private Speedometer speedometer;

    private bool clinging = false;
    private ContactPoint contactPoint;

    private Vector3 clingPosition;

    private bool bDidJump = false;
    private float currJumpWaitTime = 1;
    [SerializeField] private float jumpWaitTime = 1;
    [SerializeField] private GameObject ragdol;
    [SerializeField] private GameObject baseMesh;
    private BoxCollider boxCollider;

    private enum State
    {
        CLING,
        HANG,
        WALK,
    }

    protected override void Start()
    {
        base.Start();
        currJumpWaitTime = jumpWaitTime;
        animator = GetComponent<Animator>();
        boxCollider = gameObject.GetComponent<BoxCollider>();

        Audio.Play(RandomSound(), EAudioPlayOptions.AtTransformPosition | EAudioPlayOptions.DestroyOnEnd);
    }

    protected override void Update()
    {
        base.Update();

        if (bDidJump)
        {
            currJumpWaitTime -= Time.deltaTime;
            if (currJumpWaitTime <= 0)
            {
                currJumpWaitTime = jumpWaitTime;
                bDidJump = false;
            }
        }

        if (Active)
        {
            if (clinging)
            {
                transform.position = clingPosition;

                // Vector3 desiredPosition = transform.position - Vector3.up;
                // Vector3 gradual = Vector3.Lerp(transform.position, desiredPosition, 0.00125f);
                // transform.position = gradual;
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

    protected override void Movement(CallbackContext ctx)
    {
        if (!Active)
        {
            return;
        }

        move = ctx.ReadValue<Vector2>();

        // Convert 2D to 3D movement.
        move = new Vector3(move.x, 0, move.y).normalized;
    }

    protected override void Jump(CallbackContext ctx)
    {
        if (!Active)
        {
            return;
        }

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
            animator.SetTrigger("Jump");

            Audio.Play(RandomSound(), EAudioPlayOptions.FollowEmitter | EAudioPlayOptions.DestroyOnEnd);
        }
    }

    protected override void PerformAbility(CallbackContext ctx)
    {
        if (!Active)
        {
            return;
        }

        // ...
    }

    protected override void OnDeath()
    {
        base.OnDeath();
        baseMesh.SetActive(false);
        boxCollider.enabled = false;
        ragdol.SetActive(true);
        Rb.isKinematic = true;
        ragdol.transform.position = transform.position;
    }

    protected override void Respawn()
    {
        baseMesh.SetActive(true);
        boxCollider.enabled = true;
        ragdol.SetActive(false);
        Rb.isKinematic = false;
        base.Respawn();
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(100, 65, 250, 250), $"Clinging? {(clinging ? "Yes" : "No")}");
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        speedometer.Record(this);
        Rb.MovePosition(Rb.position + (MovementSpeed * Time.fixedDeltaTime * move));
        DetermineAnimationState();
        speedometer.Mark();
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

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

    private void DetermineAnimationState()
    {
        if (!bDidJump)
        {
            if (IsZero(speedometer.Velocity))
            {
                animator.SetTrigger("Idle");
            }
            else if (clinging)
            {
                animator.SetTrigger("Climb");
            }
            else
            {
                animator.SetTrigger("Walk");
            }
        }
    }

    string RandomSound()
    {
        bool bRandomBool = Random.Range(0f, 1f) < .5f;

        return bRandomBool ? "Scream":"OOH AHH";
    }
}