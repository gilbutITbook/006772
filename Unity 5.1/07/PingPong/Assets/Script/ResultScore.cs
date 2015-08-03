using UnityEngine;
using System.Collections;
using System;

public class ResultScore : MonoBehaviour {
    GameObject m_icon;
    GameObject m_peke;
    GameObject m_sushiNum;
    GameObject m_score;

    int m_scoreCounter; //카운트업용.
    int m_scoreMax;     //표시할 스코어.
    int m_getNum;       //획득수 표시용.

    enum State {
        Wait,       //대기 중.
        In,         //입장.
        CountUp,    //카운드업 중.
        End,        //끝.
    };
    State m_state;

	// Use this for initialization
	void Start () {
        m_scoreCounter = 0;
        m_scoreMax = 0;
        m_getNum = 0;

        m_state = State.Wait;

        m_icon = transform.FindChild("sushi_icon").gameObject;
        m_peke = transform.FindChild("peke").gameObject;
        m_sushiNum = transform.FindChild("sushinum").gameObject;
        m_score = transform.FindChild("score").gameObject;

        //표시 OFF.
        SpriteRenderer[] spriteRenderer = GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer sr in spriteRenderer) {
            sr.enabled = false;
        }
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        switch (m_state) {
        case State.Wait:
            break;

        case State.In:
            //표시물 ON.
            SpriteRenderer[] spriteRenderer = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer sr in spriteRenderer) {
                sr.enabled = true;
            }
            m_sushiNum.GetComponent<Number>().SetNum(m_getNum);
            m_score.GetComponent<Number>().SetNum(0);

            m_state = State.CountUp;
            break;

        case State.CountUp:
            //카운트업.
            m_scoreCounter++;
            m_scoreCounter = Math.Min(m_scoreCounter, m_scoreMax);
            m_score.GetComponent<Number>().SetNum(m_scoreCounter);

            if (m_scoreCounter >= m_scoreMax) {
                m_state = State.End;
            }
            break;

        case State.End:
            break;
        }
	}

    
    /**
     * 애니메이션 시작.
     * @param getNum    획득수.
     * @param score     득점.
     */
    public void FadeIn(int getNum, int score) {
        m_state = State.In;

        m_getNum = getNum;
        m_scoreMax = score;
        m_scoreCounter = 0;
    }

    //애니메이션이 끝나면 true.
    public bool IsEnd() {
        return (m_state == State.End);
    }
}
