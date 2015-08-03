using UnityEngine;
using System.Collections;
using GameObjectExtension;


public enum YELL_WORD {

	NONE = -1,
	READY = 0,		// 준비!.
	OYATU,			// 간식타임!.
	OSIMAI,			// 끝.
	TIMEUP,			// 타임업.

	CAKE_COUNT,		// 순위＋케이크 획득 수

	NUM,
}

public enum YELL_FONT {

	NONE = -1,

	KATA_RE = 0,	// "レ".
	KATA_DE,		// "デ".
	KATA_S_I,		// "ィ".
	BIKKURI,		// "！".

	HIRA_O,			// "お".
	HIRA_YA,		// "や".
	HIRA_TU,		// "つ".
	KATA_TA,		// "タ".
	KATA_I,			// "イ".
	KATA_MU,		// "ム".

	HIRA_SI,		// "し".
	HIRA_MA,		// "ま".
	HIRA_I,			// "い".

	KATA_A,			// "ア".
	KATA_S_TU,		// "ッ".
	KATA_PU,		// "プ".

	KARA,			// "～".

	HIRA_S_BAN,		// "ばん".
	KATA_S_KO,		// "コ"
	NUM,
}

[System.Serializable]
public class YellFontData {

	public YELL_FONT	font;
	public Texture		texture;
	public bool			is_small;
}


public class Navi : MonoBehaviour {

	// 프리팹.

	public GameObject	ready_yell_prefab;				// 『レディ！』(레디!).

	public GameObject	status_window_local_prefab;
	public GameObject	status_window_net_prefab;

	public GameObject	marker_prefab;					// 플레이어의 위치를 파로 가리키는 마커.
	public GameObject	kabusan_speech_prefab;			// 무기 선택 씬에서의 무 아저씨의 말풍선.

	public GameObject	selecting_icon_prefab;			// 무기 선택 씬에서 다른 플레이어가 선택 중?.
	public GameObject	cake_timer_prefab;				// 케이크 무한 제공 타이머.

	// テクスチャー.

	public Texture[]	face_icon_textures;				// 플레이어의 얼굴 아이콘.
	public Texture[]	cookie_icon_textures;			// 쿠키.
	public Texture[]	number_textures;				// 숫자　0~9.
	public Texture		lace_texture;					// 레이스.
	public Texture		toufuya_icon_texture;			// 함성용 아이콘　두부장수.
	public Texture		kabusan_icon_texture;			// 함성용 아이콘 무 아저씨.

	public Texture[]	marker_karada_textures;			// 마커　몸.
	public Texture[]	marker_ude_textures;			// 마커　팔.
	public Texture[]	marker_ude_under_textures;		// 마커　팔　아래.

	public Texture[]	uun_textures;					// 무시선택 중 아이콘   생각 중.
	public Texture[]	hai_textures;					// 무기 선택 중 아이콘   결정.

	// 폰트.
	public YellFontData[]	yell_fonts;

	protected YELL_FONT[]	yell_word_ready;
	protected YELL_FONT[]	yell_word_oyatu;
	protected YELL_FONT[]	yell_word_osimai;
	protected YELL_FONT[]	yell_word_timeup;
	protected YELL_FONT[]	yell_word_cake_count;

	//

	protected YellDisp		ready_yell     = null;
	protected KabusanSpeech	kabusan_speech = null;

	protected StatusWindowLocal		stat_win_local;
	protected StatusWindowNet[]		stat_wins_net;

	protected Marker	player_marker;					// 플레이어의 위치를 파로 가리키는 마커.

	protected int[]		player_gindex;

	protected SelectingIcon[]	selecting_icons;

	protected CakeTimer			cake_timer;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	public SelectingIcon		createSelectingIcon(int account_global_index)
	{
		SelectingIcon	selecting = this.selecting_icon_prefab.instantiate().GetComponent<SelectingIcon>();

		selecting.uun_texture  = this.uun_textures[account_global_index];
		selecting.hai_texture  = this.hai_textures[account_global_index];
		selecting.player_index = account_global_index;
		selecting.create();

		this.selecting_icons[account_global_index] = selecting;

		return(selecting);
	}

