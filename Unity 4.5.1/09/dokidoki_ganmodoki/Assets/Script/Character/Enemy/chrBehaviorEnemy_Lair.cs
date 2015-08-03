using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// 적의 둥지(제네레이터).
public class chrBehaviorEnemy_Lair : chrBehaviorEnemy {

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
		// 플레이어가 밀 수 없게.
		this.rigidbody.isKinematic = true;
	}

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// 스폰할 적의 정보.
	public class SpawnEnemy {

		public string	enemy_name = "Enemy_Obake";		// 적 종류("Enemy_Obake", "Enemy_Kumasan" 등).

		public Enemy.BEHAVE_KIND				behave_kind = Enemy.BEHAVE_KIND.UROURO;		// 행동 패턴.
		public Character.ActionBase.DescBase	behave_desc = null;							// Action 생성 시 옵션.

		public float	frequency = 1.0f;				// 발생 확률.
	}
	protected List<SpawnEnemy>	spawn_enemies = new List<SpawnEnemy>();

	// 스폰할 적의 종류 추가.
	public SpawnEnemy		resisterSpawnEnemy()
	{
		var	spawn = new SpawnEnemy();

		this.spawn_enemies.Add(spawn);

		return(spawn);
	}

	// ================================================================ //

	public override void	initialize()
	{
		base.initialize();

		this.unique_action = new Character.HakoAction();
		this.unique_action.create(this);

		this.basic_action.unique_action = this.unique_action;
	}
	public override void	start()
	{
		base.start();
		this.control.vital.hit_point = 5.0f;
	}


	public override	void	execute()
	{
		base.execute();
		this.basic_action.execute();

		this.is_attack_motion_finished = false;
	}

	// ================================================================ //

	// 적을 펑하고 스폰한다(LevelControl, 호스트용).
	public void		spawnEnemy()
	{
		Character.HakoAction	hako_action = this.unique_action as Character.HakoAction;

		if(hako_action != null) {

			hako_action.step.set_next(Character.HakoAction.STEP.SPAWN);
		}
	}

	// 적을 펑하고 스폰한다(Action, 호스트용)）.
	public void		create_enemy_internal()
	{
		// 등록된 적 중에서 랜덤하게 선택.

		if(this.spawn_enemies.Count == 0) {

			this.spawn_enemies.Add(new SpawnEnemy());
		}

		float	sum = 0.0f;

		foreach(var se in this.spawn_enemies) {

			sum += se.frequency;
		}

		SpawnEnemy	spawn_enemy = this.spawn_enemies[0];

		float	rand = Random.Range(0.0f, sum);

		foreach(var se in this.spawn_enemies) {

			rand -= se.frequency;

			if(rand <= 0.0f) {

				spawn_enemy = se;
				break;
			}
		}

		//
		dbwin.console().print("Spawn LairName:" + this.name);
		dbwin.console ().print("Create enemy:" + spawn_enemy.enemy_name);

		//Debug.Log("Spawn LairName:" + this.name);
		//Debug.Log("Create enemy:" + spawn_enemy.enemy_name);

		chrBehaviorEnemy	enemy = LevelControl.get().createCurrentRoomEnemy<chrBehaviorEnemy>(spawn_enemy.enemy_name);

		if(enemy != null) {

			enemy.setBehaveKind(spawn_enemy.behave_kind, spawn_enemy.behave_desc);
			enemy.beginSpawn(this.transform.position + Vector3.up*3.0f, this.transform.forward);

			if (GameRoot.get().isHost()) {
				// 이 문자열을 함께 게스트에 송신한다.
				string		pedigree = enemy.name + "." + spawn_enemy.behave_kind;
				
				// 원격 송신.
				EnemyRoot.get().RequestSpawnEnemy(this.name, pedigree);

				//Debug.Log(pedigree);
			}
		}
	}

	// 적을 펑하고 스폰한다(LevelControl, 게스트용).
	public void		spawnEnemyFromPedigree(string pedigree)
	{
		Character.HakoAction	hako_action = this.unique_action as Character.HakoAction;

		if(hako_action != null) {

			hako_action.pedigree = pedigree;
			hako_action.step.set_next(Character.HakoAction.STEP.SPAWN);

			dbwin.console().print("*Spawn LairName:" + this.name);
			dbwin.console ().print ("*Create enemy:" + pedigree);	
			//Debug.Log("*Spawn LairName:" + this.name);
			//Debug.Log("*Create enemy:" + pedigree);
		}
	}

	// 적을 펑하고 스폰한다(Action, 게스트용）.
	public void		create_enemy_internal_pedigree(string pedigree)
	{
		// 등록된 적 중에서 랜덤하게 선택.

		do {

			string[]	tokens = pedigree.Split('.');

			if(tokens.Length < 3) {

				break;
			}

			string	enemy_name = tokens[0] + "." + tokens[1];

			if(!System.Enum.IsDefined(typeof(Enemy.BEHAVE_KIND), tokens[2])) {

				break;
			}

			Enemy.BEHAVE_KIND	behave = (Enemy.BEHAVE_KIND)System.Enum.Parse(typeof(Enemy.BEHAVE_KIND), tokens[2]);

			chrBehaviorEnemy	enemy = LevelControl.get().createCurrentRoomEnemy<chrBehaviorEnemy>(enemy_name);

			if(enemy == null) {

				break;
			}

			enemy.name = enemy_name;
			enemy.setBehaveKind(behave, null);
			enemy.beginSpawn(this.transform.position + Vector3.up*3.0f, this.transform.forward);

		} while(false);

	}

	// ================================================================ //

	// 대미지를 받았을 때 호출.
	public override void		onDamaged()
	{
		if(this.control.vital.hit_point <= 0.0f) {

			Character.HakoAction	action = this.unique_action as Character.HakoAction;

			if(action.step.get_current() != Character.HakoAction.STEP.PECHANCO) {

				action.step.set_next(Character.HakoAction.STEP.PECHANCO);
			}
		}
	}

	// ================================================================ //
	// 애니메이션 이벤트.

	// 사망 애니메이션 종료 이벤트를 애니메이션으로부터 받는다.
	public void NotifyFinishedDeathAnimation()
	{
		Character.HakoAction	action = this.unique_action as Character.HakoAction;

		action.is_death_motion_finished = true;
	}

	// 적 생성 모션인인 'Pe!' 타이밍에 호출된다..
	public void		evEnemy_Lair_Pe()
	{
		Character.HakoAction	action = this.unique_action as Character.HakoAction;

		action.is_trigger_pe = true;
	}


}

