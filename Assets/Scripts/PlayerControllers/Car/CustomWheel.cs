using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomWheel : MonoBehaviour
{
    [Header("General Values")]
    [SerializeField] private float wheelRadius;
    [Range(1, Mathf.Infinity)]
    [SerializeField] private float wheelMass; // ?? do it need this or not

    [Header("Suspension")]
    [SerializeField] private float springStrength;
    [SerializeField] private float springDampening;
    [SerializeField] private float suspensionRange;

    [Header("Steering")]
    [Range(0, 1)]
    [SerializeField] private float grip;

    private Vector3 wheelCenter; // also the rest position of the suspension
    private Rigidbody carRb;

    private void Awake()
    {
        wheelCenter = gameObject.transform.localPosition;
        carRb = transform.parent.GetComponent<Rigidbody>();
    }

    private float DetermineDampingForce(Vector3 wheelVelocity)
    {
        float springVelocity = Vector3.Dot(transform.up, wheelVelocity);
        Debug.Log("spring Velocity: " + springVelocity);
        return springVelocity * springDampening;
    }

    private void ApplySuspensionForce(Vector3 wheelVelocity, float dist)
    {
            // the suspension force stuff
            float offset = suspensionRange + wheelRadius - dist;

            Debug.Log(offset + " The offset force");

            float forceApplying = (offset * springStrength) - DetermineDampingForce(wheelVelocity);
            carRb.AddForceAtPosition(forceApplying * transform.up, transform.position + (-transform.parent.up * (dist - wheelRadius)));

            // Debug.Log(forceApplying);
            Debug.DrawRay(transform.position, forceApplying * transform.parent.up * (suspensionRange + wheelRadius), Color.blue);

            // the steering force stuff
        // if it doesn't hit anything then the wheels just need to fall back down to their resting positions
    }

    public void ApplySteeringForce(Vector3 direction)
    {
        Vector3 wheelVelocity = carRb.GetPointVelocity(transform.position);

        float steerVelocity = Vector3.Dot(direction, wheelVelocity);

        float antiSlipForce = -steerVelocity * grip / Time.fixedDeltaTime; // acceleration = velocity change / time;

        carRb.AddForceAtPosition(direction * wheelMass * antiSlipForce, transform.position);
    }

    public void ApplyAcceleration(float amount)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.parent.up, out hit, suspensionRange + wheelRadius) && hit.collider.transform.root != transform.root)
        {
            carRb.AddForceAtPosition(amount * transform.forward, transform.position);
        }
    }

    private void FixedUpdate()
{
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.parent.up, out hit, suspensionRange + wheelRadius) && hit.collider.transform.root != transform.root)
        {
            float dist = hit.distance;
            Vector3 wheelVelocity = carRb.GetPointVelocity(transform.position);

            ApplySuspensionForce(wheelVelocity, dist);
            ApplySteeringForce(transform.right);

            SetGraphicPosition(dist);
        }
    }

    private void SetGraphicPosition(float hitDist)
    {
        transform.GetChild(0).position = transform.position + (-transform.parent.up * (hitDist - wheelRadius));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, wheelRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.parent.up * wheelRadius);
    }
}
