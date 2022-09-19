using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierMovement : MonoBehaviour
{
    //Swimming
    public bool isSwimming = false;
    public float swimSpeed;
    public Transform target;

    //public float Rigidbody3D rb;
    public CharacterController soldierController;
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10;
    public float speed = 6f;
    public Transform soldierTransform;
    Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate() {
        if (isSwimming != true)
        {
            if (rb.useGravity != true)
            {
                rb.useGravity = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                // "Pause" the physics
                rb.isKinematic = true;
                // Do positioning, etc
                soldierTransform.rotation = Quaternion.identity;
                // Re-enable the physics
                rb.isKinematic = false;
            }
            //input
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            //move
            if (direction.magnitude >= 0.1f)
            {
                //Debug.Log("Im moving");
                soldierController.Move(direction * speed * Time.deltaTime);
            }

            //shoot
            if (Input.GetKeyDown(KeyCode.F))
            {
                var bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
                bullet.GetComponent<Rigidbody>().velocity = bulletSpawnPoint.forward * bulletSpeed;
            }
        }
        else 
        {
            if (rb.useGravity == true)
            {
                rb.useGravity = false;
            }
            if (Input.GetAxisRaw("Vertical") > 0) 
            {
                transform.position += target.forward * swimSpeed * Time.deltaTime;
            }
            if (Input.GetAxisRaw("Vertical") < 0)
            {
                transform.position -= target.forward * swimSpeed * Time.deltaTime;
            }
            if (Input.GetAxisRaw("Horizontal") > 0)
            {
                transform.position += target.right * swimSpeed * Time.deltaTime;
            }
            if (Input.GetAxisRaw("Horizontal") < 0)
            {
                transform.position -= target.right * swimSpeed * Time.deltaTime;
            }
        }
        
    }

    

    
}
