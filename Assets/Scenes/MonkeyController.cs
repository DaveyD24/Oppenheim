using EventSystem;
using UnityEngine;
using static global::BatMathematics;
using static UnityEngine.InputSystem.InputAction;

public class MonkeyController : PlayerController
{
#if UNITY_EDITOR
    [field: Header("Start Reset")]
    [field: ContextMenuItem("Set Start Transform", "SetStartTransform")]
#pragma warning disable SA1202 // Elements should be ordered by access
    [field: SerializeField] public Vector3 StageStartPosition { get; set; }

    [field: ContextMenuItem("Move to Start", "MoveToStartTransform")]
    [field: SerializeField] public Quaternion StageStartRotation { get; set; }
#pragma warning restore SA1202 // Elements should be ordered by access

#endif

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
    [SerializeField] private GameObject monkeyCamera;
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
            Vector3 cameraRelativeDirection = DirectionRelativeToTransform(monkeyCamera.transform, move);
            AlignTransformToMovement(transform, cameraRelativeDirection, RotationSpeed, Vector3.up);
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
            if (AbilityUses > 0)
            {
                if (clinging)
                {
                    Rb.velocity += contactPoint.normal;
                    clinging = false;

                    // Stop any weird rotations.
                    Rb.angularVelocity = Vector3.zero;
                    AdjustAbilityValue(-1);
                }
            }
            else
            {
                clinging = false;
            }

            bDidJump = true;
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

    public override void OnDeath()
    {
        base.OnDeath();
        move = Vector3.zero;
        clinging = false;
        bDidJump = false;

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

        move = Vector3.zero;
        clinging = false;
        bDidJump = false;
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

        Vector3 cameraRelativeDirection = DirectionRelativeToTransform(monkeyCamera.transform, move);
        Rb.MovePosition(Rb.position + (MovementSpeed * Time.fixedDeltaTime * cameraRelativeDirection));
        DetermineAnimationState();
        speedometer.Mark();
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        // Only Cling to something if you're off the ground.
        if (AbilityUses > 0 && collision.transform.CompareTag("Clingable") && !IsGrounded())
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
            if (clinging)
            {
                animator.SetTrigger("Climb");
            }
            else if (IsZero(speedometer.Velocity))
            {
                animator.SetTrigger("Idle");
            }
            else
            {
                animator.SetTrigger("Walk");
            }
        }
    }

    private string RandomSound()
    {
        bool bRandomBool = Random.Range(0f, 1f) < .5f;

        return bRandomBool ? "Scream" : "OOH AHH";
    }

#if UNITY_EDITOR
    private void SetStartTransform()
    {
        StageStartPosition = transform.position;
        StageStartRotation = transform.rotation;
    }

    private void MoveToStartTransform()
    {
        transform.rotation = StageStartRotation;
        transform.position = StageStartPosition;
    }
#endif
}