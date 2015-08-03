
■ 모형정원 커뮤니케이션 게임 예제 프로그램

● 실행 파일
08\toufu_no_tumori\bin\toufu_no_tumori.exe

● 플레이 방법
타이틀 화면에서 '친구의 주소' 텍스트 필드에 함께 플레이할 친구의 IP 주소를 서로 입력합니다.
먼저 정원에 가서 친구를 기다릴 경우는 '친구를 기다린다' 버튼을 눌러서 게임을 시작합니다.
먼저 플레이를 시작한 친구와 함께 게임을 하고 싶을 때는 '놀러 간다' 버튼을 눌러주세요.
친구를 기다릴 플레이어는 '두부장수'로 플레이합니다.  
친구의 정원에 놀러가는 플레이어는 '콩장수'로 플레이합니다. 
화면 위를 마우스 왼쪽 버튼을 클릭 또는 드래그 하면 캐릭터가 마우스 포인터 위치를 향해 이동합니다. 
지면에 놓인 아이템을 주울 수 있습니다. 아이템을 더블클릭하면 획득할 수 있습니다. 
아이템을 획득한 상태에서 다른 아이템을 획득하면 가지고 있던 아이템은 폐기되고 새로운 아이템을 획득합니다.
아이템은 친구 정원에 가지고 갈 수 있습니다. 친구에게 줄 수도 있습니다. 
단, 원래 정원에 없는 식물은 말라버리므로 누군가가 원래 정원에 돌려놔야만 합니다.
고양이를 획득한 상태에서 집을 클릭하면 캐릭터의 이동이 이사 모드로 바뀝니다.
한 번 더 집을 클릭하면 이사를 마칩니다. 
화면 오른쪽 위에 있는 텍스트 필드에 메시지를 입력하면 친구에게 메시지를 보낼 수 있습니다. 


● 예제 프로그램
프로젝트 파일：08\toufu_no_tumori\Assets\Scenes\TitleScene.unity
프로그램：08\toufu_no_tumori\Assets\Script


● 통신이 관련된 파일 구성
Character			<캐릭터 제어 스크립트>
    CharacterRoot.cs		캐릭터 매니저
    chrBehaviorBase.cs		캐리터 비헤이비어의 기저 클래스
    chrBehaviorLocal.cs		로컬 캐릭터의 행동
    chrBehaviorNet.cs		리모트 캐릭터의 행동
    chrBehaviorNPC_House.cs	집의 행동
    chrBehaviorPlayer.cs	캐릭터(두부장수, 콩장수)의 공통 행동
    chrController.cs		캐릭터 컨트롤러
    ItemCarrier.cs		아이테 운반 제어

Event				<이벤트 제어 스크립트>
    EventBoxLeave.cs		정원 이동 시 제어(로컬 캐릭터가 이동할 때)
    EventEnter.cs		정원 이동 시 제어(리모트 캐릭터가 놀러 올 때)
    EventLeave.cs		정원 이동 시 아이템 운반 제어

Item				<아이템 제어 스크립트>
    ItemBehaviorBase.cs		아이템 행동의 기저 클래스
    ItemBehaviorFruit.cs	파, 유자의 행동
    ItemController.cs		아이템 컨트롤러
    ItemManager.cs		아이템 매니저


Network				<통신제어 스크립트>
    GameServer.cs		게임 서버
    IPacket.cs			패킷 인터페이스
    Network.cs			통신모듈(TransportTCP,TransportUDP 클래스 제어)
    NetworkDef.cs		통신에 관한 정의
    Packet.cs			패킷 클래스 정의
    PacketQueue.cs		패킷 큐 클래스
    PacketSerializer.cs		패킷 시리얼라이저(패킷 헤더의 시리얼라이저)
    PacketStructs.cs		패킷 데이터 정의
    Serializer.cs		시리얼라이저 기저 클래스
    SplineData.cs		캐릭터 이동용 스플라인 보간 클래스
    TransportTcp.cs		TCP 소켓 통신 프로그램
    TransportUDP.cs		UDP 소켓 통신 프로그램

System				<게임 시스템 제어 스크립트>
    EventRoot.cs		이벤트 매니저
    GameRoot.cs			게임 제어
    GlobalParam.cs		씬을 넘나드는 정보 관리
    MapCreator.cs		맵 생성 제어
    QueryManager.cs		쿼리 제어
    TitleControl.cs		게임 서버, 호스트-게스트간 접속 시퀀스 제어


● 통신 프로그램 보충
▼ 캐릭터 이동
　・로컬 캐릭터의 좌표 송신：chrBehaviorLocal.execute
  　로컬 캐릭터의 좌표는 10프레임 간격으로 버퍼 m_culling에 저장되며 과거 4점 분량의 정보를
　　CharacterRoot.SendCharacterCoord에 전달합니다(SendCharacterCoord의 처리는 본서를 참조).

