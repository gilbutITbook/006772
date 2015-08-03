
■ 매칭서버 예제 프로그램

● 실행 파일
089\MatchingServer\bin\MatchingServer.exe

● 사용법
실행 파일을 시작하면, 매칭 서버가 대기를 시작합니다. 
본 게임을 시작하기 전에 실행해 둘 필요가 있습니다.
방은 최대 네 개를 만들 수 있습니다.
시작 후 화면에 매칭 서버의 IP 주소와 대기하는 포트 번호, 현재 접속수가 표시됩니다.
매칭 클라이언트로부터 접속되어 방이 만들어지면, 만들어진 방의 정보가 표시됩니다.
게임을 시작한 세션은 일정 시간이 지나면 방 정보가 삭제됩니다.


● 예제 프로그램
프로젝트 파일：09\MatchingServer\Assets\Scenes\MatchingServer.unity
프로그램：09\MatchingServer\Assets\Script


● 통신 관련 파일 구성
IPacket.cs		패킷 인터페이스
MatchingServer.cs	매칭 서버
Network.cs		통신 모듈(TransportTCP,TransportUDP 클래스 제어)
NetworkDef.cs		통신에 관한 정의
Packet.cs		패킷 클래스 정의
PacketQueue.cs		패킷 큐 클래스
PacketSerializer.cs	패킷 시리얼라이저(패킷 헤더의 시리얼라이저)
PacketStructs.cs	패킷 데이터 정의
Serializer.cs		시리얼라이저 기저 클래스
Session.cs		세션 관리 기저 클래스
SessionUDP.cs		TCP용 세션 관리 클래스
SessionTCP.cs		UDP용 세션 관리 클래스
TransportTcp.cs		TCP 소켓통신 프로그램
TransportUDP.cs		UDP 소켓통신 프로그램
