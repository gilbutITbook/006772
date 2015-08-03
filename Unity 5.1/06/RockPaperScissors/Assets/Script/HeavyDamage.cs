using UnityEngine;
using System.Collections;

/** 성대하게 날아가는 패턴 */
public class HeavyDamage : MonoBehaviour {
    float m_boundPower; // 다른 것에 부딪혔을 때의 바운드되는 정도.
    float m_speed;

    //날아가는 패턴2.
    void Start() {
        //SE.
        GetComponent<AudioSource>().clip = GetComponent<Player>().m_hitSE; ;
        GetComponent<AudioSource>().Play();
        //Effect.
        GameObject effect = transform.FindChild("HitEffect").gameObject;
        effect.transform.parent = null;                 // 캐릭터에 따르지 않게 부모 설정을 해제합니다.
        effect.GetComponent<ParticleSystem>().Play();   //재생.


        //물리 적용.
        gameObject.AddComponent<Rigidbody>();
        GetComponent<Rigidbody>().AddForce(0.0f, 5.0f, 2.0f, ForceMode.VelocityChange);    //안쪽으로 날립니다.

        //-2.5f～1.0f 범위로 만듭니다.
        float r = Random.Range(-2.5f, 1.0f);
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

        m_boundPower = 1.0f;
        gameObject.GetComponent<Player>().ChangeAnimation(Player.Motion.Damage);
    }


    void Update() {
        transform.Rotate(Vector3.up * 900 * Time.deltaTime * m_speed, Space.Self);       //가로로 회전.
        transform.Rotate(Vector3.forward * 200 * Time.deltaTime * m_speed, Space.World); //세로로 회전.
    }


    void OnCollisionEnter(Collision col) {
        if (GetComponent<Rigidbody>().velocity.y >= 0 && col.gameObject.name == "ground") {
            return;     //지면에서 뜨기 전에 oncollisionenter되어 버리므로 대응.
        }
        //SE.
        if (col.gameObject.name == "ground") {  // 지면 충돌 시에는 지면용 효과음을 재생.
            GetComponent<AudioSource>().clip = GetComponent<Player>().m_collideGroundSE;
            GetComponent<AudioSource>().Play();
        }
        else if (m_boundPower > 0.1f) {         //어느 정도 약해지면 효과음이 울리지 않게 합니다.
            GetComponent<AudioSource>().clip = GetComponent<Player>().m_collideSE;
            GetComponent<AudioSource>().Play();
        }

        
        //히트한 장소에 힘을 더합니다. 재미있게 날아가게 파라미터를 가공합니다.
        Vector3 v = col.relativeVelocity;
        if (v.y < 0) {
            v.y = -v.y;
        }
        v.z = -v.z;

        //자신을 날립니다.
        GetComponent<Rigidbody>().AddForce(Vector3.up * 4 * m_boundPower, ForceMode.VelocityChange);

        //충돌한 것을 날립니다.
        Rigidbody rb = col.gameObject.GetComponent<Rigidbody>();
        if (rb) {
            rb.AddForceAtPosition(v * 2.0f * m_boundPower, col.contacts[0].point, ForceMode.VelocityChange);
        }


        //뭔가에 부딪히면 회전을 멈춥니다.
        m_speed = 0;
        //뭔가에 부딪히면 반동을 약하게 해 갑니다.
        m_boundPower *= 0.8f;
    }
}
