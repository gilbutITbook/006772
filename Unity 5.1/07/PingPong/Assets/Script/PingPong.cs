using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;


public class PingPong : MonoBehaviour {
    public GameObject m_serverBarPrefab;
    public GameObject m_clientBarPrefab;
    public GameObject m_gameControllerPrefab;
    public GameObject m_resultControllerPrefab;

	GameMode		m_gameMode;     //게임 모드.
    float			m_timeScale;    //기본 타임 스케일을 기억해 둡니다.

    //네트워크.
    string m_hostAddress;
    NetworkController m_networkController = null;


	public enum GameMode {
		Ready = 0,  //접속 대기.        
        Game,       //게임 중.
        Result,     //결과 표시.
	};


	void Awake()
	{
		m_timeScale = 1;
		Time.timeScale = 0;
	}


	// Use this for initialization
	void Start()
	{
        m_gameMode = GameMode.Ready;

        // 호스트 이름을 가져옵니다.
        string hostname = Dns.GetHostName();
        // 호스트 이름에서 IP 주소를 가져옵니다.
        IPAddress[] adrList = Dns.GetHostAddresses(hostname);
        m_hostAddress = adrList[0].ToString();
	}
	
	// Update is called once per frame
	void FixedUpdate() {
        //Debug.Log(gameObject.name + Time.frameCount.ToString() + " scale:" + Time.timeScale.ToString());

        switch (m_gameMode) {
		case GameMode.Ready:
			UpdateReady();
			break;

		case GameMode.Game:
			UpdateGame();
			break;

		case GameMode.Result:
			UpdateResult();
			break;
		}

		// 프레임 동기화를 진행해도 되는지 확인합니다.
        if (m_networkController!=null && m_networkController.IsSync()) {
            // 프레임 동기화를 진했으므로 플래그를 클리어합니다.
            m_networkController.ClearSync();
            
            Time.timeScale = 0; // FixedUpdate 관련 갱신은 fixedDeltaTimｅ으로 갱신되는데 2번째 이후의 호출을 막고자 =0으로 합니다.
		}
	}

	void LateUpdate(){
        if (m_networkController != null) {
            if (m_networkController.UpdateSync()) {
                Resume();   // 정지 상태를 해제합니다.
            }
            else {
                // 입력 정보를 수신하지 않았으므로 다음 프레임을 처리할 수 없습니다.
                Suspend();
            }
		}
	}



	//접속 대기.
	void UpdateReady(){
        // 통신 접속을 기다려 게임을 시작합니다.
        if (m_networkController != null) {
            if (m_networkController.IsConnected() == true) {
                NetworkController.HostType hostType = m_networkController.GetHostType();
                GameStart(hostType == NetworkController.HostType.Server);

                m_gameMode = GameMode.Game;
            }
        }
	}


    //게임 중
    void UpdateGame() {
        GameObject gameController = GameObject.Find(m_gameControllerPrefab.name);
        if (gameController == null) {
            gameController = Instantiate(m_gameControllerPrefab) as GameObject;
            gameController.name = m_gameControllerPrefab.name;
            GameObject.Find("BGM").GetComponent<AudioSource>().Play();    //BGM재생.
            return;
        }

        if (gameController.GetComponent<GameController>().IsEnd()) {
 			m_networkController.SuspendSync();
			if (m_networkController.IsSuspned() == true) {
				m_gameMode = GameMode.Result;
			}
        }
    }
    

	//
	void UpdateResult(){
        //결과를 표시하고 승부를 낸다한다.
        GameObject resultController = GameObject.Find(m_resultControllerPrefab.name);
        if (resultController == null) {
            resultController = Instantiate(m_resultControllerPrefab) as GameObject;
            resultController.name = m_resultControllerPrefab.name;
            GameObject.Find("BGM").SendMessage("FadeOut");    //BGM 페이드 아웃.
            return;
        }
	}

	

	public void Resume(){
        Time.timeScale = m_timeScale;
	}

	public void Suspend(){
		Time.timeScale = 0;
	}



