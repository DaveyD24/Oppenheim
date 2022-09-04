using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    [field: SerializeField] public float Life { get; set; } = 3;

    private void Awake()
    {
        Destroy(gameObject, Life);
    }

    private void OnCollisionEnter(Collision col)
    {
        Debug.Log("Hit");
        if (col.gameObject.CompareTag("Destroyable"))
        {
            Destroy(col.gameObject);
            Destroy(gameObject);
        }

        if (col.gameObject.CompareTag("Breakable"))
        {
            // convert this to the event system
            Destroy(col.gameObject.GetComponent<MeshCollider>());
            Destroy(col.gameObject.GetComponent<MeshRenderer>());
            Destroy(col.gameObject.GetComponent<MeshFilter>());
            for (int i = col.gameObject.transform.childCount - 1; i >= 0; i--)
            {
                col.gameObject.transform.GetChild(i).gameObject.AddComponent<Rigidbody>().AddForce(1500 * transform.forward);
                col.gameObject.transform.GetChild(i).gameObject.AddComponent<MeshCollider>().convex = true;
                col.gameObject.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().enabled = true;
                col.gameObject.transform.GetChild(i).parent = null;
            }
        }
    }
}
