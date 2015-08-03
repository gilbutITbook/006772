using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct Sound {

	// 사운드 에셋과 같은 이름으로 해두세요..
	public enum ID {

		NONE = -1,

		SYSTEM00 = 0,		// 범용 클릭음.
		TKJ_BGM01,			// 전투중 BGM(가위바위보에서 사용하던 것을 갖다 씀).
		DDG_BGM01,			// 보스전 BGM.
		TKJ_JINGLE01,		// 게임 시작.
		DDG_JINGLE02,		// 게임 클리어.
		DDG_JINGLE03,		// 플레이어 사망.
		
		DDG_SE_ENEMY01,		// 적이 당하는 효과음.
		DDG_SE_ENEMY02,		// 적 사망 효과음.
		DDG_SE_PLAYER01,	// 플레이어　공격 시.
		DDG_SE_PLAYER02,	// 플레이어  공격 상대를 맞춤.
		DDG_SE_PLAYER03,	// 플레이어  당하는 효과음.
		DDG_SE_PLAYER04,	// 발소리 효과음(앞 게임에서 유용, 필요 없을지도).
		DDG_SE_SYS01,		// 아이템 발생 효과음.
		DDG_SE_SYS02,		// 아이템 획득 효과음.
		DDG_SE_SYS03,		// 아이템 사용 효과음.
		DDG_SE_SYS04,		// 체력 회복 효과음.
		DDG_SE_SYS05,		// 워프 효과음.
		DDG_SE_SYS06,		// 아이스 과식.

		DDG_JINGLE04,		// 아이스 당첨 효과음.
		
		NUM,
	};

	public enum SLOT {

		NONE = -1,

		BGM = 0,
		SE0,
		SE_GET_ITEM,
		SE_WALK,

		NUM,
	};
};

public class SoundManager : MonoBehaviour {

	public class Slot {

		public AudioSource		source = null;
		public float			timer  = 0.0f;
		public bool				single_shot = false;
	};

	public List<AudioClip>		clips;
	public List<Slot>			slots;

	public bool		is_play_sound = true;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
		// enum과 같은 순서가 되게 clip을 정렬한다.

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
		this.slots[(int)Sound.SLOT.SE_GET_ITEM].single_shot = true;
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
	
			if(clip != null) {
	
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

	public void		playSE(Sound.ID sound_id, Sound.SLOT slot_index = Sound.SLOT.SE0)
	{
		if(this.is_play_sound) {

			do {

				AudioClip		clip = this.clips[(int)sound_id];
	
				if(clip == null) {

					break;
				}

				Slot	slot = this.slots[(int)slot_index];

				if(slot.single_shot) {

					if(slot.source.isPlaying) {
	
						break;
					}

					slot.source.clip = clip;
					slot.source.Play();

				} else {

					slot.source.PlayOneShot(clip);
				}

			} while(false);
		}
	}

	// 일정 간격으로 효과음을 울린다.
	// (매 프레임 호출해도 일정 간격에 울린다).
	public void		playSEInterval(Sound.ID sound_id, float interval, Sound.SLOT slot_id)
	{
		if(this.is_play_sound) {

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

	// 인터벌 효과음 타이머를 리셋한다.
	public void		stopSEInterval(Sound.SLOT slot_id)
	{
		this.slots[(int)slot_id].timer = 0.0f;
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
