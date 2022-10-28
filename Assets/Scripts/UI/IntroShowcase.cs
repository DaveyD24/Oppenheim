using System.Collections;
using System.Collections.Generic;
using EventSystem;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntroShowcase : MonoBehaviour
{
    [SerializeField] private new GameObject camera;
    [SerializeField] private Image introUIBanner;
    [SerializeField] private TextMeshProUGUI introUIBannerTxt;

    [SerializeField] private Vector3[] camPositions;
    private int currentPosAt = 0;
    [SerializeField] private CamLookAt camLookAt;

    [SerializeField] private IntroShowcaseScript data;

    private Tween camMoveTween;
    private Tween bannerColourTween;
    private Tween textColourTween;

    private enum CamLookAt
    {
        PosMovingTowards,
        PosMovingAwayFrom,
    }

    private void Start()
    {
        camMoveTween = new Tween(camPositions[currentPosAt], camPositions[currentPosAt + 1], Time.time, data.AnimDuration / (camPositions.Length - 1));
        bannerColourTween = new Tween(data.BannerStartColour, data.BannerEndColor, Time.time, data.AnimDuration);
        textColourTween = new Tween(data.TextStartColour, data.TextEndColour, Time.time, data.AnimDuration);
    }

    private void Update()
    {
        if (camMoveTween != null)
        {
            camera.transform.position = camMoveTween.UpdatePosition();

            // should the camera look at the next node in sequence, or the one is is currently moving away from
            if (camLookAt == CamLookAt.PosMovingTowards)
            {
                camera.transform.LookAt(camPositions[currentPosAt + 1]);
            }
            else
            {
                camera.transform.LookAt(camPositions[currentPosAt]);
            }

            if (camMoveTween.IsComplete() && currentPosAt + 1 < camPositions.Length - 1)
            {
                currentPosAt++;
                camMoveTween = new Tween(camPositions[currentPosAt], camPositions[currentPosAt + 1], Time.time, data.AnimDuration / (camPositions.Length - 1));
            }

            introUIBanner.color = bannerColourTween.UpdateColourCurve(data.AnimSpeedCurve);
            introUIBannerTxt.color = textColourTween.UpdateColourCurve(data.AnimSpeedCurve);

            if (bannerColourTween.IsComplete())
            {
                camMoveTween = null;
                introUIBanner.gameObject.SetActive(false);
                UIEvents.BeginAnnoucement();
                GameEvents.AddActiveInputs();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        foreach (var item in camPositions)
        {
            Gizmos.DrawSphere(item, 20);
        }
    }

    [Exec("Skips all time consuming introductions.")]
    public void Skia()
    {
        enabled = false;
        camMoveTween = null;
        introUIBanner.gameObject.SetActive(false);
        UIEvents.BeginAnnoucement(); // Skip this as well.
    }
}
