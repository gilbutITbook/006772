using UnityEngine;
using System.Collections;

/** 게임 시퀀스 담당 */
public class GameController : MonoBehaviour {
    public GameObject[] m_stagePrefabs;   // 스테이지를 등록해 둔다.

    GameObject m_timerObj;  //타이머 표시.
    float m_gameTime;       //게임 시간 제어용.
    int m_gameCount;        //몇 게임째인지 카운트한다.
    const int GAMECOUNT_MAX = 3;
    const int TIME_LIMIT = 30;  //1 게임의 제한 시간.

    enum State {
        GameIn,     //게임 시작 준비.
        Game,       //게임 중.
        GameChanging,//종료직전 연출.
        GameOut,    //게임 종료 준비.
        GameEnd,    //게임 종료.
    };
    State m_state;


	// Use this for initialization
	void Start () {
        m_timerObj = GameObject.Find("Timer");
        m_state = State.GameIn;

        m_gameTime = 0;
        m_gameCount = 0;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        switch (m_state) {
        case State.GameIn:
            UpdateGameIn();
            break;
        case State.Game:
            UpdateGame();
            break;
        case State.GameChanging:
            UpdateGameChanging();
            break;
        case State.GameOut:
            UpdateGameOut();
            break;
        case State.GameEnd:
            //UpdateGameEnd();
            break;
        }

        //타이머 표시.
        Number number = m_timerObj.GetComponent<Number>();
        float t = Mathf.Max(TIME_LIMIT - m_gameTime, 0);
        number.SetNum((int)t);
	}



    //게임 시작 준비.
    void UpdateGameIn() {
        //스테이지 구축.
        GameObject stage = GameObject.Find("Stage");
        if (stage == null) {
            stage = Instantiate(m_stagePrefabs[m_gameCount]) as GameObject;
            stage.name = "Stage";
            return;
        }

        //페이드인을 기다립니다.
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        foreach (GameObject obj in blocks) {
            BlockScript b = obj.GetComponent<BlockScript>();
            if (b.IsFadeIn()) {
                return;
            }
        }

        //게임 시작으로 전환.
        m_state = State.Game;
        m_gameTime = 0;

        //발사할 수 있게 합니다.
        GameObject[] bars = GameObject.FindGameObjectsWithTag("Bar");
        foreach (GameObject obj in bars) {
            BarScript bar = obj.GetComponent<BarScript>();
            bar.SetShotEnable(true);       //발사기능 OFF.
        }
    }


    //게임 중.
    void UpdateGame() {
        //종료 직전의 연출로 가도 되는지 판정합니다.
        m_gameTime += Time.fixedDeltaTime;
        bool isNext = false;

        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        if (blocks.Length == 0) {   //블록이 전부 없어짐.
            isNext = true;
        }
        if (m_gameTime > TIME_LIMIT) {
            isNext = true;
        }

        if (isNext) {
            //다음 상태로 전환.
            m_state = State.GameChanging;
        }
    }
    

    //스테이지를 바꾸는 연출..
    void UpdateGameChanging() {            
        //초밥 페이드 아웃 시작.
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        foreach (GameObject obj in blocks) {
            BlockScript b = obj.GetComponent<BlockScript>();
            b.FadeOut();
        }

        //탄환 소거.
        GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
        foreach (GameObject obj in balls) {
            Destroy(obj);
        }

        //발사할 수 없게 합니다.
        GameObject[] bars = GameObject.FindGameObjectsWithTag("Bar");
        foreach (GameObject obj in bars) {
            BarScript bar = obj.GetComponent<BarScript>();
            bar.SetShotEnable(false);       //발사 기능 OFF.
        }


        //다음 상태로 전환.
        m_state = State.GameOut;
    }


    //게임 종료 준비.
    void UpdateGameOut() { 
        // 페이드 아웃 대기.
        GameObject[] blocks = GameObject.FindGameObjectsWithTag("Block");
        foreach (GameObject obj in blocks) {
            BlockScript b = obj.GetComponent<BlockScript>();
            if (b.IsFadeOut()) {
                return;
            }
        }

        //스테이지를 지웁니다.
        Destroy(GameObject.Find("Stage"));


        // 1게임 종료.
        ++m_gameCount;
        //Debug.Log("GameCount:" + m_gameCount);
        if (m_gameCount == GAMECOUNT_MAX) {
            m_state = State.GameEnd; // 정해진 게임 수에 도달했으므로 결과 화면으로 전환합니다.
        }
        else {
            m_state = State.GameIn; // 다음 게임으로 진행합니다.
        }
    }



    //게임 종료라면 true.
    public bool IsEnd() {
        return (m_state == State.GameEnd);
    }

}
