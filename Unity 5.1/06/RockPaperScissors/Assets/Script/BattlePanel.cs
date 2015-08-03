using UnityEngine;
using System.Collections;

/** 공수선택용 패널 */
public class BattlePanel : MonoBehaviour {
    public AudioClip m_onCursorSE;  // 커서가 위에 올라왔을 때의 효과음.
    public AudioClip m_decideSE;    // 결정음.

    public ActionKind m_actionKind;
    bool m_isSelected;  // 선택됐을 때는 true.

    enum State {    // 연출.
        FadeIn,
        SelectWait,
        FadeOut,
        End,
    }
    State m_state;

    // 애니메이션.
    State m_currentAnimation;
    Animation m_animation;      
    void ChangeAnimation(State animation) {
        m_currentAnimation = animation;
        m_animation.Play(m_currentAnimation.ToString());
    }

	// Use this for initialization
	void Start () {
        m_isSelected = false;
        m_state = State.FadeIn;

        m_animation = GetComponent<Animation>();
        m_currentAnimation = State.FadeIn;
        ChangeAnimation(State.FadeIn);
	}
	
	// Update is called once per frame
	void Update () {

        switch (m_state) {
        case State.FadeIn:
            UpdateFadeIn();
            break;
        case State.SelectWait:
            UpdateSelectWait();
            break;
        case State.FadeOut:
            UpdateFadeOut();
            break;
        }
	}


    // 입장.
    void UpdateFadeIn() {
        // 애니메이션이 끝나면 다음 상태로.
        if (m_animation.isPlaying == false) {
            m_state = State.SelectWait;
        }
    }

    //선택 대기.
    void UpdateSelectWait() {
        if (IsHit()) {
            // 커서가 올라왔을 때 효과음을 울립니다.
            if (transform.localScale == Vector3.one) {
                GetComponent<AudioSource>().clip = m_onCursorSE;
                GetComponent<AudioSource>().Play();
            }

            transform.localScale = Vector3.one * 1.2f;
            if (Input.GetMouseButtonDown(0)) {
                m_isSelected = true; //상태 통지는 부모에게 위임합니다.
                //효과음.
                GetComponent<AudioSource>().clip = m_decideSE;
                GetComponent<AudioSource>().Play();
            }
        }
        else {
            transform.localScale = Vector3.one;
        }
    }

    //퇴장.
    void UpdateFadeOut() {
        if (m_currentAnimation != State.FadeOut) {
            ChangeAnimation(State.FadeOut);
        }

        // 애니메이션이 끝나면 다음 상태로.
        if (m_animation.isPlaying == false) {
            m_state = State.End;
        }
    }



    //마우스가 올라가 있으면 true를 반환.
    bool IsHit() {
        GameObject obj = GameObject.Find("GUICamera");
        Ray ray = obj.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;

        return GetComponent<Collider>().Raycast(ray, out raycastHit, 100);
    }



    // 클릭됐으면 true.
    public bool IsSelected() {
        return m_isSelected;
    }

    // 공방 선택 결정 후의 연출로 이행합니다.
    public void ChangeSelectedState() {
        m_state = State.FadeOut;

        if (m_isSelected == false) {
            Destroy(gameObject); // 선택되지 않은 패널은 지웁니다.
        }
    }
}
