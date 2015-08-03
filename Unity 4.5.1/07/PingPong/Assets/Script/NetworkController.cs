//#define EMURATE_INPUT //디버그 중 입력.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NetworkController{
    System.IO.StreamWriter m_debugWriterSyncData = null;
    void DebugWriterSetup() {
#if false
        string filename = Application.dataPath + "/SyncData.log";
        m_debugWriterSyncData = new System.IO.StreamWriter(filename);
        m_debugWriterSyncData.WriteLine("SyncDataLog");
#endif
    }
    ~NetworkController() {
#if false
        m_debugWriterSyncData.WriteLine("end");
        m_debugWriterSyncData.Close();
#endif
    }

    TransportUDP m_transport;       // 자주사용하므로.
    InputManager m_inputManager;    // 만들어 둔다.

    //서버 클라이언트 판정용.
    public enum HostType {
        Server,
        Client,
    }

    HostType m_hostType;

	private static int 			playerNum = 2;
	private const int			bufferNum = 4;  //버퍼의 크기 - 몇 개 분의 입력값을 송신할 것인가.	
	
	private List<MouseData>[]	inputBuffer = new List<MouseData>[playerNum];
	private MouseData[]			mouseData = new MouseData[playerNum];
	
	//private int					loopCounter = 0;

	public enum SyncState {
		NotStarted = 0,			// 키 데이터를 송수신하지 않는다.
		WaitSynchronize,		// 키 데이터를 송신 또는 수신한다.
		Synchronized,			// 동기화된 상태.
	}

    private int                 sendFrame = -1;
	private int					recvFrame = -1;

	private bool				isSynchronized = false;

	// 상태 관리 변수.
	private SyncState			syncState = SyncState.NotStarted;

	// 접속 상태.
	private bool				isConnected = false;

	// 절단용 플래그.
	private int 				suspendSync = 0;

	// 통신 환경이 나쁠 때를 위한 중복 데이터 재송신 카운터.
	private int					noSyncCount = 0;

	// 게임 종료 시 끊어질 때까지의 유예기간.
	private int					disconnectCount = 0;

	// 접속 확인용 더미 패킷 데이터.
	private const string 		requestData = "Request Connection.";
	

    //생성자.
    public NetworkController(string hostAddress, bool isHost) {
        DebugWriterSetup();

        isSynchronized = false;
		m_hostType = isHost? HostType.Server : HostType.Client;

        GameObject nObj = GameObject.Find("Network");
        m_transport = nObj.GetComponent<TransportUDP>();
		// 동일 단말에서 실행할 수 있게 포트 번호를 변경합니다.
		// 다른 단말에서 실행할 경우 포트 번호가 같은 것을 사용합니다.
		int listeningPort = isHost? NetConfig.GAME_PORT : NetConfig.GAME_PORT + 1;
		m_transport.StartServer(listeningPort);
		// 동일 단말에서 실행할 수 있게 포트 번호를 변경합니다.
		// 다른 단말에서 실행할 경우 포트 번호가 같은 것을 사용합니다.
		int remotePort = isHost? NetConfig.GAME_PORT + 1 : NetConfig.GAME_PORT;
		m_transport.Connect(hostAddress, remotePort);

		m_transport.RegisterEventHandler(OnEventHandling);

        GameObject iObj = GameObject.Find("InputManager");
        m_inputManager = iObj.GetComponent<InputManager>();

        for (int i = 0; i < inputBuffer.Length; ++i) {
            inputBuffer[i] = new List<MouseData>();
        }
    }


    //네트워크의 상태를 가져오기.
    public bool IsConnected()
	{
#if EMURATE_INPUT
        return true;    //디버그 중엔 접속한 거로 위장합니다.
#endif

		bool netConnected = m_transport.IsConnected();

        return (isConnected && netConnected);
    }

	public SyncState GetSyncState()
	{
		return syncState;
	}

	public bool IsSuspned()
	{
		return (suspendSync == 0x03);
	}

    public HostType GetHostType()
	{
        return m_hostType;
    }
    
	public void SuspendSync()
	{
		if (suspendSync > 0) {
			return;
		}

		// bit1: 접속 종료 응답, bit0:접속 종료 요구.
		suspendSync = 0x01;
		Debug.Log("SuspendSync requested.");
	}

    public bool IsSync(){

		//bool isConnected = m_transport.IsConnected();
		bool isSuspended = ((suspendSync & 0x02) == 0x02);
		bool frameSync = (syncState == SyncState.Synchronized && isSynchronized);
		//bool frameSync = (isSynchronized);

		//Debug.Log("Sync:" + frameSync + ":" + ":" + !isConnected + ":" + isSuspended);
		return (frameSync || !isConnected || isSuspended);
    }

    public void ClearSync()
	{
        isSynchronized = false;
    }


    //송수신해서 동기합니다.
    public bool UpdateSync()
	{
		if (IsConnected() == false && syncState == SyncState.NotStarted) {
			// 접속될 때까지 상대에게 접속 요구를 합니다.
			// TransportUDP.AcceptClient 함수에서 처음으로 패킷을 수신했을 때
			// 접속 플래그가 켜지므로 더미 패킷을 던집니다.
			byte[] request = System.Text.Encoding.UTF8.GetBytes(requestData);
			m_transport.Send(request, request.Length);
			return false;
		}

		// 키버퍼에 현재 프레임의 키 정보를 추가합니다. 
		bool update = EnqueueMouseData();
		
		// 송신.
		if (update) {
			SendInputData();
		}
		
		// 수신.
		ReceiveInputData();

		// 키 버퍼 선두의 키 입력 정보를 반영합니다.
        if (IsSync() == false) {    //동기화된 채라면 아무것도 하지 않습니다.
            DequeueMouseData();
        }
		
#if EMURATE_INPUT
        EmurateInput(); //디버그 중엔 입력을 위장합니다.
#endif

		return IsSync();
    }

    // 송신.
    void SendInputData()
	{
		PlayerInfo info = PlayerInfo.GetInstance();
		int playerId = info.GetPlayerId();
		int count = inputBuffer[playerId].Count;
		
		InputData inputData = new InputData();
		inputData.count = count;
		inputData.flag = suspendSync;
		inputData.datum = new MouseData[bufferNum];

		//string str1 = "Count :" + count;
		//Debug.Log(str1);
		for (int i = 0; i < count; ++i) {
			inputData.datum[i] = inputBuffer[playerId][i];

			//str1 = "Send mouse data => id:" + i + " - " + inputBuffer[playerId][i].frame;
			//Debug.Log(str1);
		}
		
		// 구조체를 byte 배열로 변환합니다.
		InputSerializer serializer = new InputSerializer();
		bool ret = serializer.Serialize(inputData);
		if (ret) {
			byte[] data = serializer.GetSerializedData();
			
			// 데이터를 송신합니다.
			m_transport.Send(data, data.Length);
		}

		// 상태를 갱신.
		if (syncState == SyncState.NotStarted) {
			syncState = SyncState.WaitSynchronize;
		}
    }

    // 수신.
    public void ReceiveInputData()
	{
		byte[] data = new byte[1400];
		
		// 데이터를 송신합니다.
		int recvSize = m_transport.Receive(ref data, data.Length);
		if (recvSize < 0) {
			// 입력정보를 수신하지 않았으므로 다음 프레임을 처리할 수 없습니다.
			return;
		} 

		string str = System.Text.Encoding.UTF8.GetString(data);
		if (requestData.CompareTo(str.Trim('\0')) == 0) {
			// 접속 요청 패킷 .
			return;
		}

		// byte 배열을 구조체로 변환합니다.
		InputData inputData = new InputData();
		InputSerializer serializer = new InputSerializer();
		serializer.Deserialize(data, ref inputData);
		
		// 수신한 입력 정보를 설정합니다.
		PlayerInfo info = PlayerInfo.GetInstance();
		int playerId = info.GetPlayerId();
		int opponent = (playerId == 0)? 1 : 0;
		
		for (int i = 0; i < inputData.count; ++i) {
			int frame = inputData.datum[i].frame;
			//Debug.Log("Set inputdata"+i+"[" + recvFrame + "][" + frame + "]:" + (recvFrame + 1 == frame));
			if (recvFrame + 1 == frame) {
				inputBuffer[opponent].Add(inputData.datum[i]);
				++recvFrame;
			}
		}
        //Debug.Log("recvFrame:" + recvFrame);

		// 접속 종료 플래그를 감시.  bit1:접속 종료 응답, bit0: 접속 종료 요구.
		if ((inputData.flag & 0x03) == 0x03) {
			// 접속 종료 플래그 수신.
			suspendSync = 0x03;
			Debug.Log("Receive SuspendSync.");
		}

		if ((inputData.flag & 1) > 0 && (suspendSync & 1) > 0) {
			suspendSync |= 0x02;
			Debug.Log("Receive SuspendSync." + inputData.flag);
		}

		if (isConnected && suspendSync == 0x03) {
			// 서로 접속 종료 상태가 되었으므로 상대에게의 접속 종료 플래그를 보내기위한.
			// 유예기간을 두고 조금 후에 접속종료합니다.
			++disconnectCount;
			if (disconnectCount > 10) {
				m_transport.Disconnect();
				Debug.Log("Disconnect because of suspendSync.");
			}
		}
		

		// 상태 갱신.
		if (syncState == SyncState.NotStarted) {
			syncState = SyncState.WaitSynchronize;
		}

		//loopCounter = 0;
	}


    //키 버퍼에 추가.(입력지연 이상의 정보는 무시하고 false를 반환).
	public bool EnqueueMouseData()
	{
		PlayerInfo info = PlayerInfo.GetInstance();
		int playerId = info.GetPlayerId();

		if (inputBuffer[playerId].Count >= bufferNum) {
				// 입력지연 이상의 정보는 받지않습니다. 
			++noSyncCount;
			if (noSyncCount >= bufferNum) {
				//Debug.Log("Resend inputbuffer data.");
				noSyncCount = 0;
				return true;
			}

			return false;
		}
		
		// 키 입력을 가져와서 키 버퍼에 추가.
        sendFrame++;
		//MouseData mouseData = m_inputManager.GetMouseData(playerId);
        MouseData mouseData = m_inputManager.GetLocalMouseData();
        mouseData.frame = sendFrame;
		inputBuffer[playerId].Add(mouseData);        
		//Debug.Log("Set mouse data[" + sendFrame +"]");

		return true;
	}

    //동기화된 입력값을 꺼냅니다.
	public void DequeueMouseData()
	{
		//Debug.Log("DequeueMouseData");
		// 양 단말의 데이터가 모였는지 체크합니다.
		for (int i = 0; i < playerNum; ++i) {
			if (inputBuffer[i].Count == 0) {
				return;     //입력값이 없을 때는 아무것도 하지 않습니다.
			}
		}
		
		// 데이터가 모였으므로.
		for (int i = 0; i < playerNum; ++i) {
			mouseData[i] = inputBuffer[i][0];
			inputBuffer[i].RemoveAt(0);

            //입력 관리자에게 동기된 데이터로서 전달합니다.
            m_inputManager.SetInputData(i, mouseData[i]);

#if false
            m_debugWriterSyncData.WriteLine(mouseData[i]);
#endif
		}
#if false
        m_debugWriterSyncData.Flush();
#endif

		//Debug.Log("DequeueMouseData:isSynchronized");

		// 상태 갱신.
		if (syncState != SyncState.Synchronized) {
			syncState = SyncState.Synchronized;
		}

		isSynchronized = true;
	}

	public void OnEventHandling(NetEventState state)
	{
		switch (state.type) {
		case NetEventType.Connect:
			isConnected = true;
			Debug.Log("[NetworkController] Connected.");
			break;
			
		case NetEventType.Disconnect:
			isConnected = false;
			Debug.Log("[NetworkController] Disconnected.");
			break;
		}
	}

    //debug code.
    void EmurateInput() {
        PlayerInfo info = PlayerInfo.GetInstance();
        int playerId = info.GetPlayerId();
        MouseData inputData = m_inputManager.GetLocalMouseData(); //m_inputManager.GetMouseData(playerId);
        
        //동기화된 입력값 위장(자신의 입력을 상대의 입력으로서 준다).
        int opponent = (playerId == 0) ? 1 : 0;
        m_inputManager.SetInputData(playerId, inputData);
        m_inputManager.SetInputData(opponent, inputData);

        // = SyncFlag.Synchronized;
        isSynchronized = true;
    }

}
