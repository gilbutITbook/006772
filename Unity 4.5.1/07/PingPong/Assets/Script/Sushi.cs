using UnityEngine;
using System.Collections;

/* 초밥 애니메이션 제어  */
public class Sushi : MonoBehaviour {
    public SushiType m_sushiType;   //초밥 종류(애니메이션 지정에서 사용합니다).

    //애니메이션 정의.
    public enum AnimationType {
        sleep,
        dance,
        jump,
    };
    AnimationType m_current;
    Animation m_animation;

    // Use this for initialization
    void Start() {
        m_animation = GetComponent<Animation>();
        m_current = AnimationType.sleep;

        if (m_animation.isPlaying == false) {
            PlayAnimation(AnimationType.sleep);
        }
    }


	// Update is called once per frame
	void FixedUpdate() {
        switch (m_current) {
        case AnimationType.sleep:
            break;
        case AnimationType.jump:
            //점프 종료 후엔 자동으로 애니메이션을 전환시킵니다.
            if (m_animation.isPlaying == false) {
                PlayAnimation(AnimationType.dance);
            }
            break;
        case AnimationType.dance:
            break;
        }
	}

    //애니메이션을 재생합니다.
    public void PlayAnimation(AnimationType anim) {
        m_current = anim;
        
        //초밥 종류에 따라서 애니메이션을 지정합니다.
        string animName = m_sushiType.ToString() + "_" + m_current.ToString();
        m_animation.Play(animName);
    }

}
