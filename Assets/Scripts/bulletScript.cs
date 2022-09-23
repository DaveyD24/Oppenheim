using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletScript : MonoBehaviour
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

        if (col.gameObject.TryGetComponent(out BreakableObj breakableObj))
        {
            breakableObj.OnBreak();
        }

        if (col.gameObject.CompareTag("Wall"))
        {
            col.gameObject.GetComponent<Rigidbody>().AddForce(5000 * transform.forward);
        }
    }
}
