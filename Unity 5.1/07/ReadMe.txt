
■키 입력 동기화 예제 프로그램 

●실행 파일
07\PingPong\bin\PingPong.exe 

●플레이 방법
타이틀 화면에서 '대전 상대 IP 주소' 텍스트 필드에 대전할  서로의  IP를 입력해주세요。
'대전 상대를 기다립니다'를 선택한 플레이어와 '대전 상대와 접속합니다'를 선택한 플레이어가
대전합니다.

마우스 왼쪽 버튼을 눌러 '초밥 패들'을 잡고 좌우로 이동시킵니다.
우 클릭하면 '접시 공'이 (ボール) 발사됩니다。

3 분동안 어떤 플레이어가 많이 초법을 획득했는지 다툽니다.


●예제 프로그램
프로젝트 파일：07\PingPong\Assets\PingPong.unity
프로그램：07\PingPong\Assets\Script

InputManager.cs		키 입력 제어
NetworkController.cs	키 입력 송수신, 키 입력 지연 제어
PingPong.cs		게임의 전체 시퀀스, 프레임 제어
TransportUDP.cs		UDP 소켓 통신 프로그램
