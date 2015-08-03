
■ 9장 예제 프로그램

● 실행 파일
09\dokidoki_ganmodoki\bin\dokidoki_ganmodoki.exe

● 플레이 방법
▽ 매칭

타이틀 화면에서 '매칭 서버 주소' 텍스트 필드에 매칭 서버를 실행한 단말의 IP 주소를 입력하세요.
매칭 서버와 접속하면, 새롭게 방을 만들지 기존 방에 참가할지 결정합니다.
새롭게 방을 만들 경우는 텍스트 필드에 만들 방의 이름을 입력하여 '방을 만든다' 버튼을 누릅니다.
이 때 텍스트 필드 아래의 레벨 선택 도글 버튼으로 선택된 레벨로 생성됩니다. 
생성한 방의 참가 상황을 확인할 수 있는 표시가 나타나므로 원하는 때 '게임을 시작한다'버튼을 눌러 
게임을 시작할 수 있습니다.
기존 방에 참가하는 경우는 텍스트 필드 아래의 레벨 선택의 토글 버튼으로 레벨을 지정해서 '방을 찾
는다' 버튼을 누릅니다. 검색 조건과 일치하는 방 목록 버튼이 표시되면 참가할 방의 버튼을 눌러 방에 
들어갈 수 있습니다. 

※한 번 들어간 방에서는 나올 수 없습니다.


▽ 게임 
매칭 서버에서 매칭이 이루어지면 게임이 시작됩니다. 매칭된 플레이어와 던전으로 들어갑니다.
던전 안에서는 마우스 왼쪽 버튼을 드래그 하여 캐릭터를 이동시킬 수 있습니다.
마우스 우클릭으로 폭탄(파)를 발사할 수 있습니다. 던전 안에 있는 열쇠를 주워, 이웃한 방으로 이동할 
수 있습니다. 열쇠를 가진 상태에서 열쇠와 같은 색 도넛에 모두 올라타면 옆 방으로 갈 수 있습니다. 
던전에는 몬스터가 배회하고 있습니다. 몬스터는 근접 공격과 원거리 공격으로 무찌를 수 있습니다.
 
또한, 몬스터는 제네레이터에서 발생하기도 합니다. 제네레이터는 파괴하지 않는 한 몬스터를 계속 생성
합니다. 던전 안에는 게이크와 아이스 등의 아이템이 있습니다. 이 아이템으로 몬스터에게서 입은 대미지를 
회복합니다.몬스터에 당했을 때(HP 0)는 현재 방의 시작 지점으로 돌려보냅니다. 겨우 던전을 돌파해도 원래로 
돌아가니 몬스터에게 공격받지 않게 조종합시다. 화면 좌측 상단에 동료와 대화할 수 있는 텍스트 필드가 
있습니다. 동료와 대화하면서 던전을 헤쳐나갑시다. 

※이 예제에서의 제한 사항
・초기 장비 선택, 무기를 바꿀 수 없습니다.
・아이스 추첨은 하지 않습니다.
・보스의 던전으로 진행할 수 없습니다.


● 예제 프로그램
프로젝트 파일：09\dokidoki_ganmodoki\Assets\Scenes\TitleScene.unity
프로그램：09\dokidoki_ganmodoki\Assets\Script


● 통신 관련 파일 구성
Character
    Player			캐릭터 제어 스크립트군
        chrBehaviorLocal.cs		로컬 캐릭터의 비헤이비어
        chrBehaviorNet.cs		리모트 캐릭터의 비헤이비어
        chrBehaviorPlayer.cs	캐릭터의 공통 비헤이비어
    
    CharacterRoot.cs		캐릭터 매니저
    chrBehaviorBase.cs		캐릭터 비헤이비어의 기저 클래스
    chrController.cs		캐릭터 컨트롤러
    MeleeAttack.cs		공격에 관한 제어

Event				이벤트 제어 스크립트군
    EventBoxLeave.cs		정원 이동시 제어(로컬 캐릭터가 이동할 때)
    EventEnter.cs		정원 이동시 제어(리모트 캐릭터가 놀러올 때)
    EventLeave.cs		정원 이동 시 아이템 소지 제어

Item				아이템 제어스크립트군
    ItemBehaviorBase.cs		아이템 비헤이비어 기저 클래스
    ItemController.cs		아이템 컨트롤러
    ItemManager.cs		아이템 매니저

Level
    LevelController.cs		던전 생성 관련 제어

