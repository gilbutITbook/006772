using UnityEngine;
using System.Collections;

/** 가위바위보!를 외치는 연출 */
public class ShootCall : MonoBehaviour {
    public GameObject m_jankenPrefab;
    public GameObject m_ponPrefab;
    
    GameObject m_janken = null;
    GameObject m_pon = null;
    GameObject[] m_players;

    enum State {    //연출.
        Janken,
        PonIn,
        PonOut,
        End,
    }
    State m_state;

	// Use this for initialization
	void Start () {
        m_state = State.Janken;

        m_players = new GameObject[2];
        m_players[0] = GameObject.Find("Daizuya");
        m_players[1] = GameObject.Find("Toufuya");
	}
	
	// Update is called once per frame
	void Update () {
        switch (m_state) {
        case State.Janken:
            UpdateJanken();
            break;
        case State.PonIn:
            UpdatePonIn();
            break;
        case State.PonOut:
            UpdatePonOut();
            break;
        case State.End:
            break;
        }
	}

    //【가위바위】 표시 중.
    void UpdateJanken() {
        if (m_janken == null) {
            //초기화.
            Vector3 pos = new Vector3(0, 0, 0);
            m_janken = Instantiate(m_jankenPrefab, pos, Quaternion.identity) as GameObject;
            GetComponent<AudioSource>().PlayDelayed(0.7f); // '바위' 효과음을 늦게 재생시킵니다.
        }
        
        Animation animation = m_janken.GetComponent<Animation>();
        if(animation.isPlaying == false){
            m_state = State.PonIn;
        }
    }

    //【보】입장.
    void UpdatePonIn() {
        if (m_pon == null) {
            //초기화.
            Destroy(m_janken); // [가위바위] 표시는 지웁니다.
            Vector3 pos = new Vector3(0.42f, 1.8f, 0);
            m_pon = Instantiate(m_ponPrefab, pos, Quaternion.identity) as GameObject;

            // 플레이어 애니메이션을 가위바위보로 한다.
            foreach (GameObject player in m_players) {
                player.GetComponent<Player>().ChangeAnimationJanken();
            }
        }

        Animation animation = m_pon.GetComponent<Animation>();
        if (animation.isPlaying == false) {
            m_state = State.PonOut;
            animation.Play("FadeOut");
        }
    }

    //【보】퇴장.
    void UpdatePonOut() {
        Animation animation = m_pon.GetComponent<Animation>();

        //플레이어 애니메이션 대기.
        bool isEndAnimation = true;
        foreach (GameObject player in m_players) {
            if (player.GetComponent<Player>().IsCurrentAnimationEnd() == false) {
                isEndAnimation = false;
            }
        }
        if (isEndAnimation == true && animation.isPlaying == false){
            //Debug.Log(isEndAnimation);
            m_state = State.End;
        }
    }
    
    //연출이 끝나면 true.
    public bool IsEnd() {
        return (m_state == State.End);
    }
}
