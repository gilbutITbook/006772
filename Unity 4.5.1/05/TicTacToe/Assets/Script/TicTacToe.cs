using UnityEngine;
using System;
using System.Collections;

public class TicTacToe : MonoBehaviour {
	
	// 게임 진행 상황.
	private enum GameProgress {
		None = 0,		// 시합 시작 전.
		Ready,			// 시합 시작 신호 표시.
		Turn,			// 시함 중.
		Result,			// 결과 표시.
		GameOver,		// 게임 종료.
		Disconnect,		// 연결 끊기.
	};
	
	// 턴 종류.
	private enum Turn {
		Own = 0,		// 자산의 턴.
		Opponent,		// 상대의 턴.
	};

	// 마크.
	private enum Mark {
		Circle = 0,		// ○.
		Cross,			// ×.
	};
	
	// 시합 결과.
	private enum Winner {
		None = 0,		// 시합 중.
		Circle,			// ○승리.
		Cross,			// ×승리.
		Tie,			// 무승부.
	};
	
	// 칸의 수.
	private const int 		rowNum = 3;

	// 시합 시작 전의 신호표시 시간.
	private const float		waitTime = 1.0f;

	// 대기 시간.
	private const float		turnTime = 10.0f;
	
	// 배치된 기호를 보존.
	private int[]			spaces = new int[rowNum*rowNum];
	
	// 진행 상황.
	private	GameProgress	progress;
	
	// 현재의 턴.
	private Mark			turn;

	// 로컬 기호.
	private Mark			localMark;

	// 리모트 기호.
	private Mark			remoteMark;

	// 남은 시간.
	private float			timer;

	// 승자.
	private Winner			winner;
	
	// 게임 종료 플래그.
	private bool			isGameOver;

	// 대기 시간.
	private float			currentTime;
	
	// 네트워크.
	private TransportTCP 	m_transport = null;

	// 카운터.
	private float			step_count = 0.0f;

	//
	// 텍스처 관련.
	//

	// 동그라미 텍스처.
	public GUITexture		circleTexture;
	
	// .
	public GUITexture		crossTexture;
	
	// .
	public GUITexture		fieldTexture;

	public GUITexture		youTexture;

	public GUITexture		winTexture;

	public GUITexture		loseTexture;

    // 사운드.
    public AudioClip se_click;
    public AudioClip se_setMark;
    public AudioClip se_win;

	private static float SPACES_WIDTH = 400.0f;
	private static float SPACES_HEIGHT = 400.0f;

	private static float WINDOW_WIDTH = 640.0f;
	private static float WINDOW_HEIGHT = 480.0f;

	// Use this for initialization
	void Start () {
		
		// Network 클래스의 컴포넌트 가져오기.
		GameObject obj = GameObject.Find("Network");
		m_transport  = obj.GetComponent<TransportTCP>();
		if (m_transport != null) {
			m_transport.RegisterEventHandler(EventCallback);
		}

		// 게임을 초기화합니다.
		Reset();
		isGameOver = false;
		timer = turnTime;
	}
	
	// Update is called once per frame
	void Update()
	{

 		switch (progress) {
		case GameProgress.Ready:
			UpdateReady();
			break;

		case GameProgress.Turn:
			UpdateTurn();
			break;
			
		case GameProgress.GameOver:
			UpdateGameOver();
			break;			
		}
	}
	
	// 
	void OnGUI()
	{
		switch (progress) {
		case GameProgress.Ready:
			// 필드와 기호를 그립니다.
			DrawFieldAndMarks();
			break;

		case GameProgress.Turn:
			// 필드와 기호를 그립니다.
			DrawFieldAndMarks();
			// 남은 시간을 그립니다.
			if (turn == localMark) {
				DrawTime();
			}
			break;
			
		case GameProgress.Result:
			// 필드와 기호를 그립니다.
			DrawFieldAndMarks();
			// 승자를 표시합니다.
			DrawWinner();
			// 종료 버튼을 표시합니다.
			{
				GUISkin skin = GUI.skin;
				GUIStyle style = new GUIStyle(GUI.skin.GetStyle("button"));
				style.normal.textColor = Color.white;
				style.fontSize = 25;

				if (GUI.Button(new Rect(Screen.width/2-100, Screen.height/2, 200, 100), "끝", style)) {
					progress = GameProgress.GameOver;
					step_count = 0.0f;
				}
			}
			break;

		case GameProgress.GameOver:
			// 필드와 기호를 그립니다.
			DrawFieldAndMarks();
			// 승자를 표시합니다.
			DrawWinner();
			break;

		case GameProgress.Disconnect:
			// 필드와 기호를 그립니다.
			DrawFieldAndMarks();
			// 연결 끊김을 통지합니다.
			NotifyDisconnection();
			break;

		default:
			break;
		}

	}

