using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;

public class RockPaperScissors : MonoBehaviour {
    public GameObject m_serverPlayerPrefab; // 서버 측 플레이어 캐릭터.
    public GameObject m_clientPlayerPrefab; // 클라이언트 측 플레이어 캐릭터.

    public GameObject m_RPSSelectorPrefab;  //가위바위보 선택.
    public GameObject m_shootCallPrefab;    //가위바위보! 구호 연출용.
    public GameObject m_battleSelectPrefab; //공수 선택.
    public GameObject m_actionControllerPrefab; //전투 연출.
    public GameObject m_resultScenePrefab;  //결과 표시.

    public GameObject m_finalResultWinPrefab;   //최종 결과 승리.
    public GameObject m_finalResultLosePrefab;  //최종 결과 패배.

    const int PLAYER_NUM = 2;
    const int PLAY_MAX = 3;
    GameObject m_serverPlayer;  //자주 사용하므로 확보.
    GameObject m_clientPlayer;  //자주 사용하므로 확보.

    GameState m_gameState = GameState.None;
	InputData[]         m_inputData = new InputData[PLAYER_NUM];
    NetworkController   m_networkController = null;
    string              m_serverAddress;

    int 				m_playerId = 0;
	int[]				m_score = new int[PLAYER_NUM];
    Winner              m_actionWinner = Winner.None;
	
	bool				m_isGameOver = false;

    // 공수의 송수신 대기용.
    float m_timer;
    bool m_isSendAction;
    bool m_isReceiveAction;

	
	// 게임 진행 상황.
	enum GameState
	{
		None = 0,
		Ready,      // 게임 상대의 로그인 대기.
		SelectRPS,  //가위바위보 선택.
		WaitRPS,    //수신대기.
		Shoot,      //가위바위보 연출.
		Action,     //때리기 피하기 선택・수신대기.
		EndAction,  //때리기 피하기 연출.
		Result,     //결과 발표.
		EndGame,    //끝.
		Disconnect,	//오류.
	};

		
	// Use this for initialization
	void Start () {
        ////ResultChecker.WinnerTest();

        m_serverPlayer = null;
        m_clientPlayer = null;

        m_timer = 0;
        m_isSendAction = false;
        m_isReceiveAction = false;

		// 초기화합니다. 
        for (int i = 0; i < m_inputData.Length; ++i) {
            m_inputData[i].rpsKind = RPSKind.None;
            m_inputData[i].attackInfo.actionKind = ActionKind.None;
            m_inputData[i].attackInfo.actionTime = 0.0f;
        }

		// 아직 동작시키지 않습니다.
		m_gameState = GameState.None;

		
		for (int i = 0; i < m_score.Length; ++i) {
			m_score[i] = 0;	
		}

		// 통신 모듈 작성.
		GameObject go = new GameObject("Network");
		if (go != null) {
			TransportTCP transport = go.AddComponent<TransportTCP>();
			if (transport != null) {
				transport.RegisterEventHandler(EventCallback);
			}
			DontDestroyOnLoad(go);
		}

        // 호스트명 가져오기.
        string hostname = Dns.GetHostName();
        // 호스트명에서 IP주소를 가져옵니다.
        IPAddress[] adrList = Dns.GetHostAddresses(hostname);
        m_serverAddress = adrList[0].ToString();	
	}
	
	// Update is called once per frame
	void Update () {
       // Debug.Log(m_gameState);
	
		switch (m_gameState) {
		case GameState.None:
			break;

		case GameState.Ready:
			UpdateReady();
			break;

		case GameState.SelectRPS:
			UpdateSelectRPS();
			break;

		case GameState.WaitRPS:
			UpdateWaitRPS();
			break;

		case GameState.Shoot:
			UpdateShoot();
			break;

		case GameState.Action:
			UpdateAction();
			break;

		case GameState.EndAction:
			UpdateEndAction();
			break;

		case GameState.Result:
			UpdateResult();
			break;

		case GameState.EndGame:
			UpdateEndGame();
			break;

		case GameState.Disconnect:
			break;
		}
       // Debug.Log(Time.deltaTime);
	}
	
