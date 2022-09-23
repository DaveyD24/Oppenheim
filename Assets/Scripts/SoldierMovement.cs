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

        Audio.Play("GunCock", EAudioPlayOptions.AtTransformPosition | EAudioPlayOptions.DestroyOnEnd);
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
    //Swimming
    public bool isSwimming = false;
    public float swimSpeed;
    public Transform target;

    //public float Rigidbody3D rb;
    public Transform soldierTransform;

    void Awake()
    {
        // rb = GetComponent<Rigidbody>();
    }

    //void FixedUpdate() {
    //    if (isSwimming != true)
    //    {
    //        BobScript bobTheBuilder = this.GetComponent<BobScript>();
    //        bobTheBuilder.doBob = false;

    //        if (rb.useGravity != true)
    //        {/*
    //            rb.useGravity = true;
    //            rb.velocity = Vector3.zero;
    //            rb.angularVelocity = Vector3.zero;
    //            // "Pause" the physics
    //            rb.isKinematic = true;
    //            // Do positioning, etc
    //            soldierTransform.rotation = Quaternion.identity;
    //            // Re-enable the physics
    //            rb.isKinematic = false;*/
    //        }
    //        //input
    //        float horizontal = Input.GetAxisRaw("Horizontal");
    //        float vertical = Input.GetAxisRaw("Vertical");
    //        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

    //        //move
    //        if (direction.magnitude >= 0.1f)
    //        {
    //            //Debug.Log("Im moving");
    //            soldierController.Move(direction * speed * Time.deltaTime);
    //        }

    //        //shoot
    //        if (Input.GetKeyDown(KeyCode.F))
    //        {
    //            var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
    //            bullet.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * bulletSpeed;
    //        }
    //    }
    //    else 
    //    {
    //        //Swimming
    //        BobScript bobTheBuilder = this.GetComponent<BobScript>();
    //        bobTheBuilder.doBob = true;
    //        if (rb.useGravity == true)
    //        {
    //            rb.useGravity = false;
    //        }

    //        if (Input.GetAxisRaw("Vertical") > 0)
    //        {
    //            transform.position += target.forward * swimSpeed * Time.deltaTime;
    //        }

    //        if (Input.GetAxisRaw("Vertical") < 0)
    //        {
    //            transform.position -= target.forward * swimSpeed * Time.deltaTime;
    //        }

    //        if (Input.GetAxisRaw("Horizontal") > 0)
    //        {
    //            transform.position += target.right * swimSpeed * Time.deltaTime;
    //        }

    //        if (Input.GetAxisRaw("Horizontal") < 0)
    //        {
    //            transform.position -= target.right * swimSpeed * Time.deltaTime;
    //        }
    //    }
    //}

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
        if (!Active || isSwimming)
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

        Audio.Play("Shot", EAudioPlayOptions.AtTransformPosition | EAudioPlayOptions.DestroyOnEnd);
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        speedometer.Record(this);

        if (isSwimming)
        {
            // Swimming
            BobScript bobTheBuilder = this.GetComponent<BobScript>();
            bobTheBuilder.doBob = true;
            if (Rb.useGravity == true)
            {
                Rb.useGravity = false;
            }

            if (move.z > 0.25f)
            {
                Rb.transform.position += transform.forward * swimSpeed * Time.deltaTime;
                Debug.Log("Swim Left");
            }

            if (move.z < -0.25f)
            {
                Rb.transform.position += transform.forward * swimSpeed * Time.deltaTime;
            }

            if (move.x > 0.25f)
            {
                Rb.transform.position += transform.forward * swimSpeed * Time.deltaTime;
            }

            if (move.x < -0.25f)
            {
                Rb.transform.position += transform.forward * swimSpeed * Time.deltaTime;
            }
        }
        else
        {
            BobScript bobTheBuilder = this.GetComponent<BobScript>();
            bobTheBuilder.doBob = false;
            if (Rb.useGravity == false)
            {
                Rb.useGravity = true;
            }
        }
        // else
        //{
            Rb.MovePosition(Rb.position + (MovementSpeed * Time.fixedDeltaTime * move));
        //}

        DetermineAnimationState();

        speedometer.Mark();
    }

    bool bHasPlayedScream = false;

    public override void OnDeath()
    {
        base.OnDeath();
        baseMesh.SetActive(false);
        boxCollider.enabled = false;
        ragdol.SetActive(true);
        Rb.isKinematic = true;
        ragdol.transform.position = transform.position;

        if (!bHasPlayedScream)
        {
            Audio.PlayUnique("Scream", EAudioPlayOptions.AtTransformPosition | EAudioPlayOptions.DestroyOnEnd);
            bHasPlayedScream = true;
        }
    }

    protected override void Respawn()
    {
        baseMesh.SetActive(true);
        boxCollider.enabled = true;
        ragdol.SetActive(false);
        Rb.isKinematic = false;
        bHasPlayedScream = false;
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