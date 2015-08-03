using UnityEngine;
using System.Collections;


public struct MouseData
{
	public int		frame;
	public bool		mouseButtonLeft;
	public bool		mouseButtonRight;
	
	public float 	mousePositionX;
	public float 	mousePositionY;
	public float 	mousePositionZ;

    public override string ToString() {
        string str = "";
        str += "frame:" + frame;
        str += " mouseButtonLeft:" + mouseButtonLeft;
        str += " mouseButtonRight:" + mouseButtonRight;
        str += " mousePositionX:" + mousePositionX;
        str += " mousePositionY:" + mousePositionY;
	    str += " mousePositionZ:" + mousePositionZ;
        return str;
    }

};

public struct InputData
{   
	public int 			count;		// 데이터 수. 
	public int			flag;		// 접속 종료 플래그.
	public MouseData[] 	datum;		// 키입력 정보.
};


public class InputManager : MonoBehaviour {

    MouseData[] m_syncedInputs = new MouseData[2]; //동기화된 입력값.
    MouseData m_localInput; //현재 입력값(이 값을 송신시킨다).
    

    // Update is called once per frame
    void FixedUpdate() {
        //Debug.Log(gameObject.name + Time.frameCount.ToString() + " scale:" + Time.timeScale.ToString());

        m_localInput.mouseButtonLeft = Input.GetMouseButton(0);
        m_localInput.mouseButtonRight = Input.GetMouseButton(1);


        //마우스 좌표 계산.
        //그대로 넣으면 윈도우 크기 차이로 곤란해지니 변환합니다.
        Vector3 pos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(pos);

        Plane plane = new Plane(Vector3.up, Vector3.zero);
        float depth;
        plane.Raycast(ray, out depth);

        Vector3 worldPos = ray.origin + ray.direction * depth;

        m_localInput.mousePositionX = worldPos.x;
        m_localInput.mousePositionY = worldPos.y;
        m_localInput.mousePositionZ = worldPos.z;
    }

    //현재 입력값을 반환합니다.
    public MouseData GetLocalMouseData() {
        return m_localInput;
    }

    //동기화된 입력값을 반환합니다.
    public MouseData GetMouseData(int id) {
        //		Debug.Log("id:" + id + "' " + inputData.Length);
        return m_syncedInputs[id];
    }

    //동기화된 입력값 설정용.
    public void SetInputData(int id, MouseData data) {
        m_syncedInputs[id] = data;
    }
}

