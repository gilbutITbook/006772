using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// ?곸쓽 ?μ?(?쒕꽕?덉씠??.
public class chrBehaviorEnemy_Lair : chrBehaviorEnemy {

	// ================================================================ //
	// MonoBehaviour?먯꽌 ?곸냽.

	void	Awake()
	{
		// ?뚮젅?댁뼱媛 諛 ???녾쾶.
		this.GetComponent<Rigidbody>().isKinematic = true;
	}

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ?ㅽ룿???곸쓽 ?뺣낫.
	public class SpawnEnemy {

		public string	enemy_name = "Enemy_Obake";		// ??醫낅쪟("Enemy_Obake", "Enemy_Kumasan" ??.

		public Enemy.BEHAVE_KIND				behave_kind = Enemy.BEHAVE_KIND.UROURO;		// ?됰룞 ?⑦꽩.
		public Character.ActionBase.DescBase	behave_desc = null;							// Action ?앹꽦 ???듭뀡.

		public float	frequency = 1.0f;				// 諛쒖깮 ?뺣쪧.
	}
	protected List<SpawnEnemy>	spawn_enemies = new List<SpawnEnemy>();

	// ?ㅽ룿???곸쓽 醫낅쪟 異붽?.
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

	// ?곸쓣 ?묓븯怨??ㅽ룿?쒕떎(LevelControl, ?몄뒪?몄슜).
	public void		spawnEnemy()
	{
		Character.HakoAction	hako_action = this.unique_action as Character.HakoAction;

		if(hako_action != null) {

			hako_action.step.set_next(Character.HakoAction.STEP.SPAWN);
		}
	}

	// ?곸쓣 ?묓븯怨??ㅽ룿?쒕떎(Action, ?몄뒪?몄슜)竊?
	public void		create_enemy_internal()
	{
		// ?깅줉????以묒뿉???쒕뜡?섍쾶 ?좏깮.

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
				// ??臾몄옄?댁쓣 ?④퍡 寃뚯뒪?몄뿉 ?≪떊?쒕떎.
				string		pedigree = enemy.name + "." + spawn_enemy.behave_kind;
				
				// ?먭꺽 ?≪떊.
				EnemyRoot.get().RequestSpawnEnemy(this.name, pedigree);

				//Debug.Log(pedigree);
			}
		}
	}

	// ?곸쓣 ?묓븯怨??ㅽ룿?쒕떎(LevelControl, 寃뚯뒪?몄슜).
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

	// ?곸쓣 ?묓븯怨??ㅽ룿?쒕떎(Action, 寃뚯뒪?몄슜竊?
	public void		create_enemy_internal_pedigree(string pedigree)
	{
		// ?깅줉????以묒뿉???쒕뜡?섍쾶 ?좏깮.

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

	// ?誘몄?瑜?諛쏆븯?????몄텧.
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
	// ?좊땲硫붿씠???대깽??

	// ?щ쭩 ?좊땲硫붿씠??醫낅즺 ?대깽?몃? ?좊땲硫붿씠?섏쑝濡쒕???諛쏅뒗??
	public void NotifyFinishedDeathAnimation()
	{
		Character.HakoAction	action = this.unique_action as Character.HakoAction;

		action.is_death_motion_finished = true;
	}

	// ???앹꽦 紐⑥뀡?몄씤 'Pe!' ??대컢???몄텧?쒕떎..
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

// ???쒕꽕?덉씠???≪뀡.
public class HakoAction : ActionBase {

	// ---------------------------------------------------------------- //

	public enum STEP {

		NONE = -1,

		IDLE = 0,
		READY,				// ?湲?以?
		SPAWN,				// ???ㅽ룿.
		PECHANCO,			// 李뚭렇?ъ쭊??

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	public bool		is_death_motion_finished;		// 二쎈뒗 ?좊땲硫붿씠???ъ깮 ??.
													// (?좊땲硫붿씠???대깽???덉뿉???ㅼ젙?쒕떎).

	public bool		is_trigger_pe;					// ??異쒗쁽 ?좊땲硫붿씠?섏쓽 異쒗쁽 ??대컢.

	public string 	pedigree = "";					// 寃뚯뒪?몄슜. ?몄뒪?몄뿉???앹꽦???곸쓽 ?대쫫怨??됰룞 ?⑦꽩.

	// ================================================================ //

	public override void	start()
	{
		this.step.set_next(STEP.READY);
	}

	public override void	execute()
	{
		BasicAction	basic_action = this.behavior.basic_action;

		// ---------------------------------------------------------------- //
		// ?ㅼ쓬 ?곹깭濡??댄뻾?좎? 泥댄겕?쒕떎.

		switch(this.step.do_transition()) {

			// ?湲?以?
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

			// ???ㅽ룿.
			case STEP.SPAWN:
			{
				// ?ㅽ룿 ?좊땲硫붿씠?섏씠 ?앸굹硫??湲??곹깭濡??뚯븘媛꾨떎.
				do {

					// ?좊땲硫붿씠???꾪솚 以묒씠硫??뚯븘媛吏 ?딅뒗??
					// (?닿구 ?ｌ뼱?먯? ?딆쑝硫?"idle" -> "generate"??
					//  ?꾪솚 以묒뿉 ?ㅼ쓬 if臾몄씠 true媛 ?섏뼱 踰꾨┛??.
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

			// 李뚭렇?ъ쭊??
			case STEP.PECHANCO:
			{
				if(this.is_death_motion_finished) {

					this.step.set_next_delay(STEP.IDLE, 0.5f);
				}
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// ?곹깭 ?꾪솚 ??珥덇린??

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {

				case STEP.IDLE:
				{
					// 遺紐?怨꾩링???≪뀡?쇰줈 蹂듦?.
					if(this.parent != null) {

						this.parent.resume();
					}
				}
				break;

				// ?湲?以?	
				case STEP.READY:
				{
				}
				break;

				// ???ㅽ룿.
				case STEP.SPAWN:
				{
					this.is_trigger_pe = false;
					basic_action.animator.SetTrigger("Generate");
				}
				break;

				// 李뚭렇?ъ쭊??
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
		// 媛??곹깭?먯꽌???ㅽ뻾 泥섎━.

		switch(this.step.do_execution(Time.deltaTime)) {

			// ???ㅽ룿.
			case STEP.SPAWN:
			{
				// 紐⑥뀡 ??대컢??留욎떠 ?곸쓣 ?좏빐?몃떎.
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
