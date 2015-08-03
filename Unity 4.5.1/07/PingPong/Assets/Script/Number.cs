using UnityEngine;
using System.Collections;
using System;

/** 3자리 숫자를 표시 */
public class Number : MonoBehaviour {
    GameObject[] m_asciiObj;


	// Use this for initialization
	void Start () {
        m_asciiObj = new GameObject[3];

        string[] names = { "num1", "num2", "num3" };
        for (int i = 0; i < 3; ++i) {
            Transform num = transform.FindChild(names[i]);
            m_asciiObj[i] = num.gameObject;
        }
	}
	
	
    
    public void SetNum(int num){
        int div = 1;
        for (int i = 0; i < m_asciiObj.Length; ++i) {
            int n = (num / div) % 10;

            AsciiCharacter ac = m_asciiObj[i].GetComponent<AsciiCharacter>();
            if (ac) {
                ac.SetNumber(n);
            }
            
            div *= 10;
        }
    }
}
