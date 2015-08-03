using UnityEngine;
using System.Collections;

/** 숫자, 문자 표시용 **/
public class AsciiCharacter : MonoBehaviour {
    public Sprite[] m_sprites;
    int m_index;

    void Awake() {
        m_index = 0;
    }

    public void SetNumber(int num) {
        m_index = num + 16;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = m_sprites[m_index];
    }
    public int GetNumber() {
        int num = m_index - 16;
        return num;
    }

    public void SetChar(char c) {
        if (c < ' ' || c > '?') {
            return;
        }

        m_index = c;   
        m_index -= ' ';

        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = m_sprites[m_index];
        
    }
}
