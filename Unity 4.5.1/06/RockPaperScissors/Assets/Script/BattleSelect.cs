using UnityEngine;
using System.Collections;

/** 때릴지 피할지 선택 */
public class BattleSelect : MonoBehaviour {
    ActionKind m_selected; //叩 때릴지 피할지 선택.

    enum State {
        SelectWait, //선택 대기.
        Selected,   //선택 종료.
    }
    State m_state;

	// Use this for initialization
	void Start () {
        m_selected = ActionKind.None;
        m_state = State.SelectWait;
	}
	
    
	// Update is called once per frame
	void Update () {

        switch (m_state) {
        case State.SelectWait:
            UpdateSelectWait();
            break;
        case State.Selected:
            UpdateSelected();
            break;
        }        
	}

    //선택 중.
    void UpdateSelectWait() {

        // 선택됐는지 확인.
        BattlePanel[] panels = transform.GetComponentsInChildren<BattlePanel>();
        foreach (BattlePanel p in panels) {
            if (p.IsSelected()) {   // 선택이 끝난 패널이 있다.
                m_selected = p.m_actionKind;
            }
        }

        //선택・타임아웃에서 다음 상태로 바꿉니다
        Timer timer = transform.GetComponentInChildren<Timer>();
        if (m_selected != ActionKind.None || timer.IsTimeZero()) {
            //각 패널을 선택 후의 연출로 바꿉니다.
            foreach (BattlePanel p in panels) {
                p.ChangeSelectedState();
            }

            timer.Stop();
            m_state = State.Selected;
        }
    }

    //선택 후.
    void UpdateSelected() {
        //Debug.Log("BattleSelect end.");
    }



    //선택 종료면 true.
    public bool IsEnd() {
        if (m_state == State.Selected) {
            return true;
        }
        return false;
    }

    //선택 시간을 반환합니다.
    public float GetTime() {
        Timer timer = transform.GetComponentInChildren<Timer>();
        return timer.GetNumber();
    }

    //선택된 액션을 반환합니다.
    public ActionKind GetActionKind() {
        return m_selected;
    }

    //
    public void Setup(RPSKind kind0, RPSKind kind1) {
        //선택된 가위바위보를 표시하고 싶을 때는 여기서 Instantiate합니다.
        ////캐릭터가 가위바위보 간판을 들고 있게 되어 현재는 사용하지 않습니다.

        //Debug.Log(kind0.ToString());
        //Debug.Log(kind1.ToString());
        //Debug.Log("BattleSelect Setup");
    }
}