	void OnGUI() {
		switch (m_gameState) {
		case GameState.EndGame:
			OnGUIEndGame();
			break;

		case GameState.Disconnect:
			NotifyDisconnection();
			break;
		}

		float px = Screen.width * 0.5f - 100.0f;
		float py = Screen.height * 0.75f;
		
		//미접속일 때의 GUI 표시.
        if (m_networkController == null) {
			if (GUI.Button(new Rect(px, py, 200, 30), "대전 상대를 기다립니다")) {
                m_networkController = new NetworkController();  //서버.
                m_playerId = 0;
                m_gameState = GameState.Ready;
                m_isGameOver = false;

                //플레이어 생성.
                m_serverPlayer = Instantiate(m_serverPlayerPrefab) as GameObject;
                m_serverPlayer.name = m_serverPlayerPrefab.name;

                GameObject.Find("BGM").GetComponent<AudioSource>().Play(); //BGM.
                GameObject.Find("Title").SetActive(false); //타이틀 표시 OFF.
            }

            // 클라이언트를 선택했을 때 접속할 서버의 주소를 입력합니다.
			Rect labelRect = new Rect(px, py + 80, 200, 30);
			GUIStyle style = new GUIStyle();
			style.fontStyle = FontStyle.Bold;
			style.normal.textColor = Color.white;
			GUI.Label(labelRect, "상대방 IP 주소", style);
			labelRect.y -= 2;
			style.fontStyle = FontStyle.Normal;
			style.normal.textColor = Color.black;
            GUI.Label(labelRect, "상대방 IP 주소", style);
			m_serverAddress = GUI.TextField(new Rect(px, py + 95, 200, 30), m_serverAddress);

			if (GUI.Button(new Rect(px, py + 40, 200, 30), "대전 상대와 접속합니다")) {
                m_networkController = new NetworkController(m_serverAddress);  //서버.
                m_playerId = 1;
                m_gameState = GameState.Ready;
                m_isGameOver = false;

                //플레이어 생성.
                m_clientPlayer = Instantiate(m_clientPlayerPrefab) as GameObject;
                m_clientPlayer.name = m_clientPlayerPrefab.name;
                //플레이어 위치 조정.
                GameObject board = GameObject.Find("BoardYou");
                Vector3 pos = board.transform.position;
                pos.x *= -1;
                board.transform.position = pos;

                GameObject.Find("BGM").GetComponent<AudioSource>().Play(); //BGM.
                GameObject.Find("Title").SetActive(false); //타이틀 표시 OFF.
            }
        }
	}
		
	
    //로그인 대기.
	void UpdateReady()
	{        
        //접속확인.
        if (m_networkController.IsConnected() == false) {
            return;
        }

        //플레이어 캐릭터가 만들어지지 않았으면 생성합니다.
        if (m_serverPlayer == null) {
            m_serverPlayer = Instantiate(m_serverPlayerPrefab) as GameObject;
            m_serverPlayer.name = m_serverPlayerPrefab.name;
        }
        if (m_clientPlayer == null) {
            m_clientPlayer = Instantiate(m_clientPlayerPrefab) as GameObject;
            m_clientPlayer.name = m_clientPlayerPrefab.name;
        }
        
        //모션이 Idle이 될 때까지 대기.
        if (m_serverPlayer.GetComponent<Player>().IsIdleAnimation() == false) {
            return;
        }
        if (m_clientPlayer.GetComponent<Player>().IsIdleAnimation() == false) {
            return;
        }
        
        // 플레이어 제시가 끝날 때까지 대기.
        GameObject board = GameObject.Find("BoardYou");
        if (board == null) {
            return;
        }
        if (board.GetComponent<BoardYou>().IsEnd() == false) {
            return;
        }

        //모든 대기를 통과했으므로 다음 상태로.
        board.GetComponent<BoardYou>().Sleep(); //표기 OFF.
        m_gameState = GameState.SelectRPS;
	}


