using UnityEngine;
using System;
using System.Collections;

public class BallScript : MonoBehaviour {
    public GameObject m_hitEffectPrefab; // 타격 시 효과.
    int m_playerId;
	//
	float m_velocity;
	Vector2 m_direction;

	//
	bool m_isMissed = false;
    const float DEADLINE = 6.0f;

	// Use this for initialization
	void Start()
	{
        m_velocity = 4.0f;
        m_direction = new Vector2(1.0f, 1.0f).normalized;
        if (m_playerId == 1) {
            m_direction *= -1.0f;
		}
        rigidbody2D.velocity = m_velocity * m_direction.normalized;
	}
	
	// Update is called once per frame
	void FixedUpdate()
	{
		CheckMissBall();
        if (IsMissed()) {
            Destroy(gameObject);
        }
	}

	//
	public void SetPlayerId(int id)
	{
        m_playerId = id;

        Transform model = transform.FindChild("sara");
        
        if (id == 0) {
            model.renderer.material.color = Color.blue * 0.3f + Color.white * 0.7f;
        }
        else {
            model.renderer.material.color = Color.red * 0.3f + Color.white * 0.7f;
        }
	}

	public int GetPlayerId()
	{
        return m_playerId;
	}

	//
	public bool IsMissed()
	{
        return m_isMissed;
	}

	//
	void OnCollisionEnter2D(Collision2D col)
	{
        //효과 생성.  //그대로 두면 지면에 파고들기에 카메라 방향으로 위치를 옮깁니다
        Vector3 effectPos = transform.position - 3 * Camera.main.transform.forward;
        Instantiate(m_hitEffectPrefab, effectPos, transform.rotation);
        //SE
        audio.Play();


        if (col.gameObject.tag == "Bar") {
            BarScript bar = col.gameObject.GetComponent<BarScript>();
            SetPlayerId( bar.GetBarId() );
            m_velocity += 0.3f;
        }

       
		if (col.gameObject.tag == "Ball") {
			// 공 끼리의 충돌은 역방향으로 튀게 합니다.
            m_direction *= -1.0f;
		} else {
            // 공 이외의 충돌은 충돌면의 법선 방향으로 반전합니다.
            float range = Mathf.Sin( Mathf.Deg2Rad * 3 ); //3도 정도는 허용합니다.

            Vector2 normal = col.contacts[0].normal;
            if (normal.x > range || normal.x < -range) {
                m_direction.x *= -1.0f;
            }
            if (normal.y > range || normal.y < -range) {
                m_direction.y *= -1.0f;
            }
		}

		// 반전된 방향으로 현재 속도를 합해 설정합니다.
        rigidbody2D.velocity = m_velocity * m_direction;
    }


    //미스체크
	void CheckMissBall()
	{
		Vector3 pos = transform.position;

		if (pos.y > DEADLINE || pos.y < -DEADLINE) {
            m_isMissed = true;    // 미스.
		}
        else {
            return;
        }

        //어느 쪽 플레이어가 실수했는지 id를 결정합니다.
        int missPlayerId = 0;
        if (pos.y > DEADLINE) {
            missPlayerId = 1;
        }
        
        PlayerInfo info = PlayerInfo.GetInstance();
        GameObject scoreObj;
        if (missPlayerId == info.GetPlayerId()) {
            scoreObj = GameObject.Find("PlayerScore");  //자신의 스코어를 줄이고 싶습니다.            
        }
        else {            
            scoreObj = GameObject.Find("OpponentScore"); //상대 플레이어의 점수를 줄이고 싶습니다.
        }
        //점수를 줄입니다.
        UserScore score = scoreObj.GetComponent<UserScore>();
        score.PopScore();
        if (missPlayerId != m_playerId) { //상대의 볼로 미스.
            score.PopScore();
            score.PopScore();
        }
            
	}
}