	void	Awake()
	{
		this.selecting_icons = new SelectingIcon[NetConfig.PLAYER_MAX];

		// 함성으로 표시할 단어.

		this.yell_word_ready = new YELL_FONT[4];
		this.yell_word_ready[0] = YELL_FONT.KATA_RE;
		this.yell_word_ready[1] = YELL_FONT.KATA_DE;
		this.yell_word_ready[2] = YELL_FONT.KATA_S_I;
		this.yell_word_ready[3] = YELL_FONT.BIKKURI;

		this.yell_word_oyatu = new YELL_FONT[7];
		this.yell_word_oyatu[0] = YELL_FONT.HIRA_O;
		this.yell_word_oyatu[1] = YELL_FONT.HIRA_YA;
		this.yell_word_oyatu[2] = YELL_FONT.HIRA_TU;
		this.yell_word_oyatu[3] = YELL_FONT.KATA_TA;
		this.yell_word_oyatu[4] = YELL_FONT.KATA_I;
		this.yell_word_oyatu[5] = YELL_FONT.KATA_MU;
		this.yell_word_oyatu[6] = YELL_FONT.BIKKURI;

		this.yell_word_osimai = new YELL_FONT[4];
		this.yell_word_osimai[0] = YELL_FONT.HIRA_O;
		this.yell_word_osimai[1] = YELL_FONT.HIRA_SI;
		this.yell_word_osimai[2] = YELL_FONT.HIRA_MA;
		this.yell_word_osimai[3] = YELL_FONT.HIRA_I;

		this.yell_word_timeup = new YELL_FONT[7];
		this.yell_word_timeup[0] = YELL_FONT.KATA_TA;
		this.yell_word_timeup[1] = YELL_FONT.KATA_I;
		this.yell_word_timeup[2] = YELL_FONT.KATA_MU;
		this.yell_word_timeup[3] = YELL_FONT.KATA_A;
		this.yell_word_timeup[4] = YELL_FONT.KATA_S_TU;
		this.yell_word_timeup[5] = YELL_FONT.KATA_PU;
		this.yell_word_timeup[6] = YELL_FONT.KARA;

		// 케이크 무한 제공 후 랭킹 표시.
		// 순위 숫자 등은 더미 문자를 설정해두고 나중에 교체한다.
		//
		this.yell_word_cake_count = new YELL_FONT[9];
		this.yell_word_cake_count[0] = YELL_FONT.KARA;			// "1" ～ "4" 순위.
		this.yell_word_cake_count[1] = YELL_FONT.HIRA_S_BAN;	// "번".
		this.yell_word_cake_count[2] = YELL_FONT.KATA_S_I;		// 스페이스.
		this.yell_word_cake_count[3] = YELL_FONT.KARA;			// 얼굴 아이콘.
		this.yell_word_cake_count[4] = YELL_FONT.KATA_S_I;		// 스페이스.
		this.yell_word_cake_count[5] = YELL_FONT.KARA;			// 케이크 획득 개수　100 자리.
		this.yell_word_cake_count[6] = YELL_FONT.KARA;			// 케이크 획득 개수　20 자리.
		this.yell_word_cake_count[7] = YELL_FONT.KARA;			// 케이크 획득 개수　１자리.
		this.yell_word_cake_count[8] = YELL_FONT.KATA_S_KO;		// "개".
	}

	void	Start()
	{
	}

	void 	Update()
	{
		if(Input.GetKeyDown(KeyCode.A)) {

			//YellDisp	yell = this.createCakeCount(1, 1, 32);

			//yell.setPosition(Vector3.up*64.0f);
			//this.dispatchYell(YELL_WORD.READY);
		}

		if(this.stat_win_local != null) {

			this.stat_win_local.setHP(PartyControl.get().getLocalPlayer().control.vital.getHitPoint());
		}

		//

		if(this.stat_wins_net != null) {

			for(int i = 0;i < this.stat_wins_net.Length;i++) {

				this.stat_wins_net[i].setHP(PartyControl.get().getFriend(i).control.vital.getHitPoint());
			}
		}
	}

	// ================================================================ //

	// 함성을 가져온다.
	public YellDisp		getYell()
	{
		return(this.ready_yell);
	}

	// 함성을 삭제한다.
	public void			destoryYell()
	{
		if(this.ready_yell != null) {

			this.ready_yell.destroy();
			this.ready_yell = null;
		}
	}

	// 폰트 데이터를 가져온다.
	public YellFontData		getYellFontData(YELL_FONT font)
	{
		return(System.Array.Find(this.yell_fonts, x => x.font == font));
	}

	// 스테이터스 창을 만든다.
	public void		createStatusWindows()
	{
		int		local_gindex = PartyControl.get().getLocalPlayer().control.global_index;

		int		friend_count = PartyControl.get().getFriendCount();

		int[]	friend_gindex = new int[friend_count];

		for(int i = 0;i < friend_count;i++) {

			friend_gindex[i] = PartyControl.get().getFriend(i).control.global_index;
		}

		// 스테이터스 창   로컬 플레이어용.

		GameObject	go;

		go = GameObject.Instantiate(this.status_window_local_prefab) as GameObject;

		this.stat_win_local = go.GetComponent<StatusWindowLocal>();

		this.stat_win_local.face_icon_texture    = this.face_icon_textures[local_gindex];
		this.stat_win_local.cookie_icon_textures = this.cookie_icon_textures;
		this.stat_win_local.number_textures      = this.number_textures;
		this.stat_win_local.lace_texture         = this.lace_texture;
		this.stat_win_local.create();
		this.stat_win_local.setPosition(new Vector2(640.0f/2.0f - 70.0f, 480.0f/2.0f - 70.0f));

		// 스테이터스 창　리포트 플레이어용.


		this.stat_wins_net = new StatusWindowNet[friend_count];

		Vector2		position = new Vector2(640.0f/2.0f - 60.0f, 60.0f);

		for(int i = 0;i < friend_count;i++) {

			go = GameObject.Instantiate(this.status_window_net_prefab) as GameObject;
	
			StatusWindowNet	stat_win_net = go.GetComponent<StatusWindowNet>();
	
			stat_win_net.face_icon_texture    = this.face_icon_textures[friend_gindex[i]];
			stat_win_net.cookie_icon_textures = this.cookie_icon_textures;
			stat_win_net.lace_texture         = this.lace_texture;
			stat_win_net.create();

			stat_win_net.setPosition(position);

			this.stat_wins_net[i] = stat_win_net;

			position.y -= 96.0f;
		}
	}

