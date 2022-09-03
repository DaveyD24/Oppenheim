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

    void onCollisionEnter(Collision col)
    {
        Debug.Log("Hit");
        Destroy(col.gameObject);
        Destroy(gameObject);
    }
}
