using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Endcap to demo appending to a recycler
/// </summary>
public class AppendScrollRectEndcap : RecyclerScrollRectEndcap<object>
{
    [SerializeField]
    private TMP_Text _loadingText = null;
    
    [SerializeField]
    private Image _loadingCircle = null;

    private const float TimeToLoadNextPageSeconds = 3f;
    private const string TimeFormat = "0.00";
    private const int NumEntriesToAppend = 15;

    private bool IsVisible => RectTransform.Overlaps(Recycler.viewport);

    private Coroutine _fetchWhenOnScreen;
    
    public override void OnFetchedFromRecycling()
    {
    }

    public override void OnSentToRecycling()
    {
        Reset();
    }

    private void Update()
    {
        if (_fetchWhenOnScreen == null && IsVisible)
        {
            _fetchWhenOnScreen = StartCoroutine(FetchWhenOnScreen());
        }
    }

    private IEnumerator FetchWhenOnScreen()
    {
        float timeLeft = TimeToLoadNextPageSeconds;

        while (timeLeft > 0)
        {
            if (!IsVisible)
            {
               Reset();
               yield break;
            }

            _loadingText.text = timeLeft.ToString(TimeFormat);
            _loadingCircle.fillAmount = (TimeToLoadNextPageSeconds - timeLeft) / TimeToLoadNextPageSeconds;
            
            yield return null;
            timeLeft -= Time.deltaTime;
        }

        Recycler.AppendEntries(Enumerable.Repeat<object>(null, NumEntriesToAppend));
        Reset();
    }

    private void Reset()
    {
        _loadingText.text = TimeToLoadNextPageSeconds.ToString(TimeFormat);
        _loadingCircle.fillAmount = 0f;

        if (_fetchWhenOnScreen != null)
        {
            StopCoroutine(_fetchWhenOnScreen);
            _fetchWhenOnScreen = null;
        }
    }
}
