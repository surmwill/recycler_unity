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
public class AppendScrollRectEndcap : RecyclerScrollRectEndcap<EmptyRecyclerData, string>
{
    [SerializeField]
    private Text _titleText = null;
    
    [SerializeField]
    private Text _timeLeftText = null;
    
    [SerializeField]
    private Image _loadingOutline = null;

    private const string TitleText = "Loading Next Page of Data";
    private const string TimeFormat = "0";

    private const float TimeToLoadNextPageSeconds = 3f;
    private const float TimeBetweenEllipseChangeSeconds = 0.25f;
    private const int NumEntriesToAppend = 15;

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
        if (_fetchWhenOnScreen == null && Recycler.GetStateOfEndcap() == RecyclerScrollRectEndcapState.Visible)
        {
            _fetchWhenOnScreen = StartCoroutine(FetchWhenOnScreen());
        }
    }

    private IEnumerator FetchWhenOnScreen()
    {
        float timeLeft = TimeToLoadNextPageSeconds;
        
        float nextEllipseChange = timeLeft - TimeBetweenEllipseChangeSeconds;
        string ellipse = string.Empty;
        
        while (timeLeft > 0)
        {
            if (Recycler.GetStateOfEndcap() != RecyclerScrollRectEndcapState.Visible)
            {
               Reset();
               yield break;
            }

            _timeLeftText.text = timeLeft.ToString(TimeFormat);
            _loadingOutline.fillAmount = (TimeToLoadNextPageSeconds - timeLeft) / TimeToLoadNextPageSeconds;

            if (timeLeft < nextEllipseChange)
            {
                ellipse = ellipse.Length == 3 ? string.Empty : ellipse + ".";
                _titleText.text = TitleText + ellipse;
                nextEllipseChange -= TimeBetweenEllipseChangeSeconds;
            }
            
            yield return null;
            timeLeft -= Time.deltaTime;
        }

        Recycler.AppendEntries(EmptyRecyclerData.GenerateEmptyData(NumEntriesToAppend));
        Reset();
    }

    private void Reset()
    {
        _titleText.text = TitleText;
        _timeLeftText.text = TimeToLoadNextPageSeconds.ToString(TimeFormat);

        _loadingOutline.fillAmount = 0f;

        if (_fetchWhenOnScreen != null)
        {
            StopCoroutine(_fetchWhenOnScreen);
            _fetchWhenOnScreen = null;
        }
    }
}
