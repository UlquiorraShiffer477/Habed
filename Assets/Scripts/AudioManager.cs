using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class AudioManager : MonoBehaviour
{
	#region Instance
    public static AudioManager Instance
	{
		get
		{
			if (!_instance)
				_instance = GameObject.FindObjectOfType<AudioManager>();

			return _instance;
		}
	}
	
	private static AudioManager _instance;
	#endregion

    [Header("Clips")]
	public AudioSource AS;
	public AudioSource Background_AS;
	public AudioSource QuestionAS;

	public enum BGMTypes
	{
		GameStart,
		CharacterCreation,
		HomeMenu,
		Lobby,
		EnterLies,
		ChooseLies,
		ShowLies,
		RoundResults,
		FinalRound
	}


    public List<AudioClip> ButtonClickPool = new List<AudioClip>();


	public AudioClip RoundNames1;
	public AudioClip RoundNames2;
	public AudioClip RoundNames3;
	
	public AudioClip LieScore1;
	public AudioClip LieScore2;
	public AudioClip LieScore3;

	public AudioClip TruthScore1;
	public AudioClip TruthScore2;
	public AudioClip TruthScore3;

	[Header("Background Music Loops")]
	public AudioClip GameStartLoop;
	public AudioClip CharacterCreationLoop;
	public AudioClip HomeMenuLoop;
	public AudioClip LobbyLoop;
	public AudioClip EnterLiesLoop;
	public AudioClip ChooseLiesLoop;
	public AudioClip ShowLiesLoop;
	public AudioClip RoundResultsLoop;
	public AudioClip FinalRoundLoop;

	[Header("Funny SFX")]
	public List<AudioClip> FunnySFXList;

	[Header("Showing Answers Box SFX")]
	public List<AudioClip> OnLiesSFX;
	public List<AudioClip> OnCorrectAnswerSFX;
	public List<AudioClip> OnCorrectAnswerEmptySFX;
	public List<AudioClip> OnShowingBoxSFX;
	public List<AudioClip> OnHidingBoxSFX;
	public List<AudioClip> OnShowingNamesSFX;
	public List<AudioClip> OnAddingScoresSFX;

	[Header("Character Related SFX")]
	public AudioClip ChoosingCharacterSFX;
	public AudioClip EquipSFX;

	[Header("Narrator SFX")]
	public List<AudioClip> NarratorOnLowTimeAudioClips;
	public List<AudioClip> NarratorOnFinishARoundAudioClips;
	public List<AudioClip> NarratorOnNoPickCorrectAnswerAudioClips;


	public AudioClip ResultsShowing;


	public List<AudioClip> CharacterSpawnSoundEffects;

	public List<AudioClip> ItemsEquiptSoundEffects;

	[Header("Resultes Screen Audios")]
	public AudioClip ScoreCounter;

	public AudioClip CheeringSFX;

	

	public AudioClip Drums;
	public AudioClip Fail;


	void Start() 
	{
		StartCoroutine(PlayBackgroundMusic(BGMTypes.GameStart));

		if (!PlayerPrefManager.PlayerFirstTimeData.GetTutorialFirstTime())
        {
			PlayerPrefManager.PlayerInternalSettings.SetMusicOn(0);
			PlayerPrefManager.PlayerInternalSettings.SetVFXOn(0);
        }


		Debug.Log("PlayerPrefManager.PlayerInternalSettings.GetMusicOn(): " + PlayerPrefManager.PlayerInternalSettings.GetMusicOn());
		Debug.Log("PlayerPrefManager.PlayerInternalSettings.GetVFXOn(): " + PlayerPrefManager.PlayerInternalSettings.GetVFXOn());

		if (PlayerPrefManager.PlayerInternalSettings.GetMusicOn())
		{
			Background_AS.volume = 0.2f;
		}
		else
		{
			Background_AS.volume = 0.0f;
		}

		if (PlayerPrefManager.PlayerInternalSettings.GetVFXOn())
		{
			AS.volume = 0.6f;
		}
		else
		{
			AS.volume = 0.0f;
		}
	}

	public AudioClip PlayRandomAudioFromPool(List<AudioClip> _ac)
	{
		int random = Random.Range(0, _ac.Count);

		AudioClip audioClip = _ac[random];

		AS.PlayOneShot(audioClip);

		return audioClip;
	}

	public AudioClip GetRandomAudioClipFromPool(List<AudioClip> _ac)
	{
		int random = Random.Range(0, _ac.Count);

		AudioClip audioClip = _ac[random];

		return audioClip;
	}

	public AudioClip GetRandomAudioClip(AudioClip _ac)
	{
		AudioClip audioClip = _ac;

		return audioClip;
	}

	public void PlayAudioClip(AudioClip _ac)
	{
		AS.PlayOneShot(_ac);
	}

	public void PlayQuestionAudioClip(AudioClip _ac)
	{
		QuestionAS.clip = _ac;
		QuestionAS.Play();
	}

	public void StopBackGroundMusice()
	{
		Background_AS.Stop();
	}

	public IEnumerator PlayBackgroundMusic(BGMTypes _audioName = BGMTypes.GameStart, float _volumeFadeDuration = 0.6f, float _volumeUp = 0.2f, float _volumeDown = 0f, float _optionalDelayOnStart = 0f)
	{
		Debug.LogWarning(_audioName.ToString());

		yield return new WaitForSeconds(_optionalDelayOnStart);

		if (!Background_AS.loop)
			Background_AS.loop = true;

		if (Background_AS.isPlaying)
		{
			Background_AS.DOFade(_volumeDown, _volumeFadeDuration).OnComplete(() =>
			{
				Background_AS.Stop();
			});

			yield return new WaitForSeconds(_volumeFadeDuration);
		}

		switch(_audioName)
		{
			case BGMTypes.GameStart:
			Background_AS.clip = GameStartLoop;
			break;

			case BGMTypes.CharacterCreation:
			Background_AS.clip = CharacterCreationLoop;
			break;

			case BGMTypes.HomeMenu:
			Background_AS.clip = HomeMenuLoop;
			break;

			case BGMTypes.Lobby:
			Background_AS.clip = LobbyLoop;
			break;

			case BGMTypes.EnterLies:
			Background_AS.clip = EnterLiesLoop;
			break;

			case BGMTypes.ChooseLies:
			Background_AS.clip = ChooseLiesLoop;
			break;

			case BGMTypes.ShowLies:
			Background_AS.clip = ShowLiesLoop;
			break;

			case BGMTypes.RoundResults:
			Background_AS.clip = RoundResultsLoop;
			break;

			case BGMTypes.FinalRound:
			Background_AS.clip = FinalRoundLoop;
			break;
		}

		if (!PlayerPrefManager.PlayerInternalSettings.GetMusicOn())
		{
			Background_AS.volume = 0;
		}
		else
			Background_AS.volume = _volumeUp;
			
			
		Background_AS.Play();
		// Background_AS.DOFade(_volumeUp, 0.1f);
	}
}
