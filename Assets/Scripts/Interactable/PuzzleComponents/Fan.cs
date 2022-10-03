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
    [SerializeField] private GameObject safePathParticlesObj; // the particles.
    private Vector3 boxCenter;
    [SerializeField] private ParticleSystem fanParticles;
    [SerializeField] private GlobalForwardAxis forwardAxis = GlobalForwardAxis.YAxis;

    private float currParticleTimeInterval = .1f;
    private float particleTimeInterval = .75f;

    [SerializeField] private bool bIsOn = true;

    private Rigidbody detectedRigidbody;
    private Vector3 detectedRigidbodyForce;

    private enum GlobalForwardAxis
    {
        XAxis,
        YAxis,
        ZAxis,
    }

    public static bool BSeenCar { get; set; }

    public static int NumFans { get; set; } = 0; // gets the number of fans in the scene

    public static int LateUpdateNumFans { get; set; } = 0; // gets the number of fans which have had there late update methods called.

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

        // fanParticles = collideFanParticles.gameObject.GetComponent<ParticleSystem>();

        safePathParticlesObj.transform.forward = transform.forward;
        if (!bIsOn)
        {
            fanParticles.Stop();
        }

        currParticleTimeInterval = particleTimeInterval;

        safePathParticlesObj.SetActive(false);

        NumFans++;
    }

    // Update is called once per frame
    private void Update()
    {
        if (bIsOn)
        {
            if (LateUpdateNumFans > 0)
            {
                LateUpdateNumFans = 0;
            }

            RotateBlades();
            DetermineItemForce();
        }
    }

    private void FixedUpdate()
    {
        if (bIsOn)
        {
            if (detectedRigidbody != null)
            {
                detectedRigidbody.AddForce(detectedRigidbodyForce);
                detectedRigidbody = null;
            }
        }
        else if (detectedRigidbody != null)
        {
            detectedRigidbody = null;
        }
    }

    // late update is called after all update and fixed update methods on 100% of the scripts have been called
    private void LateUpdate()
    {
        // deactivate the particles if they have not been seen at all
        if (!BSeenCar && safePathParticlesObj.activeSelf)
        {
            // Debug.Log("Not seen the car");
            safePathParticlesObj.SetActive(false);

            // safePathParticles.Stop();
            // safePathParticles.Clear();
        }

        LateUpdateNumFans++;

        // ensure this can only be run once, and only after all fans have had their late update methods called
        if (LateUpdateNumFans >= NumFans)
        {
            BSeenCar = false; // reset the car being seen so that it can be checked again on the next update
        }
    }

    private void RotateBlades()
    {
        blades.Rotate(0, 0, fanSpeed * Time.deltaTime, Space.Self);
    }

    private void DetermineItemForce()
    {
        Collider[] colliders = Physics.OverlapBox(boxCenter, fanForceSize / 2, Quaternion.identity, ~ignoreLayers, QueryTriggerInteraction.Collide);

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
            RaycastHit[] hit = Physics.RaycastAll(rayStart, transform.forward, DetermineForceSize(), ~ignoreLayers);
            System.Array.Sort(hit, (x, y) => x.distance.CompareTo(y.distance)); // sort the objects by distance by distance

            if (hit.Length > 0)
            {
                // check the parameters of the first item hit only
                if (hit[0].collider.gameObject == item.gameObject)
                {
                    if (detectedRigidbody == null && item.gameObject.TryGetComponent(out Rigidbody rb))
                    {
                        detectedRigidbody = rb;
                        if (hit[0].collider.gameObject.GetComponent<MonkeyController>() != null)
                        {
                            detectedRigidbodyForce = transform.forward * fanForce * 1.5f;

                            // rb.AddForce(transform.forward * fanForce * 1.5f);
                        }
                        else
                        {
                            detectedRigidbodyForce = transform.forward * fanForce;

                            // rb.AddForce(transform.forward * fanForce);
                        }
                    }
                }

                // spawn in the fan collide particles for the car
                currParticleTimeInterval -= Time.deltaTime;

                // for the car check if either the closest or the second closest item was hit, as once the blocking sphere spawns in it will now be seen as the closest
                if ((hit[0].collider.gameObject == item.gameObject || (hit.Length > 1 && hit[0].collider.gameObject.name == "FanBlocker" && hit[1].collider.gameObject == item.gameObject)) && item.gameObject.name == "CarBody")
                {
                    safePathParticlesObj.transform.position = item.transform.position;
                    BSeenCar = true;

                    if (!safePathParticlesObj.activeSelf)
                    {
                        safePathParticlesObj.SetActive(true);
                        safePathParticlesObj.transform.GetChild(0).gameObject.GetComponent<ParticleSystem>().Play();
                    }

                    if (currParticleTimeInterval <= 0)
                    {
                        Instantiate(collideFanParticles, hit[0].point, Quaternion.identity, item.transform);
                        currParticleTimeInterval = particleTimeInterval;
                    }
                }

                // Debug.DrawRay(rayStart, transform.forward * DetermineForceSize(), Color.red);
            }
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
