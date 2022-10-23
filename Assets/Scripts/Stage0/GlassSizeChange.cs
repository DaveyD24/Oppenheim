using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassSizeChange : MonoBehaviour
{
    private Tween tween;

    // Start is called before the first frame update
    private void Start()
    {
        tween = new Tween(transform.localScale, Vector3.zero, Time.time, 5);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!tween.IsComplete())
        {
            transform.localScale = tween.UpdatePositionEaseInExp();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
