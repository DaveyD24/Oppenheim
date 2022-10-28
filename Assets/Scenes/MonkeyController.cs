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
    private bool hanging = false;
    private ContactPoint contactPoint;

    private Vector3 clingPosition;
    private Vector3 hangPosition;

    private bool bDidJump = false;
    private float currJumpWaitTime = 1;
    [SerializeField] private float jumpWaitTime = 1;
    [SerializeField] private GameObject ragdol;
    [SerializeField] private GameObject baseMesh;
    private BoxCollider boxCollider;

    // private bool bStillClinging = false; // if the monkey is still clinging even after jumping, as it never actually left the wall

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

        if (IsGrounded() && Rb.velocity.magnitude > 0.1f)
        {
            // Audio.PlayUnique("Footsteps", EAudioPlayOptions.AtTransformPosition | EAudioPlayOptions.DestroyOnEnd);
        }
        else
        {
            // Audio.StopCoroutine("Footsteps");
        }

        if (bDidJump)
        {
            currJumpWaitTime -= Time.deltaTime;

            if (currJumpWaitTime <= 0)
            {
                currJumpWaitTime = jumpWaitTime;
                bDidJump = false;

                // if still clinging to an object, even though the jump has finished
                // if (bStillClinging)
                // {
                //     if (AbilityUses > 0)
                //     {
                //         clinging = true;
                //     }
                //     else
                //     {
                //         bStillClinging = false;
                //         clinging = false;
                //     }
                // }
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

            if (hanging)
            {
                // Debug.Log("I am hanging");
                // this.transform.position = new Vector3(this.transform.position.x, hangPosition.y, this.transform.position.z);
                // this.transform.position = hangPosition;
                Rb.constraints = RigidbodyConstraints.FreezePositionY;
            }

            Rb.useGravity = !clinging && !hanging;
        }

        // Rotate towards Movement.
        if (Rb.useGravity)
        {
            Vector3 cameraRelativeDirection = DirectionRelativeToTransform(TrackingCamera.transform, move);

            // if walking backwards and the camera is inheriting, do not rotate around as its disorienting
            if (move.x == 0 && move.z < 0 && TrackingCamera.bInheritRotation)
            {
                cameraRelativeDirection *= -1;
            }

            AlignTransformToMovement(transform, cameraRelativeDirection, RotationSpeed * Time.deltaTime, Vector3.up);
        }
    }

    protected override void Movement(CallbackContext ctx)
    {
        if (!Active)
        {
            return;
        }

        move = ctx.ReadValue<Vector2>();

        move.x = AjustMovementValue(move.x);
        move.y = AjustMovementValue(move.y);

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
        if ((IsGrounded() || clinging || hanging) && !bDidJump)
        {
            Rb.velocity = Vector3.zero;
            // Original: Jump with a modified kinematic equation.
            Rb.velocity += new Vector3(0f, Mathf.Sqrt(jumpHeight * -3f * Physics.gravity.y), 0f);

            // If we were clinging onto something, we want to jump in the opposite direction
            // as if the Monkey is jumping off the wall.
            if (AbilityUses > 0)
            {
                if (clinging)
                {
                    Rb.velocity += contactPoint.normal;
                    clinging = false; // issue as if never actually exit the transform, it will never be entered again and thus never register as clinging

                    // Stop any weird rotations.
                    Rb.angularVelocity = Vector3.zero;
                    AdjustAbilityValue(-1);
                }

                if (hanging)
                {
                    Debug.Log("Unhang");
                    hanging = false;
                    Rb.constraints = RigidbodyConstraints.None;
                }

            }
            else
            {
                clinging = false;
                hanging = false;
            }

            bDidJump = true;
            currJumpWaitTime = jumpWaitTime;

            animator.SetTrigger("Jump");

            Audio.Play("Grunt", EAudioPlayOptions.FollowEmitter | EAudioPlayOptions.DestroyOnEnd);
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

    /// <summary>
    /// checks if jumping while also having been clung to an object.
    /// </summary>
    /// <returns>if it is cling jumping or not.</returns>
    public bool IsClingJump()
    {
        return animator.GetCurrentAnimatorStateInfo(0).IsName("ClimbJump") || clinging;
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
        hanging = false;
        base.Respawn();
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        GUI.Label(new Rect(100, 65, 250, 250), $"Clinging? {(clinging ? "Yes" : "No")}");
    }
#endif

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if (!Active)
        {
            move = Vector3.zero;
        }

        speedometer.Record(this);

        // give enough time to the climbing monkey to jump upwards before player movement can be used
        if (!(bDidJump && currJumpWaitTime > jumpWaitTime * 0.6f))
        {
            Vector3 cameraRelativeDirection = DirectionRelativeToTransform(TrackingCamera.transform, move);
            Rb.MovePosition(Rb.position + (MovementSpeed * Time.fixedDeltaTime * cameraRelativeDirection));
        }

        DetermineAnimationState();
        speedometer.Mark();
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        // Only Cling to something if you're off the ground.
        if (collision.transform.CompareTag("Clingable"))
        {
            if (AbilityUses > 0 && !IsGrounded())
            {
                clinging = true;
            }

            Audio.Play("Doof", EAudioPlayOptions.AtTransformPosition | EAudioPlayOptions.DestroyOnEnd);
            hanging = false;
            clingPosition = collision.collider.ClosestPoint(transform.position);
            contactPoint = collision.GetContact(0);

            // bStillClinging = true;
        }

        if (AbilityUses > 0 && collision.transform.CompareTag("Hangable") && !IsGrounded())
        {
            Audio.Play("Doof", EAudioPlayOptions.AtTransformPosition | EAudioPlayOptions.DestroyOnEnd);
            hanging = true;
            clinging = false;
            hangPosition = this.transform.position;
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

        if (collision.transform.CompareTag("Hangable"))
        {
            hanging = false;
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

    protected override void OnDustParticles(Vector3 Position)
    {
        base.OnDustParticles(Position);

        Audio.Play("Doof", EAudioPlayOptions.AtTransformPosition | EAudioPlayOptions.DestroyOnEnd);
    }

    protected override void PlayFuelCollectionSound()
    {
        Audio.Play("Munch", EAudioPlayOptions.Global | EAudioPlayOptions.DestroyOnEnd);
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