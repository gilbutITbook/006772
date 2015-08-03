using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyRoot : MonoBehaviour {

	private List<chrController>	enemies = new List<chrController>();

	protected Network 	m_network = null;

	protected bool		m_isHost = false;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		
		// Network 클래스 컴포넌트 획득.
		GameObject obj = GameObject.Find("Network");
		
		if(obj != null) {
			m_network = obj.GetComponent<Network>();
			if (m_network != null) {
				m_network.RegisterReceiveNotification(PacketId.MonsterData, OnReceiveMonsterDataPacket);
				m_network.RegisterReceiveNotification(PacketId.BossDirectAttack, OnReceiveDirectAttackPacket);
				m_network.RegisterReceiveNotification(PacketId.BossRangeAttack, OnReceiveRangeAttackPacket);
				m_network.RegisterReceiveNotification(PacketId.BossQuickAttack, OnReceiveQuickAttackPacket);
				m_network.RegisterReceiveNotification(PacketId.BossDead, OnReceiveBossDeadPacket);
			}
		}

		m_isHost = GameRoot.get().isHost();
	}

	
	void	Update()
	{
	}

	// ================================================================ //
#if false
	// 적 캐릭터를 만든다.
	public chrController	createEnemy()
	{
		// 일시적으로 항구 버전の적 캐릭터를 생성하도록 변경.
//		chrController	enemy = CharacterRoot.getInstance().createEnemy("Enemy1");
		chrController	enemy = CharacterRoot.getInstance().createEnemy("EnemyMinato");

		this.enemies.Add(enemy);

		return(enemy);
	}
