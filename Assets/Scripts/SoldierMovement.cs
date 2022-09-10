using UnityEngine;
using EventSystem;
using static UnityEngine.InputSystem.InputAction;
using static global::BatMathematics;

public class SoldierMovement : PlayerController
{
    [Header("Soldier Movement")]

    //public float Rigidbody3D rb;
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10;

    CharacterController controller;
    //SwitchManager switchManager;
    private Vector3 playerVelocity;
    private Vector3 move;
    private float jumpHeight = 3f;
    private bool bJumpRequested = false;

    protected override void Start()
    {
        base.Start();

        //switchManager = FindObjectOfType<SwitchManager>();
        startPosition = transform.position;
        startRotation = transform.rotation;

        controller = GetComponent<CharacterController>();
    }

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
        playerVelocity = Vector3.zero;
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

    protected override void Update()
    {
        base.Update();

        if (active)
        {
            // Move the Soldier in XZ space.
            controller.Move(MovementSpeed * Time.deltaTime * move);

            // Rotate towards Movement.
            Vector3 faceDir = move;
            if (faceDir != Vector3.zero)
            {
                AlignTransformToMovement(transform, faceDir, RotationSpeed, Vector3.up);
            }

            if (controller.isGrounded)
            {
                playerVelocity.y = 0f;
            }

            if (bJumpRequested)
            {
                playerVelocity.y += ComputeJumpScalar(jumpHeight);
                bJumpRequested = false;
            }

            // Apply Gravity to this Soldier. (This Rigidbody is marked Kinematic; preserving original settings pre-refactor)
            playerVelocity.y += Physics.gravity.y * Time.deltaTime;
            controller.Move(Time.deltaTime * playerVelocity);
        }
    }

    protected override void Movement(CallbackContext ctx)
    {
        if (!active)
            return;

        move = ctx.ReadValue<Vector2>();

        // Convert 2D to 3D movement.
        move.z = move.y;
        move.y = 0f;
        move.Normalize();
    }

    protected override void Jump(CallbackContext ctx)
    {
        if (!active)
            return;

        if (!controller.isGrounded)
            return;
        
        if (IsZero(controller.velocity.y))
            bJumpRequested = true;
    }

    protected override void PerformAbility(CallbackContext ctx)
    {
        if (!active)
            return;

        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        bullet.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * bulletSpeed;
    }
}