	// 'レディ!' 등을 표시한다.
	public void		dispatchYell(YELL_WORD word)
	{
		do {

			if(this.ready_yell != null) {

				break;
			}

			GameObject	go = this.ready_yell_prefab.instantiate();

			if(go == null) {

				break;
			}

			this.ready_yell = go.GetComponent<YellDisp>();
			this.ready_yell.icon_texture = this.toufuya_icon_texture;
			this.ready_yell.word = word;

			switch(word) {

				default:
				case YELL_WORD.READY:
				{
					this.ready_yell.yell_words = this.yell_word_ready;
				}
				break;

				case YELL_WORD.OYATU:
				{
					this.ready_yell.yell_words = this.yell_word_oyatu;
				}
				break;

				case YELL_WORD.OSIMAI:
				{
					this.ready_yell.yell_words = this.yell_word_osimai;
				}
				break;

				case YELL_WORD.TIMEUP:
				{
					this.ready_yell.yell_words = this.yell_word_timeup;
				}
				break;
			}

			this.ready_yell.create();

		} while(false);
	}

	// 케이크 무한 제공 타이머를 만든다.
	public CakeTimer	createCakeTimer()
	{
		this.cake_timer = this.cake_timer_prefab.instantiate().GetComponent<CakeTimer>();

		return(this.cake_timer);
	}

	public CakeTimer	getCakeTimer()
	{
		return(this.cake_timer);
	}

	// 케이크 무한 제공 결과 순위 표시를 만든다.
	public YellDisp		createCakeCount(int rank, int account_global_index, int count)
	{
		YellDisp	cake_count = null;

		do {

			GameObject	go = this.ready_yell_prefab.instantiate();

			if(go == null) {

				break;
			}

			cake_count = go.GetComponent<YellDisp>();;

			cake_count.yell_words = this.yell_word_cake_count;

			cake_count.icon_texture = this.kabusan_icon_texture;
			cake_count.word = YELL_WORD.CAKE_COUNT;
			cake_count.create();

			cake_count.getMoji(0).moji_texture = this.number_textures[rank];

			// 공간을 비우기 위한 더미.
			cake_count.getMoji(2).moji_mae_texture = null;
			cake_count.getMoji(2).moji_texture = null;

			cake_count.getMoji(3).moji_texture = this.face_icon_textures[account_global_index];

			// 공간을 비우기 위한 더미.
			cake_count.getMoji(4).moji_mae_texture = null;
			cake_count.getMoji(4).moji_texture = null;

			if(count >= 100) {

				cake_count.getMoji(5).moji_texture = this.number_textures[(count/100)%10];

			} else {

				cake_count.getMoji(5).moji_texture = null;
			}
			if(count >= 10) {

				cake_count.getMoji(6).moji_texture = this.number_textures[(count/10)%10];

			} else {

				cake_count.getMoji(6).moji_texture = null;
			}

			cake_count.getMoji(7).moji_texture = this.number_textures[(count/1)%10];

		} while(false);

		return(cake_count);
	}

	// 플레이어 마커를 표시한다.
	public void		dispatchPlayerMarker()
	{
		if(this.player_marker == null) {

			GameObject	go = this.marker_prefab.instantiate();

			if(go != null) {

				this.player_marker = go.GetComponent<Marker>();

				// 텍스처를 설정해서 생성한다.

				int		gidx = PartyControl.get().getLocalPlayer().getGlobalIndex();

				this.player_marker.karada_texture = this.marker_karada_textures[gidx];
				this.player_marker.ude_texture    = this.marker_ude_textures[gidx];
				this.player_marker.under_texture  = this.marker_ude_under_textures[gidx];

				this.player_marker.create();
			}
		}
	}

	// 무 아저씨 말풍선 표시한다.
	public void		dispatchKabusanSpeech()
	{
		if(this.kabusan_speech == null) {

			GameObject	go = this.kabusan_speech_prefab.instantiate();

			if(go != null) {

				this.kabusan_speech = go.GetComponent<KabusanSpeech>();
			}
		}
	}

	// 무 아저씨 말풍선을 표시하지 않는다.
	public void		finishKabusanSpeech()
	{
		if(this.kabusan_speech != null) {

			this.kabusan_speech.destroy();
			this.kabusan_speech = null;
		}
	}

	// ================================================================ //
	// 인스턴스.

	private	static Navi	instance = null;

	public static Navi	get()
	{
		if(Navi.instance == null) {

			Navi.instance = GameObject.Find("Navi").GetComponent<Navi>();
		}

		return(Navi.instance);
	}
}
