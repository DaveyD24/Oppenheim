using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletScript : MonoBehaviour
{
    public float life = 3;

    void Awake()
    {
        Destroy(gameObject, life);
    }

    void OnCollisionEnter(Collision col)
    {
        Debug.Log("Hit");
        if (col.gameObject.CompareTag("Destroyable"))
        {
            Destroy(col.gameObject);
            Destroy(gameObject);
        }
    }
}
