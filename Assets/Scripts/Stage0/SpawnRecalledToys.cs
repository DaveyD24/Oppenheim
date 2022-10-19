using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// run script to generate a number of items.
/// </summary>
public class SpawnRecalledToys : MonoBehaviour
{
    [SerializeField] private Items[] recalledToyList;
    [SerializeField] private int numberToys = 1000;

    private void Start()
    {
        for (int i = 0; i < numberToys; i++)
        {
            Items randItem = recalledToyList[Random.Range(0, recalledToyList.Length)];
            GameObject obj = Instantiate(randItem.Object, transform.position, Quaternion.identity);

            obj.transform.parent = this.gameObject.transform;
            obj.transform.localScale = randItem.Scale;
            obj.transform.GetChild(0).gameObject.AddComponent<MeshCollider>().convex = true;
            obj.AddComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
            obj.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * Random.Range(1, 2));
        }
    }
}

#pragma warning disable SA1402 // File may only contain a single type
[System.Serializable]
public class Items
#pragma warning restore SA1402 // File may only contain a single type
{
    [field: SerializeField] public GameObject Object { get; set; }

    [field: SerializeField] public Vector3 Scale { get; set; }
}