　・리모트 캐릭터의 좌표 송신：chrBehaviorNet.CalcCoordinates
　　리모트 캐릭터의 좌표는 통신 모듈에서 수신한 데이터를 CharacterRoot.OnReceiveCharacterPacket에서 
　　처리하여 CalcCoordinates에 전달합니다.


▼ 아이템 획득과 폐기
　・게임 서버
　　아이템 획득 조정：GameServer.MediatePickupItem
　　　아이템 획득 문의는 GameServer.OnReceiveItemPacket에서 수신하고 MediatePickupItem이
　　　호출됩니다(MediatePickupItem 처리는 본서를 참조).
　　
　　아이템 폐기 조정：GameServer.MediateDropItem
　　　아이템 획득 문의는 GameServer.OnReceiveItemPacket에서 수신하고 MediateDropItem이 
　　　호출됩니다. (MediateDropItem 처리는 본서를 참조)
	
　・아이템 획득 문의 : ItemManager.queryPickItem
　　　아이템이 클릭되면 chrBehaviorLocal.exec_step_move로 chrController.cmdItemQueryPick
　　　에서 ItemManager.queryPickItem로 쿼리를 발행하여 서버에 문의합니다.
　　　(처리 내용은 본서를 참조)

　・아이템 폐기 문의：ItemManager.queryDropItem
　　　아이템을 가지고 있는데 다른 아이템을 주운 경우, chrBehaviorPlayer.execute_queries로
　　　chrController.cmdItemQueryDrop에서 ItemManager.queryDropItem 쿼리를 발행해 서버에 문의합니다.

　・운반할 수 없는 아이템：MapCreator.loadLevel
　　　아이템은 다른 정원으로 가져갈 수 있는 것과 가져갈 수 없는 것이 있습니다.
　　　두부장수 정원에는 파가 자라고, 콩장수의 정원에는 유자가 자랍니다.각각의 정원에는 파나 유자밖에 
　　　가져갈 수 없습니다.이 때문에 생성하는 아이템의 active 상태 설정을 MapCreator.loadLevel로 합니다. 
　　

▼ 이사
　・이사 시작/종료 송신：chrBehaviorLocal.execute
　　　집이 클릭되면 chrBehaviorLocal.execute로 chrController.cmdQueryHouseMoveStart에서 쿼리가 발행됩니다.
　　　이 쿼리로 CharacterRoot.queryHouseMoveStart에서 게임 서버의 리플렉터에 송신됩니다.

　・이사 시작/종료 수신：CharacterRoot.OnReceiveMovingPacket
　　　게임 서버의 리플렉터에 의해, 리모트 단말은 정보를 수신하면 CharacterRoot.OnReceiveMovingPacket에서
　　　이사 처리가 이루어집니다.(본서 참조)


▼ 채팅
　・채팅 메시지 송신：chrBehaviorLocal.execute
　　　채팅 메시지의 텍스트 필드에 입력이 이루어지면 chrBehaviorLocal.execute에서 chrController.cmdQueryTalk가
　　　호출됩니다. 이 함수에서 CharacterRoot.queryTalk로 메시지가 송신됩니다.

　・채팅 메시지 수신：CharacterRoot.OnReceiveChatMessage
　　　채팅 메시지를 수신한 단말은 로컬 단말과 마찬가지로 쿼리를 발행하여 말풍선에 메시지를 표시합니다.
 　　(본서 참조)

▼정원 이동
　・나가고 돌아올 때 아이템 제어：LeaveEvent.execute
　　　상대의 정원에 돌러갈 때와 돌아올 때는 LeaveEvent.execute안에서 this.step.do_transition()이  STEP.START가
　　　되었을 때 운반에 관한 아이템을 제어합니다.

　・나가기 / 돌아오기 송신：GameRoot.NotifyFieldMoving
　　　나갈 때는 EventBoxLeave.Update안에서 출발 트리거를 검사하여 GameRoot.NotifyFieldMoving에서 
　　　나가고 돌아올 때의 정보를 게임 서버에 송신합니다.
　　　게임 서버는 리플렉터로 각 단말에 송신합니다.

　・나가기 / 돌아오기 수신 : GameRoot.OnReceiveGoingOutPacket
　　　리모트 단말이 나가고 돌아오는 정보를 수신하면, GameRoot.OnReceiveGoingOutPacket이 호출됩니다. 
　　　이 함수 안에서 이벤트를 발생시키는 캐릭터와 정원,리모트 캐릭터의 위치에 따라 다음에 할 제어를 전환합니다.


● MonoDevelop에서의 빌드 
샘플 코드는 C#의 디폴트 인수를 사용합니다.
그래서 MonoDevelop 설정에 따라서는 빌드 오류가 생길 경우도 있습니다.
이런 경우는 MonoDevelop의 Solusion 창의 Assembly-CSharp를 선택하고[우클릭]-[Options를 선택합니다.
Project-Options의 [Build]-[General]-[Target framework] 항목을 .NET 4.0 이상으로 선택하고 OK를 누르세요.
