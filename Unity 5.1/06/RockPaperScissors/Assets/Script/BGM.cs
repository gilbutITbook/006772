using UnityEngine;
using System.Collections;

public class BGM : MonoBehaviour {
    enum State {
        Wait,
        FadeOut,
    };
    State m_state;


    // Use this for initialization
    void Start() {
        m_state = State.Wait;
    }

    // Update is called once per frame
    void FixedUpdate() {
        switch (m_state) {
        case State.Wait:
            break;
        case State.FadeOut:
            GetComponent<AudioSource>().volume -= 0.005f;
            break;
        }
    }


    //BGM 음량을 서서히 내립니다.
    public void FadeOut() {
        m_state = State.FadeOut;
    }
}
