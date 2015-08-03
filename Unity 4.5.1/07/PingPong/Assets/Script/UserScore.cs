using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UserScore : MonoBehaviour {
    public enum UserType {
        Player,     //자기.
        Opponent,   //상대.
    }
    public UserType m_userType;
    public GameObject[] m_scoreSushiPrefabs; //스코어 표시용 초밥을 넣습니다.

    Dictionary<SushiType, int> m_fixedScore;   //획득한 초밥 카운트업(이쪽은 획득 완료로 다룸).
    List<GameObject> m_scoreSushi;  // 획득한 초밥(이쪽은 획득 보류로 다루므로 늘거나 줄어든다).
    const int FIXED_NUM = 7;        //일정 수를 획득하면 fixed 쪽으로 이동.

	// Use this for initialization
	void Start () {
        m_fixedScore = new Dictionary<SushiType, int>();
	    m_scoreSushi = new List<GameObject>();
        
        //스코어 초기화.
        foreach( SushiType s in Enum.GetValues(typeof(SushiType)) ){
            m_fixedScore[s] = 0;
        }
	}
	
	// Update is called once per frame
	void FixedUpdate() {

        //FIXED_NUM개마다 확정 & 표시를 리셋시킵니다.
        if (m_scoreSushi.Count >= FIXED_NUM) {
            if (m_scoreSushi[0].GetComponent<ScoreSushi>().IsFadeOut()) {
                //연출 대기-------------------------------------------
                for (int i = 0; i < FIXED_NUM; ++i) {
                    ScoreSushi sushi = m_scoreSushi[i].GetComponent<ScoreSushi>();
                    if (sushi.IsFadeOutEnd() == false) {
                        return;
                    }
                }

                //SE.
                audio.Play();
                //소화 애니메이션이 끝났으므로 지웁니다.
                for (int i = 0; i < FIXED_NUM; ++i) {
                    ScoreSushi sushi = m_scoreSushi[i].GetComponent<ScoreSushi>();
                    m_fixedScore[sushi.m_sushiType]++;
                    Destroy(m_scoreSushi[i]);
                }
                m_scoreSushi.RemoveRange(0, FIXED_NUM);

                //남아 있는 표시물의 위치 맞추기.
                for (int i = 0; i < m_scoreSushi.Count; ++i) {
                    Vector3 pos = MakePosition(m_userType, i);
                    m_scoreSushi[i].transform.position = pos;
                }

                return;
            }
            else {
                //소화 준비-------------------------------------------
                //소화 애니메이션이 끝나고 나서 삭제해 주세요.
                GameObject dog = GameObject.Find(m_userType.ToString() + "Dog");
                Vector3 target = dog.transform.FindChild("target").position;
                for (int i = 0; i < FIXED_NUM; ++i) {
                    ScoreSushi sushi = m_scoreSushi[i].GetComponent<ScoreSushi>();
                    sushi.StartFadeOut(target); //연출 시작.
                }

            }
        }
	}



    //스코어 추가.
    public void PushScore(SushiType sushiType) {
        Vector3 pos = MakePosition(m_userType, m_scoreSushi.Count);
        
        //스코어용 초밥을 표시합니다.
        GameObject obj = Instantiate(
            m_scoreSushiPrefabs[(int)sushiType],
            pos, Quaternion.identity * Quaternion.Euler(0,0,15)
        ) as GameObject;

        obj.transform.parent = transform;
        m_scoreSushi.Add(obj);
    }

    //스코어 추출(삭제).
    public void PopScore() {
        if (m_scoreSushi.Count == 0) {
            return; //삭제할 것이 없습니다.
        }

        int last = m_scoreSushi.Count - 1;
        GameObject obj = m_scoreSushi[last];
        if (obj.GetComponent<ScoreSushi>().IsFadeOut()) {
            return; //페이드 아웃 중인 것은 지우지 않는다.
        }

        Destroy(obj);
        m_scoreSushi.RemoveAt(last);
    }

    //스코어 획득.
    public int GetCount(SushiType sushiType) {
        //획득 완료되지 않은 것(표시분)을 카운트.
        int count = 0;
        foreach(GameObject obj in m_scoreSushi){
            ScoreSushi sushi = obj.GetComponent<ScoreSushi>();
            SushiType type = sushi.m_sushiType;
            if(type == sushiType){
                count++;
            }
        }

        //획득된 것과 더해서 반환합니다.
        return m_fixedScore[sushiType] + count;
    }


    //스코어 표시용 초밥의 좌표를 정합니다.
    //Player이면 오른쪽 위로, Opponent이면 왼쪽 아래로 늘려갑니다.
    Vector3 MakePosition(UserType userType, int index) {
        Vector3 pos = transform.position;
        const float DISTANCE = 0.6f;
        switch (userType) {
        case UserType.Player:
            pos += Vector3.up * DISTANCE * index;
            pos -= Vector3.forward * -DISTANCE * index;
            return pos;
        case UserType.Opponent:
            pos -= Vector3.up * DISTANCE * index;
            pos -= Vector3.forward * DISTANCE * index;
            return pos;
        }
        return pos;
    }
}

