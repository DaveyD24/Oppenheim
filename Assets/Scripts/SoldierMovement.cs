using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoldierMovement : MonoBehaviour
{
    //Swimming
    public bool isSwimming;
    public float swimSpeed;
    public Transform target;

    //public float Rigidbody3D rb;
    public CharacterController soldierController;
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10;
    public float speed = 6f;
    Rigidbody rigidbody;

    void Awake()
    {
        rigidbody = GetComponent<rigidbody>();
    }

    void FixedUpdate() {
        if (isSwimming != true)
        {
            if (rigidbody.useGravity != true)
            {
                rigidbody.useGravity = true;
            }
            //input
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            //move
            if (direction.magnitude >= 0.1f)
            {
                Debug.Log("Im moving");
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
            if (rigidbody.useGravity == true)
            {
                rigidbody.useGravity = false;
            }
            if (Input.GetAxisRaw("Vertical") > 0) 
            {
                transform.position += target.forward * swimSpeed * Time.deltaTime;
            }
            if (Input.GetAxisRaw("Vertical") < 0)
            {
                transform.position -= target.forward * swimSpeed * Time.deltaTime;
            }
        }
        
    }

    

    
}
