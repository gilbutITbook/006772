using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

public class SessionUDP : Session<TransportUDP>
{
	// 각 세션별 노드를 연관 짓기.
	private Dictionary<string, int> 	m_nodeAddress = new Dictionary<string, int>();


	public SessionUDP()
	{
		// UDP 시작 인덱스.
		m_nodeIndex = 10000;
	}

	// 
	public override bool CreateListener(int port, int connectionMax)
	{
		try {
			m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			m_listener.Bind(new IPEndPoint(IPAddress.Any, port));

			string str = "Create UDP Listener " + "(Port:" + port + ")"; 
			Debug.Log(str);
		}
		catch {
			return false;
		}
		
		
		return true;
	}
	
	//
	public override bool DestroyListener()
	{
		if (m_listener == null) {
			return false;
		}

		m_listener.Close();
		m_listener = null;
		
		return true;
	}	
	
	public override void AcceptClient() 
	{
	}

	//
	protected override void DispatchReceive()
	{
		// 리스닝 소켓으로 일괄수신한 데이터를 각 트랜스포트에 배분한다.
		if (m_listener != null && m_listener.Poll(0, SelectMode.SelectRead)) {
			byte[] buffer = new byte[m_mtu];
			IPEndPoint address = new IPEndPoint(IPAddress.Any, 0);
			EndPoint endPoint =(EndPoint) address;
			
			int recvSize = m_listener.ReceiveFrom(buffer, SocketFlags.None, ref endPoint);

			if (recvSize == 0) {
				// 접속 종료 요청.

			}
			else if (recvSize < 0) {
				// 수신 오류.

			}

			IPEndPoint iep = (IPEndPoint) endPoint;
			string nodeAddr = iep.Address.ToString() + ":" + iep.Port;

			int node = -1;
			// 같은 단말에서 실행할 때 포트 번호로 송신원을 판별하기 위해 킵 얼라이브 패킷에서.
			// IP주소와 포트번호를 추출한다.
			string str = System.Text.Encoding.UTF8.GetString(buffer).Trim('\0');
			if (str.Contains(TransportUDP.m_requestData)) {
				string[] strArray = str.Split(':');
				IPEndPoint ep = new IPEndPoint(IPAddress.Parse(strArray[0]), int.Parse(strArray[1]));
				//Debug.Log("RecvMsg:" + str);

				// 수신 어드레스와 노드 번호를 연결한다.
				if (m_nodeAddress.ContainsKey(nodeAddr)) {
					node = m_nodeAddress[nodeAddr];
				}
				else {
					Debug.Log("Not contain key:" + nodeAddr);

					node = getNodeFromEndPoint(ep);
					if (node >= 0) {
						m_nodeAddress.Add(nodeAddr, node);
					}
				}
			}
			else {

				if (m_nodeAddress.ContainsKey(nodeAddr)) {
					node = m_nodeAddress[nodeAddr];
				}
			}

			//Debug.Log("remote:" + ((IPEndPoint) endPoint).Address.ToString() + " port:" + ((IPEndPoint) endPoint).Port);
			//Debug.Log("remote node:" + node); 

			if (node >= 0) {
				//Debug.Log("[UDP]Recv data from node:" + node + "[address:" +  ((IPEndPoint) endPoint).Address.ToString() + " port:" + ((IPEndPoint) endPoint).Port + "]");
				TransportUDP transport = m_transports[node];
				transport.SetReceiveData(buffer, recvSize, (IPEndPoint) endPoint);
			}
		}
	}

	
	// EndPoint에서 노드 번호 가져옴.
	private int getNodeFromEndPoint(IPEndPoint endPoint)
	{
		foreach (int node in m_transports.Keys) {
			TransportUDP transport = m_transports[node];

			IPEndPoint transportEp = transport.GetRemoteEndPoint();
			if (transportEp != null) {
				Debug.Log("NodeFromEP recv[node:" + node + "] " + ((IPEndPoint) endPoint).Address.ToString() + ":" + endPoint.Port + " transport:" + transportEp.Address.ToString() + ":" + transportEp.Port);
				if (
					transportEp.Port == endPoint.Port &&
					transportEp.Address.ToString() == endPoint.Address.ToString()
				    ) {
					return node;
				}
			}
		}
		
		return -1;
	}
}

