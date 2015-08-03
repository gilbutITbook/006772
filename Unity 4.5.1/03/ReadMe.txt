
■ 통신 샘플 프로그램 

★처음 만드는 통신 프로그램 
●実行ファイル
03\SocketSample\bin\SocketSample.exe		TCP 샘플 프로그램
03\SocketSample\bin\SocketSample*.exe		UDP 샘플 프로그램

●실행 방법
Unity3D로 프로젝트 파일을 시작해서 프로그램을 실행합니다. 이 때 Console을 표시해 주십시오.
Unity3D 에서는 서버 측에서 「Launch server」를 선택해 주십시오.
클라이언트 측은 실행 파일을 시작합니다.  서버 시작 후, 「Connect to server」를 눌러주세요. 
클라이언트의 창을 닫으면  Console에 "Hello, this is client.”라고 표시됩니다. 
메시지가 표시되면 통신이 성공한 것입니다. 


●샘플 프로그램 
프로젝트 파일: 03\SocketSample\Assets\Scene\SocketSample.unity
프록램: 03\SocketSample\Assets\Script

SocketSampleTCP.cs		TCP 소켓의  샘플 프로그램 
SocketSampleUDP.cs		UDP 소켓의 샘플 프로그램 


★통신 라이브러리 
●예제 프로그램
프로젝트 파일: 03\NetworkLibrary\Assets\Scene\NetworkLibrary.unity
프로그램: 03\NetworkLibrary\Assets\Script

LibrarySample.cs		라이브러리 동작 확인 프로그램 
TransportTCP.cs			TCP 통신을  하는 통신 모듈 
TransportUDP.cs			UDP 통신을 하는 통신 모듈 
PacketQueue.cs			패킷 데이터를 스레드 간에 공유하기 위한 버퍼 
NetworkDef.cs			통신 이벤트 관련 정의 
			
