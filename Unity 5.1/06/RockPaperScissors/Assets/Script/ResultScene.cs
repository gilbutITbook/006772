using UnityEngine;
using System.Collections;
using System;

/** 결과 화면 제어 */
public class ResultScene : MonoBehaviour {
    public GameObject m_winPrefab;
    public GameObject m_losePrefab;
    GameObject m_winLose; //승리 패배 연출용 오브젝트.

    public enum Result {    //자신의 승패 결과를 표시한다.
        Win,
        Lose,
        Draw,
    }
    Result m_result;

    enum State {
        Wait,           //대기 중.
        ScoreAnimation, //득점 연출 중.
        End,            //연출 종료.
    }
    State m_state;
    float m_endTimer; //연출 종료 대기 시간 측정용.

	// Use this for initialization
	void Start () {
        m_state = State.Wait;
    }
	
	// Update is called once per frame
	void Update () {
        switch (m_state) {
        case State.Wait:
            UpdateWait();
            break;

        case State.ScoreAnimation:
            UpdateScoreAnimation();
            break;

        case State.End:
            break;
        }
	}

    //대기 중.
    void UpdateWait() {
        bool isPlaying = false;
        if (m_winLose) { //무승부일 때는 m_winLose==null.
            isPlaying = m_winLose.GetComponent<Animation>().isPlaying;
        }

        if (isPlaying == false) {
            //승패 표시가 끝나면 득점 연출을 시작하비다
            Score[] scores = GetComponentsInChildren<Score>();
            foreach (Score s in scores) {
                s.StartAnimation();
            }
            GameObject.Find("hyphen").GetComponent<AsciiCharacter>().SetChar('-');

            m_state = State.ScoreAnimation;
        }
    }

    //득점 연출 중.
    void UpdateScoreAnimation() {
        Score[] scores = GetComponentsInChildren<Score>();
        foreach (Score s in scores) {
            if (s.IsEnd() == false) {
                return;     //연출 중이면 아무것도 하지 않습니다.
            }
        }
        //연출 종료면 상태 전환.
        m_endTimer = Time.time;
        m_state = State.End;
    }



    // 점수, 승패를 전달해주세요.
    public void Setup(int[] prevScores, int[] scores, Result result) {
        Debug.Log("GameWinner:" + result.ToString() + "[" + scores[0] + " - " + scores[1] + "]");
        
        m_result = result;

        //득점 표시 설정.
        string[] names = { "Score0", "Score1" };
        for(int i=0; i < names.Length; ++i){
            Transform scoreTransform = transform.FindChild( names[i] );
            Score s = scoreTransform.GetComponent<Score>();
            s.Setup( prevScores[i], scores[i]);
        }

        //승패 연출.
        if(m_result == Result.Win){
            m_winLose = Instantiate(m_winPrefab) as GameObject;
            m_winLose.transform.parent = transform;
        }
        else if(m_result == Result.Lose){
            m_winLose = Instantiate(m_losePrefab) as GameObject;
            m_winLose.transform.parent = transform;
        }

    }

    //연출이 끝나면 true.
    public bool IsEnd() {
        if (m_state == State.End) {
            float dt = Time.time - m_endTimer;
            return (dt > 2.0f);
        }
        return false;
    }
}
