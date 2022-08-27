using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YellowController : MonoBehaviour
{
    private CharacterController controller;
    [SerializeField] private GreenController greenGuy;
    private Vector3 playerVelocity;
    private bool groundedPlayer;
    private float playerSpeed = 2.0f;
    private float jumpHeight = 1.0f;
    private float gravityValue = -9.81f;
    private float followSpeed = 0.001f;

    bool active = false;
    bool tooClose = false;

    private void Start()
    {
        controller = gameObject.AddComponent<CharacterController>();
        this.controller.minMoveDistance = 0;
    }

    void Update()
    {
        float distance = Vector3.Distance(this.transform.position, greenGuy.transform.position);
        if (distance < 2.0f)
        {
            tooClose = true;
        }
        else
        {
            tooClose = false;
        }


        if (active)
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
            if (Input.GetButtonDown("Jump") && groundedPlayer)
            {
                playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
            }

            playerVelocity.y += gravityValue * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }
        else
        {
            if (!tooClose)
            {
                Vector3 desiredPosition = greenGuy.transform.position;
                Vector3 smoothedPosition = Vector3.Lerp(this.transform.position, desiredPosition, followSpeed);
                this.transform.position = smoothedPosition;
            }
        }
    }

    public void Activate()
    {
        active = true;
    }

    public void Deactivate()
    {
        active = false;
    }

    public bool isActive()
    {
        return active;
    }
}