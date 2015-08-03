using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct Sound {

	// 사운드 에셋과 같은 이름으로 해두세요.
	public enum ID {

		NONE = -1,

		SYSTEM00 = 0,		// 범용 클릭음.
		TFT_BGM01,			// BGM.
		SMN_JINGLE01,		// 누군가 놀러왔을 때의 딸랑소리.
		TFT_SE01,			// 아이템 잡았을 때.
		DDG_SE_PLAYER04,	// 발소리(필요없을지도).
		TFT_SE02A,			// 발소리1.
		TFT_SE02B,			// 발소리2.
		NUM,
	};

	public enum SLOT {

		NONE = -1,

		BGM = 0,
		SE0,
		SE_WALK0,
		SE_WALK1,

		NUM,
	};
};

public class SoundManager : MonoBehaviour {

	public class Slot {

		public AudioSource		source = null;
		public float			timer  = 0.0f;		// 인터벌 재생용 타이머.
		public int				sel    = 0;			// 인터벌 재생용 사운드 인덱스.
	};

	public List<AudioClip>		clips;
	public List<Slot>			slots;

	public bool		is_play_sound = true;

	// ================================================================ //
	// MonoBehaviour에서의 상속.

	void	Awake()
	{
		// enum과 같은 순서가 되게 clip을 정렬합니다.

		var		temp_clips = new List<AudioClip>();

		for(int i = 0;i < (int)Sound.ID.NUM;i++) {

			Sound.ID	sound_id = (Sound.ID)i;

			string	clip_name;

			if(i == (int)Sound.ID.SYSTEM00) {

				clip_name = "all_system00";

			} else {

				clip_name = sound_id.ToString().ToLower();
			}

			AudioClip clip = this.clips.Find(x => x.name == clip_name);

			temp_clips.Add(clip);
		}

		this.clips = temp_clips;
		
		// 슬롯(AudioSource).

		this.slots = new List<Slot>();

		for(int i = 0;i < (int)Sound.SLOT.NUM;i++) {

			Slot	slot = new Slot();

			slot.source = this.gameObject.AddComponent<AudioSource>();
			slot.timer  = 0.0f;

			this.slots.Add(slot);
		}
		this.slots[(int)Sound.SLOT.BGM].source.loop = true;
	}

	void	Start()
	{
	}
	
	void	Update()
	{
		if(!this.is_play_sound) {

			this.stopBGM();
		}
	}

	// ================================================================ //

	public void		playBGM(Sound.ID sound_id)
	{
		if(this.is_play_sound) {

			AudioClip		clip = this.clips[(int)sound_id];
	
			if(clip != null && this.slots != null) {
	
				Slot	slot = this.slots[(int)Sound.SLOT.BGM];
	
				slot.source.clip = clip;
				slot.source.Play();
			}
		}
	}
	public void		stopBGM()
	{
		this.slots[(int)Sound.SLOT.BGM].source.Stop();
	}

	public void		playSE(Sound.ID sound_id)
	{
		if(this.is_play_sound) {

			AudioClip		clip = this.clips[(int)sound_id];
	
			if(clip != null && this.slots != null) {
	
				this.slots[(int)Sound.SLOT.SE0].source.PlayOneShot(clip);
			}
		}
	}

	// 일정 간격으로 효과음을 울립니다.
	// (매 프레임 호출해도 일정 간격으로 울립니다).
	public void		playSEInterval(Sound.ID sound_id, float interval, Sound.SLOT slot_id)
	{
		if(this.is_play_sound && this.slots != null) {

			Slot	slot = this.slots[(int)slot_id];

			if(slot.timer == 0.0f) {
	
				AudioClip		clip = this.clips[(int)sound_id];
		
				if(clip != null) {
		
					slot.source.PlayOneShot(clip);
				}
			}
	
			slot.timer += Time.deltaTime;
	
			if(slot.timer >= interval) {
	
				slot.timer = 0.0f;
			}
		}
	}

	// 일정 간격으로 SE을 차례로 울립니다.
	// (매 프레임 호출해도 일정 간격으로 울립니다).
	public void		playSEInterval(Sound.ID[] sound_ids, float interval, Sound.SLOT slot_id)
	{
		if(this.is_play_sound && this.slots != null) {

			Slot	slot = this.slots[(int)slot_id];

			if(slot.timer == 0.0f) {
	
				AudioClip		clip = this.clips[(int)sound_ids[slot.sel]];
		
				if(clip != null) {
		
					slot.source.PlayOneShot(clip);
				}

				slot.sel = (slot.sel + 1)%sound_ids.Length;
			}
	
			slot.timer += Time.deltaTime;
	
			if(slot.timer >= interval) {
	
				slot.timer = 0.0f;
			}
		}
	}

	// 인터벌 SE 타이머를 리셋합니다.
	public void		stopSEInterval(Sound.SLOT slot_id)
	{
		if (this.slots != null) {
			this.slots[(int)slot_id].timer = 0.0f;
		}
	}

	// ================================================================ //
	// 인스턴스.

	private	static SoundManager	instance = null;

	public static SoundManager	getInstance()
	{
		if(SoundManager.instance == null) {

			SoundManager.instance = GameObject.Find("SoundManager").GetComponent<SoundManager>();
		}

		return(SoundManager.instance);
	}

	public static SoundManager	get()
	{
		return(SoundManager.getInstance());
	}
}
