using UnityEngine;
using System.Collections;

/** 가위바위보 손을 표시합니다 */
public class RPSPanel : MonoBehaviour {
    public AudioClip m_onCursorSE;  //커서가 올라왔을 때의 효과음.
    public AudioClip m_decideSE;    //결정음.

    public RPSKind m_rpsKind;
    bool m_isSelected;  //선택됐을 때는 true.

    enum State {
        FadeIn,     //입장.
        SelectWait, //선택 대기.
        OnSelected, //선택됨.
        UnSelected, //선택되지 않음.
        FadeOut,    //퇴장.
        End,
    }
    State m_state;

    //애니메이션.
    State m_currentAnimation;
    Animation m_animation;
    void ChangeAnimation(State animation) {
        m_currentAnimation = animation;

        //페이드 아웃 시에만 다른 애니메이션이 됩니다.
        if (m_currentAnimation == State.FadeOut) {
            //FadeOut_Rock, FadeOut_Paper, FadeOut_Scissor,
            string name = m_currentAnimation.ToString() + "_" + m_rpsKind.ToString();
            m_animation.Play(name);
        }
        else {
            m_animation.Play(m_currentAnimation.ToString());
        }
    }

	// Use this for initialization
	void Start () {
        m_state = State.FadeIn;
        m_isSelected = false;

        transform.localScale = Vector3.zero;

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
        case State.OnSelected:
            UpdateOnSelected();
            break;
        case State.UnSelected:
            UpdateUnSelected();
            break;
        case State.FadeOut:
            UpdateFadeOut();
            break;
        case State.End:
            break;
        }
	}

    //입장.
    void UpdateFadeIn() {
        //애니메이션이 끝나면 다음 상태로.
        if (m_animation.isPlaying == false) {
            m_state = State.SelectWait;
        }
    }

    //선택 대기.
    void UpdateSelectWait() {
        if (IsHit()) {
            //커서가 올라갔을 때 효과음을 울립니다.
            if (transform.localScale == Vector3.one) {
                GetComponent<AudioSource>().clip = m_onCursorSE;
                GetComponent<AudioSource>().Play();
            }

            //선택 범위에 들어있으면 확대 표시.
            transform.localScale = Vector3.one * 1.2f;
            if (Input.GetMouseButtonDown(0)) {
                m_isSelected = true;    //클릭됨.
                //SE.
                GetComponent<AudioSource>().clip = m_decideSE;
                GetComponent<AudioSource>().Play();

                /*
                 * 부모 쪽에서 상태를 감시하므로 여기서는 아직 state는 바꾸지 않습니다.
                 */
            }
        }
        else {
            transform.localScale = Vector3.one;
        }
    }

    //선택된 상태.
    void UpdateOnSelected() {
        if (m_currentAnimation != State.OnSelected) {
            ChangeAnimation(State.OnSelected);
        }

        //애니메이션이 끝나면 다음 상태로.
        if (m_animation.isPlaying == false) {
            m_state = State.FadeOut;
        }
    }

    //선택되지 않았다.
    void UpdateUnSelected() {
        if (m_currentAnimation != State.UnSelected) {
            ChangeAnimation(State.UnSelected);
        }

        //애니메이션이 끝나면 다음 상태로.
        if (m_animation.isPlaying == false) {
            m_state = State.End;
        }
    }


    //퇴장.
    void UpdateFadeOut() {
        if (m_currentAnimation != State.FadeOut) {
            ChangeAnimation(State.FadeOut);
        }

        //애니메이션이 끝나면 다음 상태로.
        if (m_animation.isPlaying == false) {
            m_state = State.End;
        }
    }


    //마우스가 올라와 있으면 true를 반환한다.
    bool IsHit() {
        GameObject obj = GameObject.Find("GUICamera");
        Ray ray = obj.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        RaycastHit raycastHit;

        return GetComponent<Collider>().Raycast(ray, out raycastHit, 100);
    }



    //클릭됐으면 true.
    public bool IsSelected(){
        return m_isSelected;
    }

    //종료됐으면true.
    public bool IsEnd() {
        return (m_state == State.End);
    }

    //가위바위보 결정 후 연출로 전환한다.
    public void ChangeSelectedState() {
        m_state = State.UnSelected;
        if (m_isSelected) {
            m_state = State.OnSelected;
        }
    }

}
