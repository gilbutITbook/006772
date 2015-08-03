//#define EMURATE_INPUT // 디버그 중 입력은 이걸로.

using UnityEngine;
using System;
using System.Collections;
using System.Net;

public class NetworkController {
    const int USE_PORT = 50765;
	TransportTCP m_network;         //자주 사용하므로 만들어 둔다.

    //서버 클라이언트 판정용.
    public enum HostType {
        Server,
        Client,
    };
    HostType m_hostType;


    //서버에서 사용할 때.
    public NetworkController() {
        m_hostType = HostType.Server;

        GameObject nObj = GameObject.Find("Network");
        m_network = nObj.GetComponent<TransportTCP>();
        m_network.StartServer(USE_PORT, 1);
    }

    //클라이언트에서 사용할 때.
    public NetworkController(string serverAddress) {
        m_hostType = HostType.Client;

        GameObject nObj = GameObject.Find("Network");
		m_network = nObj.GetComponent<TransportTCP>();
        m_network.Connect(serverAddress, USE_PORT);
    }


    //네트워크 상태 획득.
    public bool IsConnected() {
#if EMURATE_INPUT
        return true;    // 디버그 중엔 접속한 거로 위장합니다.
#endif

        return m_network.IsConnected();
    }
    public HostType GetHostType() {
        return m_hostType;
    }
// 가위바위보 송신 함수.
	public void SendRPSData(RPSKind rpsKind)
	{
		// 구조체를 byte배열로 변환합니다.
		byte[] data = new byte[1];
		data[0] = (byte) rpsKind;
		
		// 데이터를 송신합니다.
		m_network.Send(data, data.Length);
	}
	
	// 가위바위보 수신 함수 
	public RPSKind ReceiveRPSData()
	{
#if EMURATE_INPUT
		return RPSKind.Rock;;    //디버그 중엔 접속한 거로 위장합니다.
#endif
		
		byte[] data = new byte[1024];
		
		// 데이터를 수신합니다.
		int recvSize = m_network.Receive(ref data, data.Length);
		if (recvSize < 0) {
			// 입력 정보를 수신하지 않음.
			return RPSKind.None;
		}
		
		// byte 배열을 구조체로 변환합니다.
		RPSKind rps = (RPSKind) data[0];
		
		return rps;
	}
	
	// 액션 송신.
	public void SendActionData(ActionKind actionKind, float actionTime)
	{
		// 구조체를 byte 배열로 변환합니다. 
		byte[] data = new byte[3];
		data[0] = (byte) actionKind;
		
		// 정수화합니다.
		short actTime = (short)(actionTime * 1000.0f);
		// 네트워크 바이트오더로 변환합니다.
		short netOrder = IPAddress.HostToNetworkOrder(actTime);
		// byte[] 형으로 변환합니다. 
		byte[] conv = BitConverter.GetBytes(netOrder);
		data[1] = conv[0];
		data[2] = conv[1];
		
		// 데이터를 송신합니다.
		m_network.Send(data, data.Length);
	}
	
	// 액션 수신.
	public bool ReceiveActionData(ref ActionKind actionKind, ref float actionTime)
	{	
		byte[] data = new byte[1024];
		
		// 데이터를 수신합니다.
		int recvSize = m_network.Receive(ref data, data.Length);
		if (recvSize < 0) {
			// 입력 정보를 수신하지 않았습니다.
			return false;
		}
		
		// byte배열을 구조체로 변환합니다.
		actionKind = (ActionKind) data[0];
		// byte[]형에서 short형으로 변환합니다.
		short netOrder = (short) BitConverter.ToUInt16(data, 1);
		// 호스트 바이트 오더로 변환합니다..
		short hostOrder = IPAddress.NetworkToHostOrder(netOrder);
		// float 단위 시간으로 되돌립니다.
		actionTime = hostOrder / 1000.0f;
		
		return true;
	}
}
