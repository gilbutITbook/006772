using UnityEngine;
using System.Collections;

/** 결과 화면에서 사용할 스코어 */
public class Score : MonoBehaviour {
    int m_prevScore;    //이전 점수.
    int m_newScore;     //변화 후 점수.
    Animation m_animation;

    enum State {
        Wait,               //대기 중.
        PreChangeAnimation, //작아지는 애니메이션.
        ChangeAnimation,    //숫자 변화 후, 커지는 애니메이션.
        End,                //연출 종료.
    };
    State m_state = State.Wait;


    void Awake() {
        m_animation = GetComponent<Animation>();
    }
	
	// Update is called once per frame
	void Update () {
	    switch(m_state){
        case State.Wait:
            break;
        case State.PreChangeAnimation:
            //축소시킨다.
            if (m_animation.isPlaying == false) {
                GetComponentInChildren<AsciiCharacter>().SetNumber(m_newScore);
                m_animation.Play("Change");
                m_state = State.ChangeAnimation;
            }
            break;
        case State.ChangeAnimation:
            //확대시킨다.
            if (m_animation.isPlaying == false) {
                m_state = State.End;
            }
            break;
        case State.End:
            break;
        }
	}

    
    // 사전에 호출해주세요.
    public void Setup(int prevScore, int newScore) {
        m_prevScore = prevScore;
        m_newScore = newScore;
    }

    // 애니메이션 시작.
    public void StartAnimation() {
        //상태 전환.
        if (m_prevScore == m_newScore) {
            m_state = State.End;    //이 경우 애니메이션은 없으므로 End로 합니다.
        }
        else {
            // 애니메이션 시작.
            m_animation.Play("PreChange");
            m_state = State.PreChangeAnimation;
        }

        // 스코어 표시.
        GetComponentInChildren<AsciiCharacter>().SetNumber(m_prevScore);
    }

    // 연출이 끝나면 true.
    public bool IsEnd() {
        return (m_state == State.End);
    }
}
