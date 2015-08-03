using UnityEngine;
using System.Collections;

/* 초밥의 종류를 정의 */
public enum SushiType {
    ebi,
    toro,
    tamago,
    ikura,
};

/* 블록 */
public class BlockScript : MonoBehaviour {
    enum State {
        FadeIn,
        Wait,
        FadeOut,
        End,
    };
    State m_state;
    float m_timer;
    const float FADEIN_TIME = 0.3f;
    const float FADEOUT_TIME = 0.3f;

	int m_life = 2;  //[0:2] 범위.

    public SushiType m_sushiType;
    private GameObject m_sushiModel;

    void Awake() {
        m_state = State.FadeIn;
        m_timer = 0;
        transform.localScale = Vector3.zero;

        m_sushiModel = transform.FindChild("sushi").gameObject;
    }


    void FixedUpdate() {
        switch (m_state) {
        case State.FadeIn:
            m_timer += Time.fixedDeltaTime;
            {   //서서히 확대.
                float rate = Mathf.Min(m_timer / FADEIN_TIME, 1.0f);
                transform.localScale = Vector3.one * rate;
            }

            if (m_timer >= FADEIN_TIME) {
                m_state = State.Wait;
            }
            break;

        case State.Wait:
            break;

        case State.FadeOut:
            m_timer += Time.fixedDeltaTime;
            {   //서서히 축소.
                float rate = Mathf.Min(m_timer / FADEOUT_TIME, 1.0f);
                transform.localScale = Vector3.one * (1.0f - rate);
            }

            if (m_timer >= FADEOUT_TIME) {
                m_state = State.End;
            }
            break;

        case State.End:
            break;

        }
    }


    //히트 처리.
	void OnCollisionEnter2D(Collision2D col)
	{
        Sushi sushi = m_sushiModel.GetComponent<Sushi>();

		PlayerInfo info = PlayerInfo.GetInstance();
		int ballId = col.gameObject.GetComponent<BallScript>().GetPlayerId();

        m_life--;
        if (m_life <= 0) {
            // 스코어 가산.
            if (ballId == info.GetPlayerId()) {
                //자신의 스코어를 더합니다.
                GameObject score = GameObject.Find("PlayerScore");
                score.GetComponent<UserScore>().PushScore(m_sushiType);
            }
            else {
                //상대 플레이어의 스코어를 더합니다.
                GameObject score = GameObject.Find("OpponentScore");
                score.GetComponent<UserScore>().PushScore(m_sushiType);
            }
            
            // 자기자신을 제거.
            Destroy(gameObject);
        }

        //초밥 모델을 애니메이션 시킵니다.
        if (m_life == 1) {
            sushi.PlayAnimation(Sushi.AnimationType.jump);
        }
	}


    
    //페이드인 / 아웃 애니메이션 용.
    public bool IsFadeIn() {
        return (m_state == State.FadeIn);
    }
    public bool IsFadeOut() {
        return (m_state == State.FadeOut);
    }
    public void FadeOut() {
        m_state = State.FadeOut;
        m_timer = 0;
    }
    
}