	void UpdateReady()
	{
		// 시합 시작 신호 표시를 기다립니다.
		currentTime += Time.deltaTime;

		if (currentTime > waitTime) {
            //BGM 재생 시작.
            GameObject bgm = GameObject.Find("BGM");
            bgm.GetComponent<AudioSource>().Play();

			// 표시가 끝나면 게임 시작입니다.
			progress = GameProgress.Turn;
		}
	}

	void UpdateTurn()
	{
		bool setMark = false;

		if (turn == localMark) {
			setMark = DoOwnTurn();

            //둘 수 없는 장소를 누르면 클릭용 사운드효과를 냅니다.
            if (setMark == false && Input.GetMouseButtonDown(0)) {
                AudioSource audio = GetComponent<AudioSource>();
                audio.clip = se_click;
                audio.Play();
            }
		}
		else {
			setMark = DoOppnentTurn();

            //둘 수 없을 때 누르면 클릭용 사운드 효과를 냅니다.
            if (Input.GetMouseButtonDown(0)) {
                AudioSource audio = GetComponent<AudioSource>();
                audio.clip = se_click;
                audio.Play();
            }
		}

		if (setMark == false) {
			// 놓을 곳을 검토 중입니다.	
			return;
		}
        else {
            //기호가 놓이는 사운드 효과를 냅니다. 
            AudioSource audio = GetComponent<AudioSource>();
            audio.clip = se_setMark;
            audio.Play();
        }
		
		// 기호의 나열을 체크합니다.
		winner = CheckInPlacingMarks();
		if (winner != Winner.None) {
            //승리한 경우는 사운드효과를 냅니다.
            if ((winner == Winner.Circle && localMark == Mark.Circle)
                || (winner == Winner.Cross && localMark == Mark.Cross)) {
                AudioSource audio = GetComponent<AudioSource>();
                audio.clip = se_win;
                audio.Play();
            }
            //BGM재생종료.
            GameObject bgm = GameObject.Find("BGM");
            bgm.GetComponent<AudioSource>().Stop();

			// 게임 종료입니다.
			progress = GameProgress.Result;			
		}
		
		// 턴을 갱신합니다.
		turn = (turn == Mark.Circle)? Mark.Cross : Mark.Circle; 
		timer = turnTime;
	}
	
	// 게임 종료 처리
	void UpdateGameOver()
	{
		step_count += Time.deltaTime;
		if (step_count > 1.0f) {
			// 게임을 종료합니다.
			Reset();
			isGameOver = true;
		}
	}

	// 자신의 턴일 때의 처리.
	bool DoOwnTurn()
	{
		int index = 0;

		timer -= Time.deltaTime;
		if (timer <= 0.0f) {
			// 타임오버.
			timer = 0.0f;
			do {
				index = UnityEngine.Random.Range(0, 8);
			} while (spaces[index] != -1);
		}
		else {
			// 마우스의 왼쪽 버튼의 눌린 상태를 감시합니다.
			bool isClicked = Input.GetMouseButtonDown(0);
			if (isClicked == false) {
				// 눌려지지 않았으므로 아무것도 하지 않지 않습니다.
				return false;
			}
			
			Vector3 pos = Input.mousePosition;
			Debug.Log("POS:" + pos.x + ", " + pos.y + ", " + pos.z);
			
			// 수신한 정보를 바탕으로 선택된 칸으로 변환합니다.
			index = ConvertPositionToIndex(pos);
			if (index < 0) {
				// 범위 밖이 선택되었습니다.
				return false;
			}
		}

		// 칸에 둡니다.
		bool ret = SetMarkToSpace(index, localMark);
		if (ret == false) {
			// 둘 수 없습니다.
			return false;
		}

		// 선택한 칸의 정보를 송신합니다.
		byte[] buffer = new byte[1];
		buffer[0] = (byte)index;
		m_transport.Send (buffer, buffer.Length);

		return true;
	}
	
	// 상대의 턴일 때의 처리.
	bool DoOppnentTurn()
	{
		// 상대의 정보를 수신합니다.
		byte[] buffer = new byte[1];
		int recvSize = m_transport.Receive(ref buffer, buffer.Length);

		if (recvSize <= 0) {
			// 아직 수신되지 않았습니다.
			return false;			
		}

		// 서버라면 ○ 클라이언트라면 ×를 지정합니다.
		//Mark mark = (m_network.IsServer() == true)? Mark.Cross : Mark.Circle;

		// 수신한 정보를 선택된 칸으로 변환합니다. 
		int index = (int) buffer[0];

		Debug.Log("Recv:" + index + " [" + m_transport.IsServer() + "]");
	
		// 칸에 둡니다.
		//bool ret = SetMarkToSpace(index, mark);
		bool ret = SetMarkToSpace(index, remoteMark);
		if (ret == false) {
			// 둘 수 없다.
			return false;
		}
		
		return true;
	}
	
