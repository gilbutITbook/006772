using UnityEngine;
using System.Collections;

public class ScoreSushi : MonoBehaviour {
    public SushiType m_sushiType;       //초밥 종류.

    Vector3 m_startPos; //페이드 아웃 시 초기 위치.
    Vector3 m_target;   //페이드 아웃이 갈 곳.
    
    float m_timer;
    const float FADE_TIME = 2.0f;
    
    enum State {
        Wait,
        FadeOut,
    };
    State m_state;


	// Use this for initialization
	void Start () {
        m_state = State.Wait;
        m_timer = 0;
	}
	
	// Update is called once per frame
	void FixedUpdate() {
        switch (m_state) {
        case State.Wait:
            break;
        case State.FadeOut:
            m_timer += Time.fixedDeltaTime;
            
            //target을 향해서 좌표를 움직입니다.
            float rate = m_timer / FADE_TIME;
            rate = Mathf.Min(rate, 1.0f);
            Vector3 pos = Vector3.Lerp(m_target, m_startPos, Mathf.Exp(-5.0f*rate));
            transform.position = pos;

            //스케일 변화.너무 작아지지 않게 조정.
            transform.localScale = Vector3.one * (0.3f + 0.7f * Mathf.Exp(-5.0f*rate));
            
            break;
        }

	}


    //target에 맞춰 페이드 아웃합니다.
    public void StartFadeOut(Vector3 target) {
        m_startPos = transform.position;
        m_target = target;
        
        m_timer = 0;
        m_state = State.FadeOut;
    }

    //페이드 아웃 중이면 true.
    public bool IsFadeOut() {
        return (m_state == State.FadeOut);
    }
    //페이드 아웃이 끝났으면 true.
    public bool IsFadeOutEnd() {
        if (IsFadeOut()) {
            if (m_timer > FADE_TIME) {
                return true;
            }
        }
        return false;
    }

}
