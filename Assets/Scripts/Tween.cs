using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tween
{
    public Tween(Vector3 startPos, Vector3 endPos, float startTime, float duration)
    {
        // this.Target = Target;
        this.StartPos = startPos;
        this.EndPos = endPos;
        this.StartTime = startTime;
        this.Duration = duration;
    }

    public Tween(Quaternion startRot, Quaternion endRot, float startTime, float duration)
    {
        // this.Target = Target;
        this.StartRot = startRot;
        this.EndRot = endRot;
        this.StartTime = startTime;
        this.Duration = duration;
    }

    public Tween(float startPos, float endPos, float startTime, float duration)
    {
        // this.Target = Target;
        this.StartPosCircular = startPos;
        this.EndPosCircular = endPos;
        this.StartTime = startTime;
        this.Duration = duration;
    }

    // public Transform Target { get; private set; }
    public Vector3 StartPos { get; private set; }

    public float StartPosCircular { get; private set; }

    public float EndPosCircular { get; private set; }

    public Vector3 EndPos { get; private set; }

    public Quaternion StartRot { get; private set; }

    public Quaternion EndRot { get; private set; }

    public float StartTime { get; private set; }

    public float Duration { get; private set; }

    public bool IsComplete()
    {
        return (Time.time - StartTime) / Duration >= 1;
    }

    public float UpdateValue()
    {
        float timeFraction = (Time.time - StartTime) / Duration;
        return Mathf.Lerp(StartPosCircular, EndPosCircular, timeFraction);
    }
    public Vector3 UpdatePosition()
    {
        float timeFraction = (Time.time - StartTime) / Duration;
        return Vector3.Lerp(StartPos, EndPos, timeFraction);
    }

    public Vector3 UpdatePositionCurve(AnimationCurve curve)
    {
        float timeFraction = (Time.time - StartTime) / Duration;
        return Vector3.Lerp(StartPos, EndPos, curve.Evaluate(timeFraction));
    }

    public Vector3 UpdatePositionCircular(float radius, Vector2 centre)
    {
        float timeFraction = (Time.time - StartTime) / Duration;

        float pos = Mathf.Lerp(StartPosCircular, EndPosCircular, timeFraction);

        // the two angles of the unit circle to move between, being offset from the circles centre by an amount specified by the radius
        // pos is the current angle(in radians not degrees) i.e between PI/2 and PI would move the object along the circle from 90 to 180 degrees in Duration seconds
        return centre + new Vector2(Mathf.Cos(pos) * radius, Mathf.Sin(pos) * radius);
    }

    public Vector3 UpdatePositionCircularCurve(float radius, Vector2 centre, AnimationCurve curve)
    {
        float timeFraction = (Time.time - StartTime) / Duration;

        float pos = Mathf.Lerp(StartPosCircular, EndPosCircular, curve.Evaluate(timeFraction));

        // the two angles of the unit circle to move between, being offset from the circles centre by an amount specified by the radius
        // pos is the current angle(in radians not degrees) i.e between PI/2 and PI would move the object along the circle from 90 to 180 degrees in Duration seconds
        return centre + new Vector2(Mathf.Cos(pos) * radius, Mathf.Sin(pos) * radius);
    }

    public Quaternion UpdateRotation()
    {
        float timeFraction = (Time.time - StartTime) / Duration;
        return Quaternion.Lerp(StartRot, EndRot, timeFraction);
    }

    public Quaternion UpdateRotationCurve(AnimationCurve curve)
    {
        float timeFraction = (Time.time - StartTime) / Duration;
        return Quaternion.Lerp(StartRot, EndRot, curve.Evaluate(timeFraction));
    }

    public Vector3 UpdatePositionEaseInBounce()
    {
        float timeFraction = 1 - ((Time.time - StartTime) / Duration);
        const float n1 = 7.5625f;
        const float d1 = 2.75f;

        if (timeFraction < 1 / d1)
        {
            return Vector3.Lerp(StartPos, EndPos, 1 - (n1 * timeFraction * timeFraction));
        }
        else if (timeFraction < 2 / d1)
        {
            return Vector3.Lerp(StartPos, EndPos, 1 - ((n1 * (timeFraction -= 1.5f / d1) * timeFraction) + 0.75f));
        }
        else if (timeFraction < 2.5f / d1)
        {
            return Vector3.Lerp(StartPos, EndPos, 1 - ((n1 * (timeFraction -= 2.25f / d1) * timeFraction) + 0.9375f));
        }
        else
        {
            return Vector3.Lerp(StartPos, EndPos, 1 - ((n1 * (timeFraction -= 2.625f / d1) * timeFraction) + 0.984375f));
        }
    }

    public Vector3 UpdatePositionEaseOutCirc()
    {
        float timeFraction = (Time.time - StartTime) / Duration;
        timeFraction = Mathf.Clamp(timeFraction, 0.0f, 1.0f);
        timeFraction = Mathf.Sqrt(1 - (timeFraction * timeFraction));
        return Vector3.Lerp(StartPos, EndPos, 1 - timeFraction);
    }

    public Vector3 UpdatePositionEaseInCubic()
    {
        float timeFraction = (Time.time - StartTime) / Duration;
        timeFraction = Mathf.Clamp(timeFraction, 0.0f, 1.0f);
        timeFraction = 3 * Mathf.Pow(timeFraction, 3);
        return Vector3.Lerp(StartPos, EndPos, timeFraction);
    }

    public Vector3 UpdatePositionEaseOutExp()
    {
        float timeFraction = (Time.time - StartTime) / Duration;
        timeFraction = Mathf.Clamp(timeFraction, 0.0f, 1.0f);
        timeFraction = -Mathf.Pow(2, -10 * timeFraction) + 1;
        return Vector3.Lerp(StartPos, EndPos, timeFraction);
    }

    public Vector3 UpdatePositionEaseInExp()
    {
        float timeFraction = (Time.time - StartTime) / Duration;
        timeFraction = Mathf.Clamp(timeFraction, 0.0f, 1.0f);
        timeFraction = Mathf.Pow(2, 10 * (timeFraction - 1));
        Debug.Log(timeFraction);
        return Vector3.Lerp(StartPos, EndPos, timeFraction);
    }
}