    //가위바위보 선택.
	void UpdateSelectRPS()
	{
        GameObject obj = GameObject.Find("RPSSelector");
        if (obj == null) {
            // 연출용 오브젝트가 없다면 생성합니다 
			// 이 시퀀스의 초기동작입니다.
            obj = Instantiate(m_RPSSelectorPrefab) as GameObject;
            obj.name = "RPSSelector";
            return;
        }
// 선택한 가위바위보의 종류를 가져옵니다.
        RPSKind rps = obj.GetComponent<RPSSelector>().GetRPSKind();
        if (rps != RPSKind.None) {
			//선택한 가위바위보 손을 보관해 둡니다.
            m_inputData[m_playerId].rpsKind = rps;

            //선택한 가위바위보 손을 송신합니다.
			m_networkController.SendRPSData(m_inputData[m_playerId].rpsKind);
            m_gameState = GameState.WaitRPS;
        }
	}

    //가위바위보 선택 통신 대기.
	void UpdateWaitRPS()
	{
        //수신대기.
		RPSKind rps = m_networkController.ReceiveRPSData();
		if(rps == RPSKind.None) {
			//아직 수신되지 않음.
			return;
		}
		m_inputData[m_playerId ^ 1].rpsKind = rps;

        m_serverPlayer.GetComponent<Player>().SetRPS(m_inputData[0].rpsKind, m_inputData[1].rpsKind);
        m_clientPlayer.GetComponent<Player>().SetRPS(m_inputData[1].rpsKind, m_inputData[0].rpsKind);

		m_gameState = GameState.Shoot;

        //연출은 용도가 끝나면 제거.
        GameObject obj = GameObject.Find("RPSSelector");
        Destroy(obj);
	}

    //가위바뷔보 연출.
	void UpdateShoot()
	{
        GameObject obj = GameObject.Find("ShootCall");
        if (obj == null) {
            //연출용 오브젝트가 없으면 생성합니다(이 시퀀스의 초기 동작입니다).
            obj = Instantiate(m_shootCallPrefab) as GameObject;
            obj.name = "ShootCall";
            return;
        }

        // 가위바위보를 외치는 소리가 끝날 때까지 기다립니다.
        ShootCall sc = obj.GetComponent<ShootCall>();
        if (sc.IsEnd()) {
            Destroy(obj);
            m_gameState = GameState.Action;
        }
	}

    //공수 선택.
	void UpdateAction()
	{
        GameObject obj = GameObject.Find("BattleSelect");
        if (obj == null) {
            // 연출용 오브젝트가 없으면 생성한다(이 시퀀스의 초기 동작입니다).
            obj = Instantiate(m_battleSelectPrefab) as GameObject;
            obj.name = "BattleSelect";

            //선택한 가위바위보 손의 종류를 전달합니다.
            obj.GetComponent<BattleSelect>().Setup(m_inputData[0].rpsKind, m_inputData[1].rpsKind);

            //변수초기화.
            m_timer = Time.time;
            m_isSendAction = false;
            m_isReceiveAction = false;
            return;
        }

        // 선택 종료를 기다립니다.
        BattleSelect battleSelect = obj.GetComponent<BattleSelect>();
        if (battleSelect.IsEnd() && m_isSendAction == false) {
            //시간과 행동 선택을 가져옵니다.
            float time = battleSelect.GetTime();
            ActionKind action = battleSelect.GetActionKind();

            m_inputData[m_playerId].attackInfo.actionKind = action;
            m_inputData[m_playerId].attackInfo.actionTime = time;


            // 상대방에게 전송합니다. 
			m_networkController.SendActionData(action, time);

            // 애니메이션(자신).
            GameObject player = (m_playerId == 0) ? m_serverPlayer : m_clientPlayer;
            player.GetComponent<Player>().ChangeAnimationAction(action);
            
            m_isSendAction = true;          // 송신 성공.
        }

        //공수 수신 대기.
        if (m_isReceiveAction == false) {
            //수신 체크: 상대의 공격/방어를 체크합니다.
			bool isReceived = m_networkController.ReceiveActionData(ref m_inputData[m_playerId ^ 1].attackInfo.actionKind,
			                                                        ref m_inputData[m_playerId ^ 1].attackInfo.actionTime);

            if (isReceived) {
                //애니메이션(대전 상대).
                ActionKind action = m_inputData[m_playerId ^ 1].attackInfo.actionKind;
                GameObject player = (m_playerId == 1) ? m_serverPlayer : m_clientPlayer;
                player.GetComponent<Player>().ChangeAnimationAction(action);

                m_isReceiveAction = true;   //수신 성공.
            }
            else if (Time.time - m_timer > 5.0f) {
                //타임아웃이므로 상대의 입력은 기본 설정으로 합니다.
                m_inputData[m_playerId ^ 1].attackInfo.actionKind = ActionKind.None;
                m_inputData[m_playerId ^ 1].attackInfo.actionTime = 5.0f;
                m_isReceiveAction = true;   //타임아웃임로 수신 성공으로 합니다.
            }
        }

        //진행해도 좋은지 체크.
        if (m_isSendAction == true && m_isReceiveAction == true) {
            //통신 시에 하는 변환 처리로 값의 정밀도가 떨어지므로 여기서 값의 정밀도를 맞춘다.
            float time = m_inputData[m_playerId].attackInfo.actionTime;
            short actTime = (short)(time * 1000.0f);
            m_inputData[m_playerId].attackInfo.actionTime = actTime / 1000.0f;

            m_gameState = GameState.EndAction;


            Debug.Log("Own Action:" + m_inputData[m_playerId].attackInfo.actionKind.ToString() +
                      ",  Time:" + m_inputData[m_playerId].attackInfo.actionTime);
            Debug.Log("Opponent Action:" + m_inputData[m_playerId^1].attackInfo.actionKind.ToString() +
                      ",  Time:" + m_inputData[m_playerId^1].attackInfo.actionTime);
        }

    }


