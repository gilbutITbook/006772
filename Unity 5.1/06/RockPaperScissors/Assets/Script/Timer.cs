using UnityEngine;
using System.Collections;
using System;

public class Timer : MonoBehaviour {
    GameObject[] m_numbers;
    float m_timer;
    bool m_isStop;
    
	// Use this for initialization
	void Start () {
        // 타이머 표시를 위해 잡아둡니다.
        m_numbers = new GameObject[5];
        for (int i = 0; i < m_numbers.Length; ++i){
            m_numbers[i] = GameObject.Find("Number" + i);
        }
        
        // 소숫점 표시.
        GameObject dot = GameObject.Find("Dot");
        AsciiCharacter ascii = dot.GetComponent<AsciiCharacter>();
        ascii.SetChar('.');

        // 타이머 초기화.
        m_isStop = false;
        m_timer = Time.time;
        UpdateTimer();
        //SetNumber(3 * 1000);
	}
	
	// Update is called once per frame
	void Update () {
        if (m_isStop) {
            return; // 정지 상태라면 아무것도 하지 않습니다.
        }

        //타이머 표시 갱신.
        UpdateTimer();
	}


    //타이머 표시 갱신.
    void UpdateTimer() {
        float time = 3.0f - (Time.time - m_timer);
        if (time < 0.0f) {
            time = 0.0f;
        }

        int count = (int)(time * 1000);
        SetNumber(count);
    }


    //5자리 번호를 표시합니다.
    void SetNumber(int num) {
        foreach (GameObject obj in m_numbers) {
            AsciiCharacter ascii = obj.GetComponent<AsciiCharacter>();
            ascii.SetNumber( num % 10 );

            num /= 10;           
        }
    }

    // 표시 내용을 바탕으로 경과 시간을 획득.
    public float GetNumber() {
        int num = 0;
        for (int i = 0; i < m_numbers.Length; ++i) {
            AsciiCharacter ascii = m_numbers[i].GetComponent<AsciiCharacter>();
            num += ascii.GetNumber() * (int)Math.Pow(10, i);
        }
        return 3.0f - (num / 1000.0f);
    }


    // 남은 시간이 0이하라면 true.
    public bool IsTimeZero(){
        float time = 3.0f - (Time.time - m_timer);
        return (time < 0.0f);
    }

    // 타이머를 정지시킵니다.
    public void Stop() {
        m_isStop = true;
        UpdateTimer();
    }

}