// ==================================================================== //
//																		//
//																		//
//																		//
// ==================================================================== //
namespace Character {

// 적 제네레이터 액션.
public class HakoAction : ActionBase {

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		IDLE = 0,
		READY,				// 대기 중.
		SPAWN,				// 적 스폰.
		PECHANCO,			// 찌그러진다.

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	public bool		is_death_motion_finished;		// 죽는 애니메이션 재생 끝?.
													// (애니메이션 이벤트 안에서 설정된다).

	public bool		is_trigger_pe;					// 적 출현 애니메이션의 출현 타이밍.

	public string 	pedigree = "";					// 게스트용. 호스트에서 생성한 적의 이름과 행동 패턴.

	// ================================================================ //

	public override void	start()
	{
		this.step.set_next(STEP.READY);
	}

	public override void	execute()
	{
		BasicAction	basic_action = this.behavior.basic_action;

		// ---------------------------------------------------------------- //
		// 다음 상태로 이행할지 체크한다.

		switch(this.step.do_transition()) {

			// 대기 중.
			case STEP.READY:
			{
				/*do {

					if(!LevelControl.get().canCreateCurrentRoomEnemy()) {

						break;
					}

					if(!Input.GetMouseButtonDown(1)) {
					//if(!this.step.is_acrossing_cycle(5.0f)) {
	
						break;
					}

					this.step.set_next(STEP.SPAWN);

				} while(false);*/
			}
			break;

			// 적 스폰.
			case STEP.SPAWN:
			{
				// 스폰 애니메이션이 끝나면 대기 상태로 돌아간다.
				do {

					// 애니메이션 전환 중이면 돌아가지 않는다.
					// (이걸 넣어두지 않으면 "idle" -> "generate"의.
					//  전환 중에 다음 if문이 true가 되어 버린다).
					if(basic_action.animator.IsInTransition(0)) {

						break;
					}
					if(basic_action.animator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.Generate")) {

						break;
					}

					this.step.set_next(STEP.READY);

				} while(false);
			}
			break;

			// 찌그러진다.
			case STEP.PECHANCO:
			{
				if(this.is_death_motion_finished) {

					this.step.set_next_delay(STEP.IDLE, 0.5f);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				case STEP.IDLE:
				{
					// 부모 계층의 액션으로 복귀.
					if(this.parent != null) {

						this.parent.resume();
					}
				}
				break;

				// 대기 중.	
				case STEP.READY:
				{
				}
				break;

				// 적 스폰.
				case STEP.SPAWN:
				{
					this.is_trigger_pe = false;
					basic_action.animator.SetTrigger("Generate");
				}
				break;

				// 찌그러진다.
				case STEP.PECHANCO:
				{
					basic_action.animator.SetTrigger("Death");
					this.is_death_motion_finished = false;

					this.control.cmdEnableCollision(false);
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 적 스폰.
			case STEP.SPAWN:
			{
				// 모션 타이밍에 맞춰 적을 토해낸다.
				if(this.is_trigger_pe) {

					chrBehaviorEnemy_Lair	behave_lair = this.behavior as chrBehaviorEnemy_Lair;

					if(this.pedigree != "") {

						behave_lair.create_enemy_internal_pedigree(this.pedigree);
						this.pedigree = "";

					} else {

						behave_lair.create_enemy_internal();
					}

					this.is_trigger_pe = false;
				}
			}
			break;

		}

		// ---------------------------------------------------------------- //

	}
}

}
