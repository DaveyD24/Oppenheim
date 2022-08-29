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
    private float springStiffness;
    [SerializeField] private float springDampening;
    private float maxDampening;
    [SerializeField] private float suspensionRange;
    [SerializeField] private Vector3 suspensionOffset;

    [Header("Steering")]
    [Range(0, 1)]
    [SerializeField] private float grip;

    private Vector3 wheelCenter; // also the rest position of the suspension
    private Rigidbody carRb;

    private void Awake()
    {
        wheelCenter = gameObject.transform.localPosition;
        carRb = transform.parent.GetComponent<Rigidbody>();
        wheelMass = carRb.mass / 4; // assumes even distribution of the cars weight amoung all springs

        float naturalFrequency = wheelMass > 0.0001 ? springStrength / wheelMass : 0.0f;

        springStiffness = wheelMass * naturalFrequency * naturalFrequency;
        float criticalDamping = 2 * naturalFrequency * wheelMass;
        maxDampening = (criticalDamping > 0.0001 ? springDampening / criticalDamping : 1.0f) * criticalDamping;

        float springrestLength = 1; // when at rest what distance is the spring from its rest position based on it's mass and junk
    }

    private float DetermineDampingForce(Vector3 wheelVelocity)
    {
        float springVelocity = Vector3.Dot(transform.up, wheelVelocity);
        // Debug.Log("spring Velocity: " + springVelocity);
        return springVelocity * maxDampening;
    }

    private void ApplySuspensionForce(Vector3 wheelVelocity, float dist)
    {
        float naturalFrequency = wheelMass > 0.0001 ? springStrength / wheelMass : 0.0f;

        springStiffness = wheelMass * naturalFrequency * naturalFrequency;
        float criticalDamping = 2 * naturalFrequency * wheelMass;
        maxDampening = (criticalDamping > 0.0001 ? springDampening / criticalDamping : 1.0f) * criticalDamping;

        // in unreal they find the center point of the springs and ensure that the force applied to them will equal the weight of the vehicle

        Vector3 localSpringEnd = wheelCenter - (suspensionRange / 2 * carRb.transform.up);
        // the suspension force stuff
        float offset = Vector3.Dot(wheelCenter - (wheelCenter + (carRb.transform.up * dist)), carRb.transform.up);  // (suspensionRange / 2) + wheelRadius - dist; // how compressed is the spring currently

        // Debug.Log(offset + " The offset force");

        float forceApplying = offset * springStiffness;
      //  if (offset > -suspensionRange && offset < suspensionRange)
        {
            forceApplying -= DetermineDampingForce(wheelVelocity);
        }
        Debug.Log(forceApplying + " stiffness " + springStiffness + " offset " + offset + " damping " + maxDampening);
        carRb.AddForceAtPosition(forceApplying * -Vector3.up * Time.fixedDeltaTime, suspensionOffset + transform.position + (-transform.parent.up * (dist - wheelRadius)));

        // Debug.Log(forceApplying);
        Debug.DrawRay(transform.position, forceApplying * -transform.parent.up * (suspensionRange + wheelRadius), Color.blue);

        // the steering force stuff
        // if it doesn't hit anything then the wheels just need to fall back down to their resting positions
    }

    // from https://github.com/EpicGames/UnrealEngine/blob/5.0/Engine/Source/Runtime/Engine/Private/PhysicsEngine/SimpleSuspension.cpp

    float ComputeSpringStiffness(float SprungMass, float NaturalFrequency)
    {
        return SprungMass * NaturalFrequency * NaturalFrequency;
    }

    private float ComputeSpringForce(float springStiffness, float springDamping, float springDisplacement, float springVelocity)
    {
        float stiffnessForce = springDisplacement * springStiffness;
        float dampingForce = springDisplacement > 0.0001 ? springVelocity * springDamping : 0.0f;
        return stiffnessForce + dampingForce;
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
        if (Physics.Raycast(transform.position, -transform.parent.up, out hit, suspensionRange + wheelRadius) && hit.collider.transform.root != transform.root
          )//  && hit.collider != transform.GetChild(0).gameObject)
        {
            carRb.AddForceAtPosition(amount * transform.forward, transform.position);
        }
    }

    private void FixedUpdate()
{
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.parent.up, out hit, suspensionRange + wheelRadius) && hit.collider.transform.root != transform.root)
//            && hit.collider.gameObject != transform.GetChild(0).gameObject)
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
        transform.localPosition = suspensionOffset + transform.localPosition + (-transform.parent.up * (hitDist - wheelRadius));
        transform.localPosition = new Vector3(wheelCenter.x, transform.localPosition.y, wheelCenter.z);

        Vector3 wheelCurrPos = transform.localPosition;
        wheelCurrPos.y = Mathf.Clamp(wheelCurrPos.y, wheelCenter.y - suspensionRange/2, wheelCenter.y + suspensionRange/2);
        transform.localPosition = wheelCurrPos;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, wheelRadius);
        if (carRb != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawCube(carRb.transform.position + carRb.centerOfMass, new Vector3(wheelRadius, wheelRadius, wheelRadius));
        }

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, -transform.parent.up * wheelRadius);
    }
}
