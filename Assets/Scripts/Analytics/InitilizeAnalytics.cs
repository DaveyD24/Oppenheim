using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine;

public class InitilizeAnalytics : MonoBehaviour
{
    private async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
            List<string> consentIdentifiers = await AnalyticsService.Instance.CheckForRequiredConsents();
        }
        catch (ConsentCheckException e)
        {
            Debug.LogError(e.Reason);

            // Something went wrong when checking the GeoIP, check the e.Reason and handle appropriately.
        }
    }
}
