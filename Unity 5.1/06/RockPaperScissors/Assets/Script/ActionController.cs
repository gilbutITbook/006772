using UnityEngine;
using System.Collections;

/** 전투 액션 제어 */
public class ActionController : MonoBehaviour {
    float m_time;


	// Use this for initialization
	void Start () {
        m_time = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
        
	}


    // 연출이 끝나면 true.
    public bool IsEnd() {
        float dt = Time.time - m_time;
        return (dt > 5.0f);
    }



    // 재생시킬 모션과 승패판정, 현재 득점을 전달합니다.
    public void Setup(Winner winner, int serverScore, int clientScore) {
        GameObject serverPlayer = GameObject.Find("Daizuya");
        GameObject clientPlayer = GameObject.Find("Toufuya");

        // 슬로우 재생을 원래대로 되돌리고 날아가는 액션을 발동시킵니다.
        float delay = 0.0f;
        switch (winner) {
        case Winner.ServerPlayer:
            serverPlayer.GetComponent<Player>().SetDefaultAnimationSpeed();
            serverPlayer.GetComponent<Collider>().enabled = false; // 날아가는 처리에 간섭하지 않게 충돌을 끕니다.
            delay = serverPlayer.GetComponent<Player>().GetRemainAnimationTime();
            clientPlayer.GetComponent<Player>().StartDamage(serverScore, delay * 0.3f); // 모션이 절반 정도로 끝나므로*0.5f합니다.
            break;
        case Winner.ClientPlayer:
            clientPlayer.GetComponent<Player>().SetDefaultAnimationSpeed();
            clientPlayer.GetComponent<Collider>().enabled = false; // 날아가는 처리에 간섭하지 않게 충돌을 끕니다.
            delay = clientPlayer.GetComponent<Player>().GetRemainAnimationTime();
            serverPlayer.GetComponent<Player>().StartDamage(clientScore, delay * 0.3f); // 모션이 절반 정도로 끝나므로 *0.5f합니다.
            break;
        case Winner.Draw:
        default:
            serverPlayer.GetComponent<Player>().SetDefaultAnimationSpeed();
            clientPlayer.GetComponent<Player>().SetDefaultAnimationSpeed();
            serverPlayer.GetComponent<Player>().ActionEffectOn(); // 공격 미스음, 실패음을 유효하게 합니다.
            clientPlayer.GetComponent<Player>().ActionEffectOn();
            break;
        }
    }

}

