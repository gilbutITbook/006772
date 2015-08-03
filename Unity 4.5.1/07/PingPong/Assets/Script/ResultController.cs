using UnityEngine;
using System.Collections;

public class ResultController : MonoBehaviour {
    public GameObject m_winPrefab;  //[승리] 표시.
    public GameObject m_losePrefab; //[패배] 표시.
    GameObject m_winlose;

    GameObject m_playerScore;
    GameObject m_opponentScore;

    
    //표시물을 만들어 둡니다.
    GameObject m_resultback;
    GameObject m_resultPlayer;
    GameObject m_resultOpponent;
    
    GameObject[] m_playerIcons;    //초법 아이콘과 스코어.
    GameObject[] m_opponentIcons;  //초밥 아이콘과 스코어.
    int m_resultAnimationIndex; //표시물의 애니메이션 관리.

    enum State {
        In,         //입장.
        ScoreWait,  //스코어 애니메이션 대기.
        TotalScore, //합계 스코어 표시.
        WinLose,    //승부.
        End,        //끝.
    }
    State m_state;


	// Use this for initialization
	void Start () {
        m_state = State.In;
        m_resultback = GameObject.Find("resultback");
        m_resultPlayer = GameObject.Find("result_player");
        m_resultOpponent = GameObject.Find("result_opponent");

        m_playerScore = GameObject.Find("PlayerScore");
        m_opponentScore = GameObject.Find("OpponentScore");

        //표시물을 확보해 둡니다.
        m_playerIcons = new GameObject[4];
        m_opponentIcons = new GameObject[4];
        string[] names = { "tamago", "ebi", "ikura", "toro" };
        for (int i = 0; i < names.Length; ++i) {
            string name = names[i];
            m_playerIcons[i] = transform.FindChild(name + "_player").gameObject;
            m_opponentIcons[i] = transform.FindChild(name + "_opponent").gameObject;
        }

        // 서버/클라이언트 아이콘.
        GameObject serverIcon = GameObject.Find("server_icon");
        GameObject clientIcon = GameObject.Find("client_icon");
        PlayerInfo playerInfo = PlayerInfo.GetInstance();
        if (playerInfo.GetPlayerId() != 0) {
            //클라이언트 시작인 경우는 클라이언트 아이콘을 왼쪽에 표시합니다.
            Vector3 pos = serverIcon.transform.position;
            serverIcon.transform.position = clientIcon.transform.position;
            clientIcon.transform.position = pos;
        }
        serverIcon.GetComponent<SpriteRenderer>().enabled = false; //처음은 표시를 꺼둡니다.
        clientIcon.GetComponent<SpriteRenderer>().enabled = false;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        switch (m_state) {
        case State.In:
            //배경 페이드 인.
            if (m_resultback.GetComponent<Animation>().isPlaying == false) {
                //서버 클라이언트 아이콘 표시를 ON으로 합니다.
                GameObject.Find("server_icon").GetComponent<SpriteRenderer>().enabled = true;
                GameObject.Find("client_icon").GetComponent<SpriteRenderer>().enabled = true;

                //효과음-카운트업음 재생.
                audio.Play();

                m_state = State.ScoreWait;
            }
            break;

        case State.ScoreWait:
            UpdateScoreWait();  //스코어 표시.

            ResultScore prs = m_playerIcons[3].GetComponent<ResultScore>();
            ResultScore ors = m_opponentIcons[3].GetComponent<ResultScore>();
            if (prs.IsEnd() && ors.IsEnd()) {
                //표시를 마치고 합계 득점을 표시합니다.
                m_resultPlayer.GetComponent<Number>().SetNum( GetResultScore(m_playerScore) );
                m_resultOpponent.GetComponent<Number>().SetNum( GetResultScore(m_opponentScore) );
                m_resultPlayer.GetComponent<Animation>().Play("ResultScore");
                m_resultOpponent.GetComponent<Animation>().Play("ResultScore");
                //SE.
                m_resultPlayer.audio.PlayDelayed(0.75f);
                audio.Stop(); //카운트업 효과음 정지.

                m_state = State.TotalScore;
            }
            break;

        case State.TotalScore:
            //합계 득점 표시 대기.
            Animation pAnim = m_resultPlayer.GetComponent<Animation>();
            Animation oAnim = m_resultOpponent.GetComponent<Animation>();
            if (pAnim.isPlaying == false && oAnim.isPlaying == false) {
                m_state = State.WinLose;
            }
            break;

        case State.WinLose:
            if (m_winlose == null) {
                //win/lose 표시 시작.
                if (GetResultScore(m_playerScore) < GetResultScore(m_opponentScore)) {
                    m_winlose = Instantiate(m_losePrefab) as GameObject;  //패배.
                }
                else {
                    m_winlose = Instantiate(m_winPrefab) as GameObject;   //승리.
                }
                m_winlose.name = "winlose";
                return;
            }

            if (m_winlose.GetComponent<Animation>().isPlaying == false) {
                Destroy(m_winlose);
                m_state = State.End;
            }
            break;

        case State.End:
            break;
        }
    }

    
    // 스코어 표시 중.
    void UpdateScoreWait(){
        if (m_resultAnimationIndex >= m_playerIcons.Length) {
            return;
        }
        if (m_resultAnimationIndex == 0) {
            // 표시 시작.
            int pCount = m_playerScore.GetComponent<UserScore>().GetCount(SushiType.tamago);
            int oCount = m_opponentScore.GetComponent<UserScore>().GetCount(SushiType.tamago);
            m_playerIcons[0].GetComponent<ResultScore>().FadeIn(pCount, pCount * 8);
            m_opponentIcons[0].GetComponent<ResultScore>().FadeIn(oCount, oCount * 8);
            m_resultAnimationIndex = 1;
            
            return;
        }


	    //스코어를 표시합니다.
        ResultScore prs = m_playerIcons[m_resultAnimationIndex - 1].GetComponent<ResultScore>();
        ResultScore ors = m_opponentIcons[m_resultAnimationIndex - 1].GetComponent<ResultScore>();
        
        //애니메이션이 끝나면 다음 애니메이션을 재생합니다.
        if(prs.IsEnd() && ors.IsEnd()){
            if (m_resultAnimationIndex >= m_playerIcons.Length) {
                return;
            }

            SushiType[] typeList = { SushiType.tamago, SushiType.ebi, SushiType.ikura, SushiType.toro };
            int[] pointList = { 8, 10, 12, 15 };  // 초밥 타입별 득점 정의.

            SushiType type = typeList[m_resultAnimationIndex];
            int point = pointList[m_resultAnimationIndex];
            int pCount = m_playerScore.GetComponent<UserScore>().GetCount(type);
            int oCount = m_opponentScore.GetComponent<UserScore>().GetCount(type);

            //득점 표시 시작.
            m_playerIcons[m_resultAnimationIndex].GetComponent<ResultScore>().FadeIn(pCount, pCount * point);
            m_opponentIcons[m_resultAnimationIndex].GetComponent<ResultScore>().FadeIn(oCount, oCount * point);

            m_resultAnimationIndex++;
        }
	}


    //결과 표시가 끝나면 true.
    public bool IsEnd() {
        return (m_state == State.End);
    }


    //합계 득점 계산.
    int GetResultScore(GameObject userScore) {
        SushiType[] typeList = { SushiType.tamago, SushiType.ebi, SushiType.ikura, SushiType.toro };
        int[] pointList = { 8, 10, 12, 15 };  //초밥 타입별 득점 정의.

        int result = 0;
        for (int i = 0; i < 4; ++i) {
            SushiType type = typeList[i];
            int point = pointList[i];
            int count = userScore.GetComponent<UserScore>().GetCount(type);

            result += count * point;
        }

        return result;
    }
    
}