    //공격・방어 애니메이션 연출 대기.
	void UpdateEndAction()
	{
        GameObject obj = GameObject.Find("ActionController");
        if (obj == null) {
            //연출용 오브젝트가 없으면 생성합니다(이 시퀀스의 초기 동작입니다).
            obj = Instantiate(m_actionControllerPrefab) as GameObject;
            obj.name = "ActionController";

            //연출을 위해 승패 판정을 합니다.
            InputData serverPlayer = m_inputData[0];
            InputData clientPlayer = m_inputData[1];
            
            //가위바위보 판정.
            Winner rpsWinner = ResultChecker.GetRPSWinner(serverPlayer.rpsKind, clientPlayer.rpsKind);
            //공방 속도 판정.
            m_actionWinner = ResultChecker.GetActionWinner(serverPlayer.attackInfo, clientPlayer.attackInfo, rpsWinner);
            Debug.Log("RPS Winner:" + rpsWinner.ToString() + " ActionWinner" + m_actionWinner.ToString());

            //연출 시작.
            obj.GetComponent<ActionController>().Setup(
                m_actionWinner, m_score[0], m_score[1]
            );
            return;
        }

        //전투 연출이 끝나길 기다립니다.
        ActionController actionController = obj.GetComponent<ActionController>();
        if (actionController.IsEnd()) {
            //뒷처리.
            Destroy(GameObject.Find("BattleSelect"));       //공수선택 표시물을 지운다.
            Destroy(GameObject.Find("ActionController"));   //연출 제어를 제거.

            //m_timer = 0.0f;
            m_gameState = GameState.Result;
        }
	}