Network				통신제어스크립트군
    GameServer.cs		게임 서버
    IPacket.cs			패킷 인터페이스
    MatchingClient.cs		매칭 클라이언트
    Network.cs			통신 모듈(TransportTCP,TransportUDP 클래스 제어)
    NetworkDef.cs		통신에 관한 정의
    Packet.cs			패킷클래스정의
    PacketQueue.cs		패킷큐 클래스
    PacketSerializer.cs		패킷 시리얼라이저(패킷 헤더 시리얼라이저)
    PacketStructs.cs		패킷 데이터 정의
    Serializer.cs		시리얼라이저 기저 클래스
    SplineData.cs		캐릭터 이동용 스플라인 보간 클래스
    Session.cs			세션 관리 기저 클래스
    SessionUDP.cs		TCP용 세션 관리클래스
    SessionTCP.cs		UDP용 세션 관리클래스
    TransportTcp.cs		TCP소켓 통신 프로그램
    TransportUDP.cs		UDP소켓 통신 프로그램

Stage
    DoorControl.cs		방 이동 제어
    MapCreator.cs		맵 생성 제어

System				게임 시스템 제어스크립트군
    GameRoot.cs			게임 제어
    GlobalParam.cs		씬을 넘나드는 정보 관리
    QueryManager.cs		쿼리 제어
    TitleControl.cs		게임 서버, 호스트-게스트간 접속 시퀀스 제어


● 통신 프로그램 보충
▼캐릭터 이동
　・로컬 캐릭터 좌표 송신：chrBehaviorLocal.execute
  　로컬 캐릭터 좌표는 10프레임 간격으로 버퍼 m_culling에 쌓이고, 과거 4개 점의 정보가
　　CharacterRoot.SendCharacterCoord에 전달됩니다.(SendCharacterCoord의 처리는 본서를 참조하세요)

　・리모트 캐릭터 좌표 수신：chrBehaviorNet.CalcCoordinates
　　리모트 캐릭터 좌표는 통신 모듈에서 수신한 데이터는 CharacterRoot.OnReceiveCharacterPacket
　　에서 처리하여 CalcCoordinates에 전달됩니다.


▼아이템 획득・사용
　・게임 서버
　　아이템 획득 조정：GameServer.MediatePickupItem
　　　아이템 획득 문의는 GameServer.OnReceiveItemPacket에서 수신하고 MediatePickupItem이
　　　호출됩니다(MediatePickupItem 처리는 이 책을 참조하세요)
　　
　　아이템 폐기 조정：GameServer.MediateDropItem
　　　아이템 획득 문의는 GameServer.OnReceiveItemPacket에서 수신하고MediateDropItem이 호출됩니다.
　　　MediateDropItem 처리는 이 책을 참조하세요)
	
　・아이템 획득 문의：ItemManager.queryPickItem
　　　아이템이  클릭되었을 때 chrBehaviorLocal.exec_step_move로 chrController.cmdItemQueryPick에서 
         ItemManager.queryPickItem로 쿼리를 발행하여 서버에 문의합니다.
　　　(각 처리는 이 책을 참조하세요)


▼아이템 사용　　
　・아이템 사용：chrController.cmdUseItemSelf, chrController.cmdUseItemToFriend
　　　케이크를 획득하거나 아이스를 사용할 때는 chrController.cmdUseItemSelf, chrController.cmdUseItemToFriend
　　　로부터 ItemManager.useItem이 호출됩니다. 이 함수로 아이템을 사용하고 정보를 송신합니다.
　　　아이템 사용 정보를 수신하면 ItemManager.OnReceiveUseItemPacket이 호출되고 리모트 측에서 
         ItemManager.useItem으로 아이템을 사용합니다.
 

▼캐릭터 공격
　・공격：chrBehaviorLocal.execute, MeleeAttack.attack
　　　캐릭터가 공격하면 CharacterRoot.SendAttackData가 호출됩니다. 공격 종류를 판별하여 패킷을 보냅니다.
　　　이 통신은 캐릭터가 공격하고 있는 것처럼 보이게 하는 연출을 위해 데이터가 유실돼도 게임 진행에 영향을
　　　미치지 않게 UDP로 통신을 합니다.
　　　데이터를 수신하면 CharactorRoot.OnReceiveAttackPacket가 호출됩니다.이 함수로 송신 시에 지정된 공격 
         종류에 따라서 chrBehaviorNet.cmdShotAttack, chrBehaviorNet.cmdMeleeAttack를 호출하여 캐릭터에게 
         공격을 시킵니다. 단, 리모트 캐릭터는 컬리전을 가지고 있지 않으므로 공격하는 척할 뿐입니다.
　　

▼대미지・HP 알림
　・대미지 알림：chrController.caseDamage
　　　각 단말에서 몬스터로의 공격이 이루어졌을 때 chrController.caseDamage가 호출됩니다. 이 함수에서 받은 
         대미지양을 호스트 단말에 통신합니다. 게스트 단말로부터 몬스터 공격 대미지를 통지받은 호스트 단말은 
         수신한 대미지양을 CharacterRoot.NotifyDamage를 호출하여 각 단말에 알려줍니다. 이 NotifyHitPoint 함수로
         게임 서버의 리플렉터를 거쳐 각 단말에 알려줍니다.

