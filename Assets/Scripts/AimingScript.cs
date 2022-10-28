using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimingScript : MonoBehaviour
{
    //public GameObject currentGun;
    //public GameObject currentTarget;
    [SerializeField] private GameObject fovStartPoint;
    [SerializeField] private float lookSpeed;
    [SerializeField] private float maxAngle = 90;
    [SerializeField] private float closestDist = Mathf.Infinity;
    [SerializeField] private GameObject closestObj = null;
    [SerializeField] private GameObject soldierGun;
    private GameObject[] objNearby;

    private Quaternion targetRotation;
    private Quaternion lookAt;

    private void Start()
    {
        objNearby = GameObject.FindGameObjectsWithTag("Breakable");
    }

    private void Update()
    {
        FindTarget();

        // face direction if there is a breakable object in view, else just look forward
        if (ObjInFieldOfView(fovStartPoint) && (closestObj != null || closestObj.activeSelf))
        {
            Vector3 direction = closestObj.transform.position - soldierGun.transform.position;
            targetRotation = Quaternion.LookRotation(direction);
            lookAt = Quaternion.RotateTowards(soldierGun.transform.rotation, targetRotation, Time.deltaTime * lookSpeed);
            soldierGun.transform.rotation = lookAt;
        }
        else
        {
            targetRotation = Quaternion.Euler(0, 0, 180);
            soldierGun.transform.localRotation = Quaternion.RotateTowards(
                soldierGun.transform.localRotation, targetRotation, Time.deltaTime * lookSpeed);
        }
    }

    // find closest object that is breakable
    private void FindTarget()
    {
        // objNearby = Physics.OverlapSphere(this.transform.position, 100f);
        foreach (GameObject obj in objNearby)
        {
            // if (obj.GetComponent<Collider>().tag == "Breakable") {
            float breakableDist = (obj.transform.position - soldierGun.transform.position).sqrMagnitude;
            // Debug.Log("calculating breakable dis");
            if (breakableDist < closestDist && obj.activeSelf)
            {
                closestDist = breakableDist;
                closestObj = obj;
                // Debug.Log("THIS ONE CLOSER");
            }

            // }
            /*else{
                Debug.Log("not breakable");
            }*/
        }

        if (closestObj)
        {
            Debug.DrawLine(soldierGun.transform.position, closestObj.transform.position);
        }
    }

    //check if the player POV can see the object
    private bool ObjInFieldOfView(GameObject thePlayer)
    {

        if (!closestObj)
            return false;

        Vector3 targetDir = closestObj.transform.position - soldierGun.transform.position;
        float angle = Vector3.Angle(targetDir, thePlayer.transform.forward);

        if (angle < maxAngle)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
