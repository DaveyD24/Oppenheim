using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobScript : MonoBehaviour
{
    float speed = 5.0f;
    float height = 0.5f;
    Vector3 startPosition;
    public bool doBob = false;

    [HideInInspector] public float yPosition;

    // Start is called before the first frame update
    private void Start()
    {
        startPosition = this.transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        if (doBob)
        {
            // get the objects current position and put it in a variable so we can access it later with less code
            // calculate what the new Y position will be
            float newY = Mathf.Sin(Time.time * speed);

            // set the object's Y to the new calculated Y
            transform.position = new Vector3(transform.position.x, (newY * height) + yPosition, transform.position.z);
        }
    }
}