　・HP 통신：chrController.caseDamage
　　　각 캐릭터가 몬스터로부터 공격받았을 때 chrController.caseDamage가 호출됩니다.
　　　이 함수 내에서 각 단말에 통지하고자 CharacterRoot.NotifyHitPoint를 호출합니다.
　　　이 NotifyHitPoint 함수로 게임 서버의 리플렉터를 거쳐 각 단말에 알려줍니다.

▼몬스터 발생 통지
　・몬스터 리스폰：EnemyRoot.RequestSpawnEnemy
　　　호스트 단말의 LevelControl.Update로 몬스터의 리스폰이 이루어지면 EnemyRoot.RequestSpawnEnemy가 호출됩니다.
　　　RequestSpawnEnemy 함수로 발생시킬 제네레이터를 지정하고 게임 서버의 리플렉터를 거쳐 모든 단말에 통지됩니다.
         수신한 각 단말에서 발생시킴으로써 발생을 동기화합니다.
　　　이 통지를 받는 동안 제네레이터를 공격해도 대미지양은 호스트 단말에 송신한 후, 모든 단말에 통지되므로 발생이
　　　동기화됩니다.(자세한 것은 이 책을 참조해 주세요).

▼채팅
　・채팅 메시지 송신：chrBehaviorLocal.execute
　　　채팅 메시지의 텍스트 필드에 입력이 이루어지면서 텍스트 필드에 입력이 이루어지면 chrBehaviorLocal.execute에서 
      chrController.cmdQueryTalk가 호출됩니다.이 함수에서 CharacterRoot.queryTalk로 메시지가 송신됩니다.

　・채팅 메시지 수신：CharacterRoot.OnReceiveChatMessage
　　　채팅 메시지를 수신한 리모트 단말은 로컬 단말처럼 쿼리를 발행해서 말풍선에 메시지를 표시합니다.
 　　 (자세한 내용은 본문을 참조해주세요)


▼무기 선택/동기화 대기
　・무기 선택 통지：WeaponSelectLevelSequence.execute
　　　무기 선택은 WeaponSelectLevelSequence에서 이루어집니다. 무기를 선택한 후, execute 함수에서 글로벌 ID와 
      선택한 무기 종류를 모아서 게임 서버에 통지합니다.

　・무기 선택 동기화 대기：GameServer.checkInitidalEquipment
　　　게임 서버가 각 단말로부터 선택한 무기 정보를 수신하면 GameServer.OnReceiveEquipmentPacket 함수가 호출됩니다.
　　　이 함수로 수신한 단말에서 선택된 무기의 종류를 보존합니다. 그 후 checkInitidalEquipment 함수로 모든 단말에서 
         무기 선택 정보를 수신했는지 감시하고, 모든 단말에서 선택됐을 때 전원의 무기 선택 정보를 모든 단말에 보냅니다.
　　　전원의 무기 선택 정보를 수신한 각 단말에서는 WeaponSelectLevelSequence.OnReceiveSyncPacket 함수가 호출됩니다.
　　　이 정보를 GlobalParam에 보존합니다.무기 선택 동기화 패킷은 GameScene로 전환하는 신호도 됩니다.
　　　이 패킷을 수신한 후엔 다음 씬으로 전환하는 처리를 합니다.


▼아이스 당첨
　・아이스 당첨 처리 ：
　　　아이스 당첨은 본문 설명대로 통신하지 않습니다. 당첨됐을 때 평소처럼 소비하지 않고 다시 사용할 수 있게 하기만
      하면 단말 간에 동기화된 상태가 됩니다.


▼보스 이동/공격/사망 통지
　・보스 이동 좌표 송신：chrBehaviorEnemyBoss.sendCharacterCoordinates
 　 　로컬 캐릭터 좌표는 10프레임 간격으로 버퍼 m_culling에 저장되며 과거 4점 분량의 정보를 
　　　CharacterRoot.SendCharacterCoord에 전달합니다. (SendCharacterCoord 처리는 본문을 참조하세요)


　・보스 직접공격 송신：chrBehaviorEnemyBoss.decideNextStep
　　　직접공격은 decideNextStep 함수 내에서 단말이 호스트인 경우만 실행됩니다. decideNextStep 함수에서 
      EnemyRoot.RequestBossDirectAttack 함수로 공격 대상인 캐릭터 ID와 공격력을 지정하고 게임 서버의 
      리플렉터를 거쳐 각 게스트 단말에 통지됩니다.
