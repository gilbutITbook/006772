using UnityEngine;
using System.Collections;

public class BoardYou : MonoBehaviour {
    enum State {
        Run,
        Sleep,
    };
    State m_state;
    Animation m_anim;

	// Use this for initialization
	
	void Start () {
        m_anim = GetComponent<Animation>();
        Sleep();
    
	}
	
    
	// Update is called once per frame


	void Update () {
	
	}

    // 표시를 유효하게 합니다.
    public void Run() {
        if (m_state == State.Run) {
            return; // 이미 움직이고 있으면 아무 일도 하지 않습니다.
        }
        m_state = State.Run;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        Color col = renderer.color;
        col.a = 1;
        renderer.color = col;

        m_anim.Play("appeal");
    }

    // 표시를 무효로 합니다.
    public void Sleep() {
        m_state = State.Sleep;

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        Color col = renderer.color;
        col.a = 0;
        renderer.color = col;

        m_anim.Stop("appeal");
    }

    // 연출이 끝나면 true.
    public bool IsEnd() {
        AnimationState animState = m_anim["appeal"];
        if (animState.time >= animState.length) {
            return true;
        }
        return false;
    }
}

