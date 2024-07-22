using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RecyclerScrollRect
{
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

        protected override void OnFetchedFromRecycling(RecyclerScrollRectContentState startActiveState)
        {
            OnActiveStateChanged(RecyclerScrollRectContentState.InactiveInPool, startActiveState);
        }

        public override void OnSentToRecycling()
        {
            Reset();
        }

        protected override void OnActiveStateChanged(RecyclerScrollRectContentState prevActiveState, RecyclerScrollRectContentState newActiveState)
        {
            if (newActiveState == RecyclerScrollRectContentState.ActiveVisible)
            {
                _fetchWhenOnScreen = StartCoroutine(FetchWhenOnScreen());
            }
            else
            {
                Reset();
            }
        }
        
        private IEnumerator FetchWhenOnScreen()
        {
            float timeLeft = TimeToLoadNextPageSeconds;

            float nextEllipseChange = timeLeft - TimeBetweenEllipseChangeSeconds;
            string textEllipses = string.Empty;

            while (timeLeft > 0)
            {
                _timeLeftText.text = timeLeft.ToString(TimeFormat);
                _loadingOutline.fillAmount = (TimeToLoadNextPageSeconds - timeLeft) / TimeToLoadNextPageSeconds;

                if (timeLeft < nextEllipseChange)
                {
                    textEllipses = textEllipses.Length == 3 ? string.Empty : textEllipses + ".";
                    _titleText.text = TitleText + textEllipses;
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
}