　　　각 게스트 단말은 EnemyRoot.OnReceiveDirectAttackPacket 함수로 캐릭터 ID와 공격력을 수신합니다. 이 정보를 
　　　chrControllerEnemyBoss.cmdBossDirectAttack 함수로 알리고 보스 공격을 합니다.


　・보스 범위 공격 송신：chrBehaviorEnemyBoss.decideNextStep
　　　범위공격도 직접공격과 마찬가지로 decideNextStep 함수 내에서 단말이 호스트인 경우만 실행됩니다.
      decideNextStep 함수에서 EnemyRoot.RequestBossRangeAttack 함수로 공격 범위와 공격력을 지정하고, 게임 서버의 
      리플렉터를 거쳐 각 게스트 단말에 알립니다. 
      각 게스트 단말은 EnemyRoot.OnReceiveRangeAttackPacket 함수로 범위공격과 공격력을 수신합니다.
　　　이 정보를 chrControllerEnemyBoss.cmdBossRangeAttack 함수에 통지해여 보스 공격을 합니다.


　・보스 사망 통지：EnemyRoot.RequestBossDead
　　　보스와의 전투 시에 단말의 씬 로드와 이동 시의 매우 작은 동기화의 차이로 보스 대미지 통지의 송수신에 차이가 발생하는 일이
　　　있습니다. 보스 씬에 들어오면 동기화를 기다려도 되지만, 몬스터 등의 사망 통지를 송수신하는 방법도 있으므로 동기가 어긋날 때의
　　　안전장치로서 이쪽을 구현했습니다(게임으로서는 연출 등으로 동기화를 기다리는 게 좋은 방법이라고 생각합니다).
　　　보스가 사망했을 때 chrControllerEnemyBase.goToVanishState 함수로 사망 시 처리가 시작됩니다. 
        이때 EnemyRoot.RequestBossDead 함수를 호출하여 게임 서버의 리플렉터를 거쳐서 사망을 알립니다.
　　　보스 사망 통지를 수신한 단말은 EnemyRoot.OnReceiveBossDeadPacket 함수가 호출됩니다. 이 함수에서 
　　　chrControllerEnemyBase.causeVanish 함수를 호출함으로써 보스를 사망하게 할 수 있습니다.


▼보너스 케이크 무한제공
　・케이크 획득 수 통지 : BossLevelSequenceResult.sendPrizeData
　　　케이크 획득이 끝나면 시퀀스가 BossLevelSequenceResult로 전환됩니다. 이 시퀀스로 이행했을 때의 Start 함수로 sendPrizeData 함수를 
         호출하고 케이크 획득 수를 알립니다。


　・케이크 획득 결과：GameServer.checkReceivePrizePacket
　　　케이크 획득 수를 수신한 게임 서버는 GameServer.OnReceivePrizePacket 함수가 호출되어 각 단말의 케이크 획득 수를 보존합니다.
　　　모든 단말의 획득 수를 수신할 때까지 checkReceivePrizePacket 함수로 감시합니다. 모든 단말의 획득 수를 수신하면 획득 결과를 모아서 
　　　단말에 보냅니다.
　　　획득 결과를 수신한 단말은 BossLevelSequenceResult.OnReceivePrizeResultPacket 함수가 호출되고 각 캐릭터가 획득한 케이크 수를 보존합니다.
         수이 끝나면 execute 함수로 획득 결과를 집계하여 표시합니다。



● 1대의 단말에서 실행할 때
이 예제는 실행할 단말을 여러 대 준비할 수 없는 분을 위해 한 대의 단말에서도 동작하게 만들었습니다.
이 때문에 UDP에서 사용하는 포트 번호를 기준 포트에 플레이어 번호를 더한 포트 번호를 사용합니다.
별도의 단말에서 통신할 경우는 보통은 모든 단말에서 같은 포트 번호를 사용합니다. 

또한, 디버그 용도로서 매칭 서버를 사용하지 않는 게임 플레이도 할 수 있습니다. TitleControl.cs, MatchingClient.cs로 정의되어 있는 
UNUSE_MATCHING_SERVER의 정의를 유효하게 해줍니다. 이 경우는 같은 단말에서 모든 애플리케이션을 실행하세요.

●MonoDevelop로 빌드하기 
예제 코드는 C#의 기본 인수를 사용합니다.
그러므로 MonoDevelop의 설정에 따라서는 빌드 오류가 생길 때가 있습니다. 
이런 때는 MonoDevelop의 Solusion 창의 Assembly-CSharp을 선택하여 [우클릭]-[Options]를 선택합니다.
Project-Options의 [Build]-[General]-[Target framework] 항목을 .NET 4.0 이상으로 선택하고 [OK]를 눌러주세요. 
