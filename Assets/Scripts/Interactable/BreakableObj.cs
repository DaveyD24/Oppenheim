using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// an object which when shot or dashed into either shatters into pieces or simply disapears and plays a particle effeect.
/// </summary>
[RequireComponent(typeof(AudioController))]
public class BreakableObj : UniqueID, IDataInterface
{
    [Tooltip("Should this item break into pieces when broken, or should it just disapear")]
    [SerializeField] private bool bDoShatter = true;

    [Tooltip("if it does not shatter, play these particles when destroyed")]
    [SerializeField] private GameObject disapearParticles;
    [SerializeField] private Material[] particleColour;
    [Tooltip("the power to add to the shattered objects items when it is broken")]
    [SerializeField] private float shatterPower = 1500;

    [ReadOnly] public AudioController Audio;

    private void Start()
    {
        Audio = GetComponent<AudioController>();
    }

    public void OnBreak()
    {
        if (bDoShatter)
        {
            Destroy(gameObject.GetComponent<MeshCollider>());
            Destroy(gameObject.GetComponent<MeshRenderer>());
            Destroy(gameObject.GetComponent<MeshFilter>());

            for (int i = gameObject.transform.childCount - 1; i >= 0; i--)
            {
                gameObject.transform.GetChild(i).gameObject.AddComponent<Rigidbody>().AddForce(1500 * transform.forward);
                gameObject.transform.GetChild(i).gameObject.AddComponent<MeshCollider>().convex = true;
                gameObject.transform.GetChild(i).gameObject.GetComponent<MeshRenderer>().enabled = true;
                gameObject.transform.GetChild(i).parent = null;
            }

            Audio.Play("Glass", EAudioPlayOptions.AtTransformPosition | EAudioPlayOptions.DestroyOnEnd);
        }
        else
        {
            Material material = gameObject.GetComponent<MeshRenderer>().material;

            GameObject particles = Instantiate(disapearParticles, transform.position, Quaternion.identity);
            ParticleSystem particleSystem = particles.GetComponent<ParticleSystem>();
            ParticleSystemRenderer particelRenderer = particleSystem.GetComponent<ParticleSystemRenderer>();

            // set the particles to the same colour as the box
            if (material.name.Contains("Blue"))
            {
                particelRenderer.material = particleColour[0];
            }
            else if (material.name.Contains("Green"))
            {
                particelRenderer.material = particleColour[1];
            }
            else if (material.name.Contains("Purple"))
            {
                particelRenderer.material = particleColour[2];
            }
            else if (material.name.Contains("Red"))
            {
                particelRenderer.material = particleColour[3];
            }
            else if (material.name.Contains("Yellow"))
            {
                particelRenderer.material = particleColour[4];
            }

            gameObject.SetActive(false);
        }
    }

    public void LoadData(SectionData data)
    {
        if (!bDoShatter)
        {
            if (data.GeneralPhysicsObject.Dictionary.ContainsKey(SaveID) && data.GeneralPhysicsObject.Dictionary.TryGetValue(SaveID, out GeneralPhysicsObjectData boxData))
            {
                Rigidbody rb = gameObject.GetComponent<Rigidbody>();
                rb.angularVelocity = boxData.AngularVelocity;
                transform.position = boxData.Position;
                transform.rotation = boxData.Rotation;
                rb.velocity = boxData.Velocity;
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }

    public void SaveData(SectionData data)
    {
        if (!bDoShatter && gameObject.activeSelf)
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();

            GeneralPhysicsObjectData boxData = new GeneralPhysicsObjectData();
            boxData.AngularVelocity = rb.angularVelocity;
            boxData.Position = transform.position;
            boxData.Rotation = transform.rotation;
            boxData.Velocity = rb.velocity;

            data.GeneralPhysicsObject.Dictionary.Add(SaveID, boxData);
        }
    }
}
