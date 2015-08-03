
■ 액션 가위바위보 예제 프로그램 

● 실행 파일 
06\RockPaperScissors\bin\rps.exe

● 놀이 방법 
타이틀 화면에서 대전 상대를 기다릴 플레이어는 ' 대전 상대를 기다립니다'를 누르세요.
대기 중인 플레이어와 대전할 플레이어는 '상대방 IP 주소' 텍스트 필드에 대기 중인 
플레이어의 IP 주소를 입력하고 '대전 상대와 접속합니다'를 누르세요.


처음에 가위바위보에 낼 손을 마우스로 클릭해서 선택합니다.
가위바위보를 하고 나면 액션을 선택하는 모드로 전환되므로 재빨리
'攻' 또는 '守' 중 하나의 버튼을 클릭해 주세요.

3포인트 먼저 득점하는 쪽이 승리합니다.


● 예제 프로그램
프로젝트 파일：06\RockPaperScissors\RockPaperScissors.unity
프로그램：06\RockPaperScissors\Assets\Script

InputData.cs			입력정보 정의
NetworkController.cs		선택한 가위바위보 액션 송수신
RockPaperScissors.cs		게임의 시퀀스 제어
TransportTCP.cs			TCP 소켓 통신 프로그램