	// 
	int ConvertPositionToIndex(Vector3 pos)
	{
		float sx = SPACES_WIDTH;
		float sy = SPACES_HEIGHT;
		
		// 맆드 왼쪽 위 모퉁이를 기점으로 한 좌표계로 변환합니다.
		float left = ((float)Screen.width - sx) * 0.5f;
		float top = ((float)Screen.height + sy) * 0.5f;
		
		float px = pos.x - left;
		float py = top - pos.y;
		
		if (px < 0.0f || px > sx) {
			// 필드 밖입니다.
			return -1;	
		}
		
		if (py < 0.0f || py > sy) {
			// 필드 밖입니다.
			return -1;	
		}
	
		// 인덱스 번호로 변환합니다.
		float divide = (float)rowNum;
		int hIndex = (int)(px * divide / sx);
		int vIndex = (int)(py * divide / sy);
		
		int index = vIndex * rowNum  + hIndex;
		
		return index;
	}
	
	// 
	bool SetMarkToSpace(int index, Mark mark)
	{
		if (spaces[index] == -1) {
			// 미선택된 칸이므로 놓을 수 없습니다.
			spaces[index] = (int) mark;
			return true;
		}
		
		// 이미 놓여 있습니다.
		return false;
	}
	
	// 기호 배열 체크.
	Winner CheckInPlacingMarks()
	{
		string spaceString = "";
		for (int i = 0; i < spaces.Length; ++i) {
			spaceString += spaces[i] + "|";
			if (i % rowNum == rowNum - 1) {
				spaceString += "  ";	
			}
		}
		Debug.Log(spaceString);
		
		// 가로 방향을 체크합니다.
		for (int y = 0; y < rowNum; ++y) {
			int mark = spaces[y * rowNum];
			int num = 0;
			for (int x = 0; x < rowNum; ++x) {
				int index = y * rowNum + x;
				if (mark == spaces[index]) {
					++num;
				}
			}
			
			if (mark != -1 && num == rowNum) {
				// 기호가 다 모였으므로 승패결정.
				return (mark == 0)? Winner.Circle : Winner.Cross;
			}
		}
		
		// 세로 방향을 체크합니다.
		for (int x = 0; x < rowNum; ++x) {
			int mark = spaces[x];
			int num = 0;
			for (int y = 0; y < rowNum; ++y) {
				int index = y * rowNum + x;
				if (mark == spaces[index]) {
					++num;
				}
			}
					
			if (mark != -1 && num == rowNum) {
				// 기호가 다 모였으므로 승패 결정.
				return (mark == 0)? Winner.Circle : Winner.Cross;
			}
		}
		
		// 왼쪽 위에서부터 사선 방향을 체크합니다.
		{
			int mark = spaces[0];
			int num = 0;
			for (int xy = 0; xy < rowNum; ++xy) {
				int index = xy * rowNum + xy;
				if (mark == spaces[index]) {
					++num;
				}
			}
						
			if (mark != -1 && num == rowNum) {
				// マークがそろったので勝敗決定.
				return (mark == 0)? Winner.Circle : Winner.Cross;
			}	
		}

		// 왼쪽 위부터 사선 방향을 체크합니다.
		{
			int mark = spaces[rowNum - 1];
			int num = 0;
			for (int xy = 0; xy < rowNum; ++xy) {
				int index = xy * rowNum + rowNum - xy - 1;
				if (mark == spaces[index]) {
					++num;
				}
			}
						
			if (mark != -1 && num == rowNum) {
				// 기호가 다 모였으므로 승패 결정.
				return (mark == 0)? Winner.Circle : Winner.Cross;
			}	
		}
		
		// 비겼는지 체크합니다.
		{
			int num = 0;
			foreach (int space in spaces) {
				if (space == -1) {
					++num;	
				}
			}
			
			if (num == 0) {
				// 놓을 장소가 없고 승패가 나지 않으므로 비겼습니다.
				return Winner.Tie;
			}
		}
		
		return Winner.None;
	}

	// 게임 리셋.
	void Reset()
	{
		//turn = Turn.Own;
		turn = Mark.Circle;
		progress = GameProgress.None;
		
		// 미선택으로 하고 초기화합니다.
		for (int i = 0; i < spaces.Length; ++i) {
			spaces[i] = -1;	
		}
	}