#endif	
	// 적 캐릭터를 만든다.
	public chrController	createEnemy(string enemy_name)
	{
		chrController	enemy = CharacterRoot.getInstance().createEnemy(enemy_name);

		this.enemies.Add(enemy);

		return(enemy);
	}

	// 적 캐릭터를 만든다.
	public chrController	createEnemy(string chr_name, string controller_class_name, string behavior_class_name)
	{
		chrController	enemy = CharacterRoot.getInstance().createEnemy(chr_name, controller_class_name, behavior_class_name);

		this.enemies.Add(enemy);

		return(enemy);
	}

	// 적 캐릭터 제네레이터를 만든다.
	public chrController	createEnemyLair()
	{
		chrController	enemy = CharacterRoot.getInstance().createEnemy("EnemyLairMinato");

		this.enemies.Add(enemy);

		return(enemy);
	}

	// 적 캐릭터를 삭제한다.
	public void	deleteEnemy(chrController enemy)
	{
		this.enemies.Remove(enemy);

		GameObject.Destroy(enemy.gameObject);
	}

	// 적 리스트를 삭제한다.
	public List<chrController>	getEnemies()
	{
		return(this.enemies);
	}

	// 디버그용 적을 잔뜩 만든다.
	public void		createManyEnemies()
	{
		for(int i = 0;i < 6;i++) {

			chrController	enemy;

			if(i%2 == 0) {

				enemy = this.createEnemy("Enemy_Kumasan");

			} else {

				enemy = this.createEnemy("Enemy_Obake");
			}

			Vector3		position = PartyControl.get().getLocalPlayer().control.getPosition();

			position += new Vector3(Random.Range(-10.0f, 10.0f), 0.0f, Random.Range(-10.0f, 10.0f));

			enemy.transform.position = position;
		}
	}

	// 디버그용　모든 적에게 대미지.
	public void	debugCauseDamageToAllEnemy()
	{
		foreach(var chr in this.enemies) {

			chrBehaviorEnemy	enemy = chr.gameObject.GetComponent<chrBehaviorEnemy>();

			if(enemy != null) {

				enemy.causeDamage();
			}
		}
	}

	// 디버그용   모든 적 퇴장.
	public void	debugCauseVanishToAllEnemy()
	{
		foreach(var chr in this.enemies) {

			chrBehaviorEnemy	enemy = chr.gameObject.GetComponent<chrBehaviorEnemy>();

			if(enemy != null) {

				enemy.causeVanish();
			}
		}
	}


	public void RequestSpawnEnemy(string lairName, string monsterName)
	{
		if(m_network != null) {

			MonsterData monster = new MonsterData ();
	
			monster.lairId = lairName;
			monster.monsterId = monsterName;
	
			MonsterPacket packet = new MonsterPacket(monster);
			int serverNode = m_network.GetServerNode();
	
			m_network.SendReliable<MonsterData>(serverNode, packet);
		}
	}

	// ================================================================ //
	//
	//  로컬 오브젝트에서 호출되는 보스 관계 조작 명령.
	//
	public void RequestBossDirectAttack(string targetName, float attackPower)
	{
		// 호스트 측의 보스를 움직인다 : 이 명령을 로컬의 보스 오브젝트에게 바이패스한다.
		GameObject go = GameObject.FindGameObjectWithTag("Boss");
		if (go != null)
		{
			chrControllerEnemyBoss bossController = go.GetComponent<chrControllerEnemyBoss>();
			if (bossController != null)
			{
				bossController.cmdBossDirectAttack(targetName, attackPower);
				
				// 게스트 측의 보스를 움직인다. 통신 대응(패킷을 만들어 던진다).
				BossDirectAttack attack = new BossDirectAttack();
				
				attack.power = attackPower;
				attack.target = targetName;
				
				BossDirectPacket packet = new BossDirectPacket(attack);

				if (m_network != null)
				{
					int serverNode = m_network.GetServerNode();
					m_network.SendReliable<BossDirectAttack>(serverNode, packet);
					Debug.Log("Sendboss direct attack[Power:" + attackPower + " Target:" + targetName + "]");
				}
			}
		}
	}

	public void RequestBossRangeAttack(float attackPower, float attackRange)
	{
		// 호스트 측의 보스를 움직인다 : 이 명령을 로컬의 보스 오브젝트에 바이패스한다.
		GameObject go = GameObject.FindGameObjectWithTag("Boss");
		if (go != null)
		{
			chrControllerEnemyBoss bossController = go.GetComponent<chrControllerEnemyBoss>();

			if (bossController != null)
			{

				bossController.cmdBossRangeAttack(attackPower, attackRange);
				
				// 게스트 측의 보스를 움직인다. 통신 대응(패킷을 만들어 던진다).
				BossRangeAttack attack = new BossRangeAttack();

				attack.power = attackPower;
				attack.range = attackRange;

				BossRangePacket packet = new BossRangePacket(attack);

				if (m_network != null)
				{
					int serverNode = m_network.GetServerNode();
					m_network.SendReliable<BossRangeAttack>(serverNode, packet);
					Debug.Log("Send boss range attack[Power:" + attackPower + " Ramge:" + attackRange + "]");
				}
			}
		}
	}

	// 보스의 퀵 공격(근접).
	public void		RequestBossQuickAttack(string targetName, float attackPower)
	{
		do {

			GameObject go = GameObject.FindGameObjectWithTag("Boss");

			if(go == null) {

				break;
			}

			chrControllerEnemyBoss bossController = go.GetComponent<chrControllerEnemyBoss>();

			if(bossController == null) {

				break;
			}

			bossController.cmdBossQuickAttack(targetName, attackPower);
	
			// 게스트 측의 보스를 움직인다. 통신 대응(패킷을 만들어 던진다).
			BossQuickAttack attack = new BossQuickAttack();

			attack.target = targetName;
			attack.power = attackPower;

			BossQuickPacket packet = new BossQuickPacket(attack);

			if (m_network != null)
			{
				int serverNode = m_network.GetServerNode();
				m_network.SendReliable<BossQuickAttack>(serverNode, packet);
				Debug.Log("Send boss quick attack[Target" + targetName + " Power:" + attackPower + "]");
			}

		} while(false);
	}

	public void RequestBossDead(string bossId)
	{
		// 호스트 측의 보스를 움직인다 : 이 명령을 로컬의 보스 오브젝트에 바이패스한다.
		GameObject go = GameObject.FindGameObjectWithTag("Boss");
		if (go != null)
		{
			chrControllerEnemyBoss bossController = go.GetComponent<chrControllerEnemyBoss>();
			if (bossController != null)
			{
				BossDead dead = new BossDead();
				
				dead.bossId = bossId;
				
				BossDeadPacket packet = new BossDeadPacket(dead);
				
				if (m_network != null)
				{
					int serverNode = m_network.GetServerNode();
					m_network.SendReliable<BossDead>(serverNode, packet);
					Debug.Log("Send boss dead");
				}
			}
		}

	}
	
	// ================================================================ //
	//
	//  하급 몬스터 관계 조작 명령 패킷 수신.
	//

	public void OnReceiveMonsterDataPacket(int node, PacketId id, byte[] data)
	{
		if (m_isHost) {
			// 호스트의 몬스터는 발생 완료.
			return;
		}

		MonsterPacket packet = new MonsterPacket(data);
		MonsterData monster = packet.GetPacket();

		//Debug.Log("[CLIENT] Receive monster data packet:" + monster.lairId + " - " + monster.monsterId);

		var	lairs = enemies.FindAll(x => (x.behavior as chrBehaviorEnemy_Lair) != null);
		
		foreach(var lair in lairs) {

			if (lair.name == monster.lairId) {

				QuerySpawn query = new QuerySpawn(lair.name, monster.monsterId);

				query.set_done(true);
				query.set_success(true);

				QueryManager.get().registerQuery(query);
			
			}
		}

	}
	
	// ================================================================ //
	//
	//  보스 관계 조작 명령 패킷 수신.
	//
	
	public void OnReceiveDirectAttackPacket(int node, PacketId id, byte[] data)
	{
		if (m_isHost) {
			// 호스트의 몬스터는 실행 완료.
			return;
		}

		// 패킷에서 파라미터를 추출해서 ↓의 코드 스니펫을 호출한다.
		GameObject go = GameObject.FindGameObjectWithTag("Boss");
		if (go != null)
		{
			chrControllerEnemyBoss bossController = go.GetComponent<chrControllerEnemyBoss>();
			if (bossController != null)
			{
				BossDirectPacket packet = new BossDirectPacket(data);
				BossDirectAttack attack = packet.GetPacket();

				string targetName = attack.target;
				float attackPower = attack.power;

				Debug.Log("Receive boss direct attack[Power:" + attackPower + " Target:" + targetName + "]");
				bossController.cmdBossDirectAttack(targetName, attackPower);
			}
		} 
	}
	
	public void OnReceiveRangeAttackPacket(int node, PacketId id, byte[] data)
	{
		if (m_isHost) {
			// 호스트의 몬스터는 실행 완료.
			return;
		}

		// 패킷에서 파라미터를 추출해서 ↓의 코드 스니펫을 호출한다.
		GameObject go = GameObject.FindGameObjectWithTag("Boss");
		if (go != null)
		{
			chrControllerEnemyBoss bossController = go.GetComponent<chrControllerEnemyBoss>();
			if (bossController != null)
			{
				BossRangePacket packet = new BossRangePacket(data);
				BossRangeAttack attack = packet.GetPacket();
				
				float attackPower = attack.power;
				float attackRange = attack.range;

				Debug.Log("Receive boss quick attack[Power:" + attackPower + " Ramge:" + attackRange + "]");
				bossController.cmdBossRangeAttack(attackPower, attackRange);
			}
		} 
	}

	public void OnReceiveQuickAttackPacket(int node, PacketId id, byte[] data)
	{
		if (m_isHost) {
			// 호스트의 몬스터는 실행 완료.
			return;
		}

		// 패킷에서 파라미터를 추출해서 ↓의 코드 스니펫을 호출한다.
		GameObject go = GameObject.FindGameObjectWithTag("Boss");
		if (go != null)
		{
			chrControllerEnemyBoss bossController = go.GetComponent<chrControllerEnemyBoss>();
			if (bossController != null)
			{
				BossQuickPacket packet = new BossQuickPacket(data);
				BossQuickAttack attack = packet.GetPacket();
				
				string targetName = attack.target;
				float attackPower = attack.power;

				Debug.Log("Receive boss range attack[Ramge:" + targetName + " Power:" + attackPower + "]");
				dbwin.console().print("Receive boss range attack[Ramge:" + targetName + " Power:" + attackPower + "]"); 

				bossController.cmdBossQuickAttack(targetName, attackPower);
			}
		} 
	}

	// 보스 사망 정보 수신 함수.
	public void OnReceiveBossDeadPacket(int node, PacketId id, byte[] data)
	{
		BossDeadPacket packet = new BossDeadPacket(data);
		BossDead dead = packet.GetPacket();

		chrBehaviorEnemyBoss behavior = CharacterRoot.get(). findCharacter<chrBehaviorEnemyBoss>(dead.bossId);
		
		if (behavior == null) {
			return;
		}

		behavior.dead();

		Debug.Log("Receive boss dead packet");
	}

	// ================================================================ //

	private	static EnemyRoot	instance = null;

	public static EnemyRoot	get()
	{
		if(EnemyRoot.instance == null) {

			EnemyRoot.instance = GameObject.Find("GameRoot").GetComponent<EnemyRoot>();
		}

		return(EnemyRoot.instance);
	}
	public static EnemyRoot	getInstance()
	{
		return(EnemyRoot.get());
	}
}
