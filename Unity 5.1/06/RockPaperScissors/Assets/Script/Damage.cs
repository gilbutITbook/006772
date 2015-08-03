using UnityEngine;
using System.Collections;

/** 회전해서 날아가는 패턴 */
public class Damage : MonoBehaviour {
    float m_speed;

    // Use this for initialization
    void Start() {
        //SE.
        GetComponent<AudioSource>().clip = GetComponent<Player>().m_hitSE;
        GetComponent<AudioSource>().Play();
        //Effect.
        GameObject effect = transform.FindChild("HitEffect").gameObject;
        effect.transform.parent = null;                 // 캐릭터에 따르지 않게 부모 설정을 해제합니다.
        effect.GetComponent<ParticleSystem>().Play();   // 재생.


        //물리 적용.
        gameObject.AddComponent<Rigidbody>();
        GetComponent<Rigidbody>().AddForce(Vector3.up * 6.0f, ForceMode.VelocityChange);    //위로 날립니다.

        //-2.0f～-1.0f 범위에서 만듭니다.
        float r = Random.Range(-2.0f, -1.0f);
        if (gameObject.name == "Daizuya") {
            r = -r;     //1P,2P 전환.
        }

        if (r < 0) {
            m_speed = 1.0f;
        }
        else {
            m_speed = -1.0f;
        }

        GetComponent<Rigidbody>().AddForce(Vector3.right * r, ForceMode.VelocityChange);
        //Debug.Log(r);

        //Debug.Log("start" + Time.time);
        gameObject.GetComponent<Player>().ChangeAnimation(Player.Motion.Damage);
    }

    // Update is called once per frame
    void Update() {
        transform.Rotate(Vector3.up * 900 * Time.deltaTime * m_speed, Space.Self);       //가로로 회전.
        transform.Rotate(Vector3.forward * 200 * Time.deltaTime * m_speed, Space.World); //세로로 회전.
    }

    //hit.
    void OnCollisionEnter(Collision col) {
        //낙하 중에 뭔가 부딪히면 회전을 멈춥니다.
        if (GetComponent<Rigidbody>().velocity.y < 0) {
            if (m_speed != 0) {
                GetComponent<AudioSource>().clip = GetComponent<Player>().m_collideGroundSE; //m_collideSE;
                GetComponent<AudioSource>().Play();
            }

            m_speed = 0;
        }

        //Debug.Log("col" + Time.time);
    }
}
