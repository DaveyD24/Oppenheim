using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fan : MonoBehaviour
{
    private Transform blades;
    [SerializeField] private float fanSpeed = 400;
    [SerializeField] private float fanForce = 5;
    [SerializeField] private Vector3 fanForceSize;
    [SerializeField] private LayerMask ignoreLayers;
    [SerializeField] private GameObject collideFanParticles;
    [SerializeField] private GameObject safePathParticlesObj;
    private Vector3 boxCenter;
    private ParticleSystem safePathParticles;
    private ParticleSystem fanParticles;
    [SerializeField] private GlobalForwardAxis forwardAxis = GlobalForwardAxis.YAxis;

    private float currParticleTimeInterval = .1f;
    private float particleTimeInterval = .75f;

    private enum GlobalForwardAxis
    {
        XAxis,
        YAxis,
        ZAxis,
    }

    [SerializeField] private bool bIsOn = true;

    public void SetFanState(bool enabled)
    {
        bIsOn = enabled;
        if (enabled)
        {
            fanParticles.Play();
        }
        else
        {
            fanParticles.Stop();
            fanParticles.Clear();
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        blades = transform.GetChild(0);
        boxCenter = DetermineBoxCenter();
        fanParticles = collideFanParticles.gameObject.GetComponent<ParticleSystem>();
        safePathParticles = safePathParticlesObj.gameObject.GetComponent<ParticleSystem>();
        safePathParticlesObj.transform.forward = transform.forward;
        if (!bIsOn)
        {
            fanParticles.Stop();
        }

        currParticleTimeInterval = particleTimeInterval;
    }

    // Update is called once per frame
    private void Update()
    {
        if (bIsOn)
        {
            RotateBlades();
        }
    }

    private void FixedUpdate()
    {
        if (bIsOn)
        {
            AddItemForce();
        }
    }

    private void RotateBlades()
    {
        blades.Rotate(0, 0, fanSpeed * Time.deltaTime, Space.Self);
    }

    private void AddItemForce()
    {
        Collider[] colliders = Physics.OverlapBox(boxCenter, fanForceSize / 2, Quaternion.identity, ~ignoreLayers);

        bool bSeenCar = false;
        foreach (Collider item in colliders)
        {
            // based on the direction of the fan draw a ray towards the item detected as being within the fan, so that the ray is parellel to the items direction to the fan
            Vector3 rayStart;
            if (forwardAxis == GlobalForwardAxis.XAxis)
            {
                rayStart = new Vector3(transform.position.x, item.transform.position.y, item.transform.position.z);
            }
            else if (forwardAxis == GlobalForwardAxis.YAxis)
            {
                rayStart = new Vector3(item.transform.position.x, transform.position.y, item.transform.position.z);
            }
            else
            {
                rayStart = new Vector3(item.transform.position.x, item.transform.position.y, transform.position.z);
            }

            if (item.gameObject.GetComponent<Bat>() != null || item.gameObject.GetComponent<SoldierMovement>() != null)
            {
                rayStart += item.gameObject.GetComponent<Rigidbody>().centerOfMass;
            }

            // check if an item is blocking the fans force, if not this item gets the force
            RaycastHit hit;
            if (Physics.Raycast(rayStart, transform.forward, out hit, DetermineForceSize(), ~ignoreLayers) && hit.collider.gameObject == item.gameObject)
            {
                if (item.gameObject.TryGetComponent(out Rigidbody rb))
                {
                    if (hit.collider.gameObject.GetComponent<MonkeyController>() != null)
                    {
                        rb.AddForce(transform.forward * fanForce * 1.5f);
                    }
                    else
                    {
                        rb.AddForce(transform.forward * fanForce);
                    }
                }

                // spawn in the fan collide particles for the car
                currParticleTimeInterval -= Time.deltaTime;
                if (item.gameObject.name == "CarBody")
                {
                    safePathParticlesObj.transform.position = item.transform.position;
                    bSeenCar = true;
                    if (!safePathParticles.isPlaying)
                    {
                        safePathParticles.Play();
                    }

                    if (currParticleTimeInterval <= 0)
                    {
                        Instantiate(collideFanParticles, hit.point, Quaternion.identity, item.transform);
                        currParticleTimeInterval = particleTimeInterval;
                    }
                }

                Debug.DrawRay(rayStart, transform.forward * DetermineForceSize(), Color.red);
            }
        }

        if (!bSeenCar && safePathParticles.isPlaying)
        {
            safePathParticles.Stop();
            safePathParticles.Clear();
        }
    }

    private Vector3 DetermineBoxCenter()
    {
        return transform.position + (transform.forward * DetermineForceSize() / 2);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(DetermineBoxCenter(), fanForceSize);
    }

    private float DetermineForceSize()
    {
        float forceSize;
        if (forwardAxis == GlobalForwardAxis.XAxis)
        {
            forceSize = fanForceSize.x;
        }
        else if (forwardAxis == GlobalForwardAxis.YAxis)
        {
            forceSize = fanForceSize.y;
        }
        else
        {
            forceSize = fanForceSize.z;
        }

        return forceSize;
    }
}
