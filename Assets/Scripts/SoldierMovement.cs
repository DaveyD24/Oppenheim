using EventSystem;
using UnityEngine;
using static global::BatMathematics;
using static UnityEngine.InputSystem.InputAction;

public class SoldierMovement : PlayerController
{
    private Vector3 playerVelocity;
    private Vector3 move;
    private Animator animator;
    private Speedometer speedometer;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private GameObject ragdol;
    [SerializeField] private GameObject baseMesh;
    private BoxCollider boxCollider;

    [field: Header("Soldier Movement")]
    [field: SerializeField] public Transform BulletSpawnPoint { get; set; }

    [field: SerializeField] public GameObject BulletPrefab { get; set; }

    [field: SerializeField] public float BulletSpeed { get; set; } = 10;

    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        speedometer.Initialise();
        boxCollider = gameObject.GetComponent<BoxCollider>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void Update()
    {
        base.Update();

        // Rotate towards Movement.
        Vector3 faceDir = move;
        faceDir.y = 0;
        if (faceDir != Vector3.zero)
        {
            AlignTransformToMovement(transform, faceDir, RotationSpeed, Vector3.up);
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
        move.z = move.y;
        move.y = 0;
        move.Normalize();
    }

    protected override void Jump(CallbackContext ctx)
    {
        if (!Active)
        {
            return;
        }

        // This check is original and untouched.
        if (IsGrounded())
        {
            // Original: Jump with a modified kinematic equation.
            Rb.velocity += new Vector3(0f, Mathf.Sqrt(jumpHeight * -3f * Physics.gravity.y), 0f);
            // If we were clinging onto something, we want to jump in the opposite direction
            // as if the Monkey is jumping off the wall.
            Debug.Log("solider jump");

            animator.SetTrigger("Jump");
        }
    }

    protected override void PerformAbility(CallbackContext ctx)
    {
        if (!Active)
        {
            return;
        }

        animator.SetTrigger("Fire");
        GameObject bullet = Instantiate(BulletPrefab, BulletSpawnPoint.position, BulletSpawnPoint.rotation);
        bullet.GetComponent<Rigidbody>().velocity = BulletSpawnPoint.forward * BulletSpeed;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        speedometer.Record(this);
        Rb.MovePosition(Rb.position + (MovementSpeed * Time.fixedDeltaTime * move));
        DetermineAnimationState();

        speedometer.Mark();
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

    private void DetermineAnimationState()
    {
        if (IsZero(speedometer.Velocity))
        {
            animator.SetTrigger("Idle");
        }
        else
        {
            animator.SetTrigger("Walk");
        }
    }
}