using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Stage Intro Data", fileName = "Stage Intro Data")]
public class IntroShowcaseScript : ScriptableObject
{
    [field: SerializeField] public Color BannerStartColour { get; private set; }

    [field: SerializeField] public Color BannerEndColor { get; private set; }

    [field: SerializeField] public Color TextStartColour { get; private set; }

    [field: SerializeField] public Color TextEndColour { get; private set; }

    [field: SerializeField] public float AnimDuration { get; private set; }

    [field: SerializeField] public AnimationCurve AnimSpeedCurve { get; private set; }
}
