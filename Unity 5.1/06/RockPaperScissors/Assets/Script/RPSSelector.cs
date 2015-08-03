using UnityEngine;
using System.Collections;

/** 가위바위보 선택 패널 관리 */
public class RPSSelector : MonoBehaviour {
    RPSKind m_selected; //가위바위보 선택.

    // Use this for initialization
    void Start() {
        m_selected = RPSKind.None;

        string[] names = { "Daizuya", "Toufuya" };
        foreach (string n in names) {
            GameObject player = GameObject.Find(n);
            player.GetComponent<Player>().ChangeAnimation(Player.Motion.RPSInputWait);
        }
    }

    // Update is called once per frame
    void Update(){
        if(m_selected != RPSKind.None){
            return;     //선택됐으므로 아무 것도 하지 않습니다.
        }

        RPSPanel[] panels = transform.GetComponentsInChildren<RPSPanel>();
        foreach (RPSPanel p in panels) {
            if (p.IsSelected()) {   // 선택이 끝난 패널이 있다.
                m_selected = p.m_rpsKind;
            }
        }

        if (m_selected != RPSKind.None) {
            //각 패널을 선택 후의 연출로 변경.
            foreach (RPSPanel p in panels) {
                p.ChangeSelectedState();
            }
        }
    }


    //아직 선택되지 않은 때는 RPSKind.None을 반환합니다.
    public RPSKind GetRPSKind() {
        RPSPanel[] panels = transform.GetComponentsInChildren<RPSPanel>();
        foreach (RPSPanel p in panels) {
            if (p.IsEnd() == false) {   // 연출 대기일 때는 미결정으로 다룹니다.
                return RPSKind.None;
            }
        }

        return m_selected;
    }
    
}
