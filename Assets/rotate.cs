using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour
{
    [SerializeField] private float speed;
    // Update is called once per frame
    private void Update()
    {
        transform.Rotate(Vector3.up * speed * Time.deltaTime);
    }
}
