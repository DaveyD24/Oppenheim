using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MonkeyController : MonoBehaviour
{
    enum State
    {
        CLING,
        HANG,
        WALK
    }

    private CharacterController controller;
    [SerializeField] private GameObject activePlayer;
    private Rigidbody rb;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float playerSpeed = 4.5f;
    private float jumpHeight = 2.0f;
    private float gravityValue = -9.81f;
    private float followSpeed = 0.001f;

    private bool active = true;
    bool tooClose = false;
    bool clinging = false;

    [SerializeField] Canvas canvas;
    float groundHeight = 0.580005f;

    private Vector3 clingPosition;

    private bool bDidJump = false;
    private float jumpWaitTime = 1;

    private void Start()
    {
        controller = gameObject.AddComponent<CharacterController>();
        this.controller.minMoveDistance = 0;
        rb = gameObject.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        /*float distance = Vector3.Distance(this.transform.position, activePlayer.transform.position);
        if (distance < 2.0f)
        {
            tooClose = true;
        }
        else
        {
            tooClose = false;
        }*/

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
            if (Input.GetButtonDown("Jump") && (groundedPlayer || clinging) && !bDidJump)
            {
                playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
                clinging = false;
                bDidJump = true;
                Debug.Log("yeet");
            }
            else if (clinging)
            {
                //transform.position = clingPosition;
                Vector3 desiredPosition = new Vector3(transform.position.x, 6.184793f, transform.position.z);
                Vector3 gradual = Vector3.Lerp(transform.position, desiredPosition, 0.00125f);
                transform.position = gradual;
                playerVelocity.y = gravityValue * Time.deltaTime;
            }
            else
            {
                groundedPlayer = controller.isGrounded;
                if (groundedPlayer && playerVelocity.y < 0)
                {
                    playerVelocity.y = 0f;
                }

                Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                controller.Move(move * Time.deltaTime * playerSpeed);

                if (move != Vector3.zero)
                {
                    gameObject.transform.forward = move;
                }

                // Changes the height position of the player..


                playerVelocity.y += gravityValue * Time.deltaTime;
                controller.Move(playerVelocity * Time.deltaTime);
            }
            /*        else
                    {
                        if (!tooClose)
                        {
                            Vector3 desiredPosition = activePlayer.transform.position;
                            Vector3 smoothedPosition = Vector3.Lerp(this.transform.position, desiredPosition, followSpeed);
                            Vector3 flattenedPosition = new Vector3(smoothedPosition.x, groundHeight, smoothedPosition.z);
                            this.transform.position = flattenedPosition;
                        }
                    }*/
        }
    }

    public void Activate()
    {
        active = true;
        canvas.gameObject.SetActive(true);
    }

    public void Deactivate()
    {
        active = false;
        canvas.gameObject.SetActive(false);
    }

    public bool isActive()
    {
        return active;
    }

    private void OnControllerColliderHit(ControllerColliderHit collision)
    {
        if (collision.gameObject.CompareTag("Clingable"))
        {
            Debug.Log("dsadfs");
            clinging = true;
            clingPosition = transform.position;
        }
        else
        {
            clinging = false;
        }
    }
}