	// 필드와 기호를 그립니다.
	void DrawFieldAndMarks()
	{
		float sx = SPACES_WIDTH;
		float sy = SPACES_HEIGHT;
		
		// 필드를 그립니다.
		Rect rect = new Rect(Screen.width / 2 - WINDOW_WIDTH * 0.5f, 
		                     Screen.height / 2 - WINDOW_HEIGHT * 0.5f, 
		                     WINDOW_WIDTH, 
		                     WINDOW_HEIGHT);
		Graphics.DrawTexture(rect, fieldTexture.texture);
		
		// 필드의 왼쪽 위 모퉁이를 기점으로 한 좌표계로 변환합니다. 
		float left = ((float)Screen.width - sx) * 0.5f;
		float top = ((float)Screen.height - sy) * 0.5f;

		// 기호를 그립니다. 
		for (int index = 0; index < spaces.Length; ++index) {
			if (spaces[index] != -1) {
				int x = index % rowNum;
				int y = index / rowNum;
				
				float divide = (float)rowNum;
				float px = left + x * sx / divide;
				float py = top + y * sy / divide;
				
				Texture texture = (spaces[index] == 0)? circleTexture.texture : crossTexture.texture;
				
				float ofs = sx / divide * 0.1f;
				
				Graphics.DrawTexture(new Rect(px+ofs, py+ofs, sx * 0.8f / divide, sy* 0.8f / divide), texture);
			}
		}

		// 순서 텍스처 표시.
		if (localMark == turn) {
			float offset = (localMark == Mark.Circle)? -94.0f : sx + 36.0f;
			rect = new Rect(left + offset, top + 5.0f, 68.0f, 136.0f);
			Graphics.DrawTexture(rect, youTexture.texture);
		}
	}

	// 남은 시간 표시.
	void DrawTime()
	{
		GUIStyle style = new GUIStyle();
		style.fontSize = 35;
		style.fontStyle = FontStyle.Bold;
		
		string str = "Time : " + timer.ToString("F3");
		
		style.normal.textColor = (timer > 5.0f)? Color.black : Color.white;
		GUI.Label(new Rect(222, 5, 200, 100), str, style);
		
		style.normal.textColor = (timer > 5.0f)? Color.white : Color.red;
		GUI.Label(new Rect(220, 3, 200, 100), str, style);
	}

	// 결과 표시.
	void DrawWinner()
	{
		float sx = SPACES_WIDTH;
		float sy = SPACES_HEIGHT;
		float left = ((float)Screen.width - sx) * 0.5f;
		float top = ((float)Screen.height - sy) * 0.5f;

		// 순서 텍스처 표시.
		float offset = (localMark == Mark.Circle)? -94.0f : sx + 36.0f;
		Rect rect = new Rect(left + offset, top + 5.0f, 68.0f, 136.0f);
		Graphics.DrawTexture(rect, youTexture.texture);

		// 결과 표시.
		rect.y += 140.0f;

		if (localMark == Mark.Circle && winner == Winner.Circle ||
		    localMark == Mark.Cross && winner == Winner.Cross) {
			Graphics.DrawTexture(rect, winTexture.texture);
		}
			
		if (localMark == Mark.Circle && winner == Winner.Cross ||
		    localMark == Mark.Cross && winner == Winner.Circle) {
			Graphics.DrawTexture(rect, loseTexture.texture);
		}	
	}

	// 끊김 통지.
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

		string message = "회선이 끊겼습니다.\n\n버튼을 누르세요.";
		if (GUI.Button (new Rect (px, py, sx, sy), message, style)) {
			// 게임이 종료됐습니다.
			Reset();
			isGameOver = true;
		}
	}

	// 게임 시작.
	public void GameStart()
	{
		// 게임 시작 상태로 합니다.
		progress = GameProgress.Ready;

		// 서버가 먼저 하게 설정합니다.
		turn = Mark.Circle;

		// 자신과 상대의 기호를 설정합니다.
		if (m_transport.IsServer() == true) {
			localMark = Mark.Circle;
			remoteMark = Mark.Cross;
		}
		else {
			localMark = Mark.Cross;
			remoteMark = Mark.Circle;
		}

		// 이전 설정을 클리어합니다.
		isGameOver = false;
	}
	
	// 게임 종료 체크.
	public bool IsGameOver()
	{
		return isGameOver;
	}

	// 이벤트 발생 시의 콜백 함수.
	public void EventCallback(NetEventState state)
	{
		switch (state.type) {
		case NetEventType.Disconnect:
			if (progress < GameProgress.Result && isGameOver == false) {
				progress = GameProgress.Disconnect;
			}
			break;
		}
	}
}