	void GameStart(bool isServer){
        //바를 생성.
        GameObject serverBar = Instantiate(m_serverBarPrefab) as GameObject;
        serverBar.GetComponent<BarScript>().SetBarId(0);
        serverBar.name = m_serverBarPrefab.name;
        GameObject clientBar = Instantiate(m_clientBarPrefab) as GameObject;
        clientBar.GetComponent<BarScript>().SetBarId(1);
        clientBar.name = m_clientBarPrefab.name;


        // 클라이언트의 경우는 2P용 카메라로 합니다.
        if (isServer == false) {
            Vector3 cameraPos = Camera.main.transform.position;
            cameraPos.y *= -1;
            cameraPos.x *= -1;
            Camera.main.transform.position = cameraPos;

            Vector3 cameraRot = Camera.main.transform.rotation.eulerAngles;
            cameraRot.x *= -1;
            cameraRot.y *= -1;
            cameraRot.z += 180;
            Camera.main.transform.rotation = Quaternion.Euler(cameraRot);

            GameObject light = GameObject.Find("Directional light");
            Vector3 lightRot = light.transform.rotation.eulerAngles;
            lightRot.x *= -1;
            light.transform.rotation = Quaternion.Euler(lightRot);
        }
	}



    void OnGUI() {
        //버튼이 눌리면 통신 시작.
        if (m_networkController == null) {
            PlayerInfo info = PlayerInfo.GetInstance();

			int x = 50;
			int y = 650;

			// 클라이언트를 선택했을 때 접속할 서버의 주소를 입력합니다.
			GUIStyle style = new GUIStyle();
			style.fontSize = 18;
			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.black;
			GUI.Label(new Rect(x, y-25, 200.0f, 50.0f), "대전 상대의 IP 주소", style);
			m_hostAddress = GUI.TextField(new Rect(x, y, 200, 20), m_hostAddress);
			y += 25;

			if (GUI.Button(new Rect(x, y, 150, 20), "대전 상대를 기다립니다")) {
				m_networkController = new NetworkController(m_hostAddress, true);    //서버 시작.
                info.SetPlayerId( 0 );  // 플레이어 ID를 설정합니다.

                GameObject.Find("Title").SetActive(false); //타이틀 표시 OFF.
            }

			if (GUI.Button(new Rect(x+160, y, 150, 20), "대전 상대와 접속합니다")) {
                m_networkController = new NetworkController(m_hostAddress, false);   // 클라이언트 시작.
                info.SetPlayerId( 1 );  // 플레이어 ID를 설정합니다.

                GameObject.Find("Title").SetActive(false); // 타이틀 표시 OFF.
            }
        }


        //결과 화면 종료 시는 버튼으로 리셋할 수 있게 합니다.
        GameObject resultController = GameObject.Find(m_resultControllerPrefab.name);
        if (resultController && resultController.GetComponent<ResultController>().IsEnd()) {
            // 종료 버튼 표시.
            if (GUI.Button(new Rect(20, Screen.height - 100, 80, 80), "RESET")) {
                Application.LoadLevel("PingPong");
            } 
			return;
        }

		// 연결 끊김 확인 .
		if (m_networkController != null &&
			m_networkController.IsConnected() == false &&
		    m_networkController.IsSuspned() == false &&
		    m_networkController.GetSyncState() != NetworkController.SyncState.NotStarted) {
			// 끊었습니다.
			NotifyDisconnection(); 
		}
    }

	
	// 접속 끊김 통지 .
	void NotifyDisconnection()
	{
		GUISkin skin = GUI.skin;
		GUIStyle style = new GUIStyle(GUI.skin.GetStyle("button"));
		style.normal.textColor = Color.white;
		style.fontSize = 25;
		
		float sx = 450;
		float sy = 200;
		float px = Screen.width / 2 - sx * 0.5f;
		float py = Screen.height / 2 - sy * 0.5f;
		
		string message = "연결이 끊어졌습니다.\n\n버튼을 누르세요.";
		
		// 종료 버튼 표시.  
		if (GUI.Button (new Rect (px, py, sx, sy), message, style)) {
			// 게임이 끝났습니다.
			Application.LoadLevel("PingPong");
		}
	}

}


