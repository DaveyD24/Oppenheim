using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletScript : MonoBehaviour
{
    [field: SerializeField] public float Life { get; set; } = 3;

    [ReadOnly] public AudioController Audio;

    private void Awake()
    {
        Audio = GetComponent<AudioController>();
        Destroy(gameObject, Life);
    }

    private void OnCollisionEnter(Collision col)
    {
        // Debug.Log("Hit");
        if (col.gameObject.CompareTag("Destroyable"))
        {
            Destroy(col.gameObject);
            Audio.Play("Bullet Break", EAudioPlayOptions.AtTransformPosition);
            Destroy(gameObject);
        }
        else if (col.gameObject.TryGetComponent(out BreakableObj breakableObj))
        {
            breakableObj.OnBreak();
        }
        else
        {
            Audio.Play("Bullet Hit", EAudioPlayOptions.AtTransformPosition);
        }
    }
}
