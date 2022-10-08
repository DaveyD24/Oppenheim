using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimingScript : MonoBehaviour
{
    public Transform aimPosition;
    //public GameObject currentGun;
    GameObject currentTarget;
    float closestDist = Mathf.Infinity;
    Collider closestObj = null;
    Collider[] objNearby;

    bool isAiming;

    void Start()
    {
        //currentGun.transform.position = aimPosition.position;
    }

    void Update()
    {
        FindTarget();
    }

    private void FindTarget()
    {
        /*
        float closestDist = Mathf.Infinity;
        GameObject closestObj = null;
        GameObject[] allBreakables = GameObject.FindObjectsOfType<Breakable>();

        foreach (GameObject breakableBlock in allBreakables) 
        {
            float breakableDist = (breakableBlock.tranform.position - this.transform.position).sqrMagnitude;
            if (breakableDist < closestDist)
            {
                closestDist = breakableDist;
                closestObj = currentTarget;
            }
        }
        Debug.DrawLine(this.transform.position, closestObj.transform.position);*/







        //RaycastHit hit;

        //if (Physics.Raycast(transform.position, transform.forward, out hit, distance))
        //{
        //    if (hit.transform.gameObject.tag == "Breakable")
        //    {
        //        if (!isAiming)
        //            Debug.Log("detected!!");

        //        currentTarget = hit.transform.gameObject;
        //        isAiming = true;
        //    }
        //    else
        //    {
        //        currentTarget = null;
        //        isAiming = false;
        //    }
        //}


        /*
    private void FindTarget()
        {
        objNearby = Physics.OverlapSphere(this.soldierTransform.position, 50f);
        foreach (Collider obj in objNearby) 
        {
            if (obj.tag == "Breakable") {
                float breakableDist = (obj.transform.position - this.transform.position).sqrMagnitude;
                if (breakableDist < closestDist)
                {
                    closestDist = breakableDist;
                    closestObj = obj;
                }
            }
        }
        Debug.DrawLine(this.transform.position, closestObj.transform.position);
    }
        */
    }

    private void AutoAim() 
    {
        //currentGun.transform.LookAt(currentTarget.transform);
    }
}
