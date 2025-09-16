using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ScorpionSteps
{
    public class UIAnimatedObject : MonoBehaviour
    {
        #region Inspector
        public AnimationSettings animationSettings;
        public TimingSettings timingSettings;
        public AdvanceOptions advanceOptions;
        public AudioSettings audioSettings;

        private bool isToggled = false;
        public bool isPuzzleEndScreen = false;
        private bool showEndScreenEffect = false;
        #endregion
        #region MonoBehaviour Functions
        private void Awake()
        {
            if (animationSettings.prewarm)
            {
                if (animationSettings.animationType == AnimationType.Move)
                    this.transform.localPosition = animationSettings.startValue;
                else if (animationSettings.animationType == AnimationType.Scale)
                    this.transform.localScale = animationSettings.startValue;
            }
        }
        private void OnEnable()
        {
            if (advanceOptions.hasOnEnableTrigger)
                PlayAnimation();
        }
        private void OnDisable()
        {
            if (advanceOptions.hasOnDisableTrigger)
            {
                //Debug.Log("I didn't implement a reverse animation !!!");
            }
            if (advanceOptions.killOnDisable)
                DOTween.Kill(this.transform);
        }
        #endregion
        // HomeScreenManager homeScreen;
        #region Core Functions

        public void PlayAnimation()
        {
           
            Vector3 startVal = animationSettings.startValue;
            Vector3 endVal = animationSettings.endValue;
            float duration = timingSettings.duration;
            float delay = timingSettings.delay;
            AudioClip audioClip = audioSettings.startSound;
            Ease ease = animationSettings.ease;
            bool loop = animationSettings.loopAnimation;

            if (isToggled && animationSettings.canToggle)
            {
                startVal = animationSettings.endValue;
                endVal = animationSettings.startValue;
                ease = advanceOptions.reverseEase;
                audioClip = audioSettings.endSound;

                if (isPuzzleEndScreen)
                    showEndScreenEffect = true;
            }

            // if (audioSettings.hasAudioToPlay)
            // {
            //     if (SceneManager.GetActiveScene().name == "HomeScreen")
            //     {
            //         if(homeScreen == null)
            //             homeScreen = GameObject.FindObjectOfType<HomeScreenManager>();

            //         if (homeScreen.loadingLevel)
            //             return;

            //             homeScreen.GetComponent<AudioSource>().Stop();
            //         homeScreen.GetComponent<AudioSource>().clip = audioClip;
            //         homeScreen.GetComponent<AudioSource>().Play();

                    
            //     }
                 
            //     else
            //     {
            //         UIController.Instance.StopUISound();
            //         UIController.Instance.ChangeUISound(audioClip);
            //         UIController.Instance.PlayUISound();
            //     }                
            // } 

            if (animationSettings.animationType == AnimationType.Scale)
            {
                if (!animationSettings.prewarm)
                    this.gameObject.transform.localScale = startVal;

                PlayScaleAnimation(startVal, endVal, duration, delay, ease, loop);
            }
            
            else if (animationSettings.animationType == AnimationType.Move)
            {
                if (!animationSettings.prewarm)
                    this.gameObject.transform.localPosition = startVal;

                PlayMovementAnimation(startVal, endVal, duration, delay, ease, loop);
            } 
        }
        private void PlayScaleAnimation(Vector3 startValue, Vector3 endValue, float duration, float delay, Ease ease, bool loop)
        {
            isToggled = !isToggled;

            if (loop)
            {

                //Play Button Audios..
                AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
                //------------

                this.gameObject.transform.DOScale(endValue, duration)
                .SetEase(ease).SetDelay(delay).SetLoops(-1).OnComplete(() =>
                {
                    if (advanceOptions.hasCallbackFunction)
                        if (SafetyChecks.IsNotNull(advanceOptions.callBackFunction))
                            advanceOptions.callBackFunction.Invoke();
                        else
                            Debug.LogError("Empty Callback Unity Event, Please make sure to assign event in the inspector");
                });
            }

            else
            {
                //this.GetComponent<Button>().interactable = false;

                //Play Button Audios..
                AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
                //------------

                this.gameObject.transform.DOScale(endValue, duration)
                .SetEase(ease).SetDelay(delay).OnComplete(() => {

                    //this.GetComponent<Button>().interactable = true;

                    if (showEndScreenEffect && isPuzzleEndScreen)
                    {
                        //Debug.Log("Animated Object - CheckGateMissingItem");
                        //UIController.Instance.CheckGateMissingItem();
                    }


                    if (advanceOptions.hasCallbackFunction)
                        if (SafetyChecks.IsNotNull(advanceOptions.callBackFunction))
                        {
                            advanceOptions.callBackFunction.Invoke();
                          
                        } 
                        else
                            Debug.LogError("Empty Callback Unity Event, Please make sure to assign event in the inspector");
                });
            }

        }
        private void PlayMovementAnimation(Vector3 startValue, Vector3 endValue, float duration, float delay, Ease ease, bool loop)
        {
            isToggled = !isToggled;

            if (loop)
            {
                //Play Button Audios..
                AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
                //------------

                this.gameObject.transform.DOLocalMove(endValue, duration)
               .SetEase(ease).SetDelay(delay).SetLoops(-1).OnComplete(() =>
               {
                  
                   if (advanceOptions.hasCallbackFunction)
                       if (SafetyChecks.IsNotNull(advanceOptions.callBackFunction))
                           advanceOptions.callBackFunction.Invoke();
                       else
                           Debug.LogError("Empty Callback Unity Event, Please make sure to assign event in the inspector");
               });
            }

            else
            {
                //Play Button Audios..
                AudioManager.Instance.PlayRandomAudioFromPool(AudioManager.Instance.ButtonClickPool);
                //------------

                //this.GetComponent<Button>().interactable = false;
                this.gameObject.transform.DOLocalMove(endValue, duration)
               .SetEase(ease).SetDelay(delay).OnComplete(() => 
               {
                   //this.GetComponent<Button>().interactable = true;
                   if (advanceOptions.hasCallbackFunction)
                       if (SafetyChecks.IsNotNull(advanceOptions.callBackFunction))
                           advanceOptions.callBackFunction.Invoke();
                       else
                           Debug.LogError("Empty Callback Unity Event, Please make sure to assign event in the inspector");
               });
            }
           
        }
        #endregion
    }

    public enum AnimationType
    {
        Move, Scale , Rotate
    }

    [System.Serializable]
    public class AdvanceOptions
    {
        public bool hasOnEnableTrigger = false;
        public bool hasOnDisableTrigger = false;
        public bool killOnDisable = false;
        [Space]
        public Ease reverseEase = Ease.Linear;
        [Space]
        public bool hasCallbackFunction = false;
        public UnityEvent callBackFunction;

    }

    [System.Serializable]
    public class AnimationSettings
    {
        public AnimationType animationType;
        public Vector3 startValue, endValue = Vector3.zero;
        public Ease ease = Ease.Linear;
        public bool canToggle = false;
        public bool prewarm = false;
        public bool loopAnimation = false;
    }

    [System.Serializable]
    public class TimingSettings
    {
        [Range(0,5)]
        public float duration = 0.5f;
        [Range(0, 10)]
        public float delay = 0f;
    }
    [System.Serializable]
    public class AudioSettings
    {
        public AudioClip startSound,endSound;
        public bool hasAudioToPlay = false;
    }
}