	void UpdateResult()
	{
        GameObject obj = GameObject.Find("ResultScene");
        if (obj == null) {
            //연출용 오브젝트가 없으면 생성합니다(이 시퀀스의 초기 동작입니다).
            obj = Instantiate(m_resultScenePrefab) as GameObject;
            obj.name = "ResultScene";

            //득점 판정 전에 이전 득점을 기억해 둡니다.
            int[] prevScores = { m_score[0], m_score[1] };
            // 이긴 쪽에 포인트가 들어갑니다.
            if (m_actionWinner == Winner.ServerPlayer) {
                ++m_score[0];
            }
            else if (m_actionWinner == Winner.ClientPlayer) {
                ++m_score[1];
            }
            
            //자신의 승패를 구합니다.
            ResultScene.Result ownResult = ResultScene.Result.Lose;
            if (m_actionWinner == Winner.Draw) {
                ownResult = ResultScene.Result.Draw;
            }
            else if (m_playerId == 0 && m_actionWinner == Winner.ServerPlayer) {
                ownResult = ResultScene.Result.Win;
            }
            else if (m_playerId == 1 && m_actionWinner == Winner.ClientPlayer) {
                ownResult = ResultScene.Result.Win;
            }

            //연출 시작.
            obj.GetComponent<ResultScene>().Setup(prevScores, m_score, ownResult);
            return;
        }

        //연출 대기.
        ResultScene resultScene = obj.GetComponent<ResultScene>();
        if (resultScene.IsEnd()) {
            Debug.Log("result end");
            if (m_score[0] == PLAY_MAX || m_score[1] == PLAY_MAX) {
                // 모든 대전이 끝난 경우는 게임 종료입니다.
                GameObject.Find("BGM").SendMessage("FadeOut"); //BGM.
                m_gameState = GameState.EndGame;
            }
            else {
                // 다음 대전으로 진행합니다.
                Reset();
                m_gameState = GameState.Ready;
            }

            //뒷처리.
            Destroy(obj);
        }
	}

	void UpdateEndGame()
	{
        GameObject obj = GameObject.Find("FinalResult");
        if (obj == null) {
            //승패에 따라 결과를 표시합니다.
            if (m_score[m_playerId] > m_score[m_playerId ^ 1]) {
                obj = Instantiate(m_finalResultWinPrefab) as GameObject;    //승리.
                obj.name = "FinalResult";
            }
            else {
                obj = Instantiate(m_finalResultLosePrefab) as GameObject;   //패배.
                obj.name = "FinalResult";
            }
        }
	}


	void Reset()
	{	
        //입력 초기화.
		for (int i = 0; i < m_inputData.Length; ++i) {
			m_inputData[i].rpsKind = RPSKind.None;
			m_inputData[i].attackInfo.actionKind = ActionKind.None;
            m_inputData[i].attackInfo.actionTime = 0.0f;
		}

        //캐릭터 상태 리셋.
        Destroy(m_serverPlayer);
        Destroy(m_clientPlayer);

        m_serverPlayer = Instantiate(m_serverPlayerPrefab) as GameObject;
        m_clientPlayer = Instantiate(m_clientPlayerPrefab) as GameObject;
        m_serverPlayer.name = "Daizuya";
        m_clientPlayer.name = "Toufuya";
        m_serverPlayer.GetComponent<Player>().ChangeAnimation(Player.Motion.Idle);
        m_clientPlayer.GetComponent<Player>().ChangeAnimation(Player.Motion.Idle);
        //플레이어 표기.
        GameObject board = GameObject.Find("BoardYou");
        board.GetComponent<BoardYou>().Run();
	}
	

	void OnGUIEndGame()
	{			
		// 종료 버튼을 표시합니다..
        GameObject obj = GameObject.Find("FinalResult");
        if (obj == null) { return; }

        Animation anim = obj.GetComponent<Animation>();
        if (anim.isPlaying) { return; }
        

        Rect r = new Rect(Screen.width / 2 - 50, Screen.height -60, 100, 50);
        if (GUI.Button(r, "RESET")) {
            //m_isPressedLeave = true;
            Application.LoadLevel("RockPaperScissors");
        }       
	}
	
	// 게임 종료 체크.
	public bool IsGameOver()
	{
		return m_isGameOver;
	}

	// 이벤트 발생 시 콜백 함수.
	public void EventCallback(NetEventState state)
	{
		switch (state.type) {
		case NetEventType.Disconnect:
			if (m_gameState < GameState.EndGame && m_isGameOver == false) {
				m_gameState = GameState.Disconnect;
			}
			break;
		}
	}

	// 연결 끊김 알림.
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
		if (GUI.Button (new Rect (px, py, sx, sy), message, style)) {
			// 게임 종료.
			//Reset();
			m_isGameOver = true;
            Application.LoadLevel("RockPaperScissors");
		}
	}
}
