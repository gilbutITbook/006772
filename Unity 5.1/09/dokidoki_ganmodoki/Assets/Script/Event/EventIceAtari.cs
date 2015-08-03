using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MathExtension;
using GameObjectExtension;

// 아이스 당첨 이벤트.
public class EventIceAtari : EventBase {

	// ================================================================ //

	public enum STEP {

		NONE = -1,

		IDLE = 0,			// 실행 중이 아님.

		START,				// 이벤트 시작.
		BIKKURI,			// "！" 말풍선.
		TURN_AND_RISE,		// 빙글 돌아서 모션 재생.
		STICK_THROW,		// 아이스 스틱을 던진다.
		STICK_FLYING,		// 아이스 스틱이 날아온다.

		END,

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	protected chrBehaviorPlayer		player = null;
	protected GameObject			player_weapon = null;		// 플레이어가 든 무기.
	protected GameObject			ice_bar = null;

	protected EventDataIceAtari		data = null;

	protected Sprite2DControl		sprite_bikkuri;
	protected Sprite2DControl		sprite_ice_bar;
	protected Sprite2DControl		sprite_atari;

	protected GameObject			ougi;
	protected Vector3				ougi_position = Vector3.zero;

	protected SimpleSplineObject	spline = null;
	protected SimpleSpline.Tracer	tracer = new SimpleSpline.Tracer();

	protected ipModule.Spring		spring          = new ipModule.Spring();		// 당첨 말풍선용.
	protected ipModule.Spring		bikkuri_spring  = new ipModule.Spring();
	protected ipModule.FCurve		ice_fcurve      = new ipModule.FCurve();
	protected ipModule.FCurve		ougi_fcurve     = new ipModule.FCurve();		// 뒤 부채용.
	protected ipModule.FCurve		ice_zoom_fcurve = new ipModule.FCurve();		// 플레이어가 가지고 있는 아이스바(당첨)용.
	protected ipModule.FCurve		atari_fcurve    = new ipModule.FCurve();		// 당첨 말풍선용.


	protected CameraModule.Posture	cam_posture0;
	protected CameraModule.Posture	cam_posture1;

	protected ipModule.FCurve		camera_fcurve = new ipModule.FCurve();

	protected int			slot_index = 0;
	protected Item.Favor	item_favor;

	protected List<chrController>	hide_enemies;

	// ================================================================ //


	public EventIceAtari()
	{
	}
	public void		setItemSlotAndFavor(int slot, Item.Favor item_favor)
	{
		this.slot_index = slot;
		this.item_favor = item_favor;
	}

	public override void	initialize()
	{
		this.player = PartyControl.get().getLocalPlayer();
		this.player_weapon = this.player.gameObject.findDescendant("anim_wepon");

		this.data = GameObject.Find("EventDataIceAtari").GetComponent<EventDataIceAtari>();

		// 당첨의 막대 모델.
		this.ice_bar = this.data.prefab_ice_atari_bar.instantiate();

		this.ice_bar.setParent(this.player.gameObject.findDescendant("anim_wrist_R"));
		this.ice_bar.setLocalPosition(new Vector3(-0.056f, -0.086f, 0.039f));
		this.ice_bar.SetActive(false);

		// "！" 말풍선.
		this.sprite_bikkuri   = Sprite2DRoot.get().createSprite(this.data.texture_bikkuri,   true);
		this.sprite_bikkuri.setVisible(false);

		// 당첨 말풍선.
		this.sprite_atari   = Sprite2DRoot.get().createSprite(this.data.texture_atari,   true);

		this.sprite_atari.setPosition(new Vector2(0.0f, 150.0f));
		this.sprite_atari.setVisible(false);

		this.sprite_ice_bar = Sprite2DRoot.get().createSprite(this.data.texture_ice_bar, true);
		this.sprite_ice_bar.setVisible(false);
		this.sprite_ice_bar.setMaterial(this.data.material_ice_sprite);

		this.spline = this.data.gameObject.findDescendant("spline_ice").GetComponent<SimpleSplineObject>();
		this.tracer.attach(this.spline.curve);
	}

	// 이벤트 시작.
	public override void		start()
	{
		this.tracer.restart();
		this.step.set_next(STEP.START);
	}

	public override void	execute()
	{
		CameraModule	camera_module = CameraControl.get().getModule();

		float		ougi_pos_y0 = -3.2f;
		float		ougi_pos_y1 =  0.0f;
		
		// ---------------------------------------------------------------- //
		// 다음 상태로 이동할지 체크.

		switch(this.step.do_transition()) {

			case STEP.START:
			{
			}
			break;

			// "！" 말풍선.
			case STEP.BIKKURI:
			{
				if(this.step.get_time() > 1.0f) {

					this.sprite_bikkuri.setVisible(false);
					this.step.set_next(STEP.TURN_AND_RISE);
				}
			}
			break;

			// 빙글 돌아 모션 재생.
			case STEP.TURN_AND_RISE:
			{
				if(this.step.get_time() > 2.0f) {

					this.step.set_next(STEP.STICK_THROW);
				}
			}
			break;

			// 아이스 스틱을 던진다.
			case STEP.STICK_THROW:
			{
				if(this.player.control.getMotionCurrentTime() > 0.3f) {

					this.step.set_next(STEP.STICK_FLYING);
				}
			}
			break;

			// 아이스 스틱이 날라온다.
			case STEP.STICK_FLYING:
			{
				if(!this.ice_fcurve.isMoving()) {

					ItemWindow.get().setItem(Item.SLOT_TYPE.MISC, this.slot_index, this.item_favor);

					this.player.item_slot.miscs[this.slot_index].is_using = false;

					this.step.set_next(STEP.END);
				}
			}
			break;

			case STEP.END:
			{
				this.step.set_next(STEP.IDLE);
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.START:
				{
					Vector3		player_position = this.player.control.getPosition();

					// -------------------------------------------------------- //
					// 카메라.

					CameraControl.get().beginOuterControll();
					camera_module.pushPosture();

					this.cam_posture0 = camera_module.getPosture();
					this.cam_posture1.position  = player_position + new Vector3(0.0f, 5.3f, -3.5f);
					this.cam_posture1.intererst = player_position + Vector3.forward*2.0f;
					this.cam_posture1.up        = Vector3.up;

					this.camera_fcurve.setSlopeAngle(70.0f, 5.0f);
					this.camera_fcurve.setDuration(0.5f);
					this.camera_fcurve.start();
					
					// 플레이어.
					this.player.beginOuterControll();
					this.player.GetComponent<Rigidbody>().Sleep();

					// 피라미 적 캐릭터의 움직임을  멈춘다.
					LevelControl.get().beginStillEnemies();

					//
					this.step.set_next_delay(STEP.BIKKURI, 1.0f);
				}
				break;

				// "！" 말풍선.
				case STEP.BIKKURI:
				{
					this.bikkuri_spring.k      = 750.0f;
					this.bikkuri_spring.reduce = 0.77f;
					this.bikkuri_spring.start(-2.75f);

					this.sprite_bikkuri.setVisible(true);
				}
				break;

				// 빙글 돌아 모션 재생.
				case STEP.TURN_AND_RISE:
				{
					// 뒤의 부채.

					this.ougi_position   = this.player.control.getPosition() + Vector3.forward*1.0f;
					this.ougi_position.y = ougi_pos_y0;

					this.ougi = EffectRoot.get().createAtariOugi(this.ougi_position);

					this.ougi_fcurve.setSlopeAngle(70.0f, 5.0f);
					this.ougi_fcurve.setDuration(0.5f);
					this.ougi_fcurve.start();

					// 플레이어.
					if(this.player_weapon != null) {

						this.player_weapon.SetActive(false);
					}
					this.player.control.cmdSetMotion("m004_use" , 1);

					// 아이스바 모델.
					this.ice_bar.SetActive(true);

					// 종을 울림.
					SoundManager.getInstance().playSE(Sound.ID.DDG_JINGLE04);

					// 플레이어 가까이 있는 적을 지운다.
					this.hide_enemies = LevelControl.get().getRoomEnemiesInRange(null, this.player.getPosition(), 2.0f);

					foreach(var enemy in this.hide_enemies) {

						enemy.damage_effect.startFadeOut(0.1f);
					}
				}
				break;

				// 아이스 스틱을 던진다.
				case STEP.STICK_THROW:
				{
					// 플레이어.
					this.player.control.cmdSetMotion("m003_attack", 1);
				}
				break;

				// 아이스 스틱이 날라온다.
				case STEP.STICK_FLYING:
				{
					this.cam_posture1 = camera_module.getPosture();

					this.camera_fcurve.setSlopeAngle(10.0f, 5.0f);
					this.camera_fcurve.setDuration(1.0f);
					this.camera_fcurve.start();

					// 당첨 말풍선.
					this.atari_fcurve.setSlopeAngle(50.0f, 30.0f);
					this.atari_fcurve.setDuration(0.5f);
					this.atari_fcurve.start();

					// 뒤 부채.
					this.ougi_fcurve.setSlopeAngle(70.0f, 5.0f);
					this.ougi_fcurve.setDuration(1.0f);
					this.ougi_fcurve.start();

					// 아이스.
					this.ice_fcurve.setSlopeAngle(60.0f, 20.0f);
					this.ice_fcurve.setDuration(1.0f);
					this.ice_fcurve.start();

					this.sprite_ice_bar.setVisible(true);

					// 플레이어가 가진 아이스바(당첨).
					this.ice_bar.gameObject.destroy();
				}
				break;

				case STEP.END:
				{
					GameObject.Destroy(this.ice_bar);
	
					if(this.player_weapon != null) {

						this.player_weapon.SetActive(true);
					}
					this.player.GetComponent<Rigidbody>().WakeUp();
					this.player.endOuterControll();
	
					this.sprite_bikkuri.destroy();
					this.sprite_atari.destroy();
					this.sprite_ice_bar.destroy();
	
					this.ougi.destroy();
	
					camera_module.popPosture();
					CameraControl.get().endOuterControll();
	
					// 피래미 적 캐릭터의 움직임을 재개한다.
					LevelControl.get().endStillEnemies(null, 2.0f);
	
					foreach(var enemy in this.hide_enemies) {
	
						enemy.damage_effect.startFadeIn(0.5f);
					}
				}
				break;

			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// "！" 말풍선.
			case STEP.BIKKURI:
			{
				this.bikkuri_spring.execute(Time.deltaTime);

				Vector2		position = new Vector2(0.0f, 80.0f);

				position += Vector2.up*8.0f*this.bikkuri_spring.position;

				this.sprite_bikkuri.setPosition(position);
			}
			break;

			// 빙글 돌아 모션 재생.
			case STEP.TURN_AND_RISE:
			{
				// -------------------------------------------------------- //
				// 카메라.

				this.camera_fcurve.execute(Time.deltaTime);

				if(this.camera_fcurve.isMoving()) {

					CameraModule.Posture	posture = CameraModule.lerp(this.cam_posture0, this.cam_posture1, this.camera_fcurve.getValue());

					camera_module.setPosture(posture);

				} else {

					float	dolly = camera_module.getDistance();

					camera_module.dolly(dolly*(1.0f - 0.05f*Time.deltaTime));
				}

				// -------------------------------------------------------- //
				// 플레이어.

				this.player.control.cmdSmoothHeadingTo(CameraControl.get().transform.position);

				if(this.player.control.getMotionCurrentTime() > 0.5f) {

					this.player.control.getAnimationPlayer().Stop();
				}

				// -------------------------------------------------------- //
				// 뒤의 부채.
			
				this.ougi_fcurve.execute(Time.deltaTime);
				this.ougi_position.y = Mathf.Lerp(ougi_pos_y0, ougi_pos_y1, this.ougi_fcurve.getValue());

				this.ougi.transform.position = this.ougi_position;

				// -------------------------------------------------------- //
				// 플레이어가 가지고 있는 아이스바(당첨).

				// 카메라 방향으로 향한다.

				Vector3		dir = CameraControl.get().gameObject.getPosition() - this.ice_bar.getPosition();

				this.ice_bar.setRotation(Quaternion.LookRotation(-dir.Y(0.0f), Quaternion.AngleAxis(20.0f, Vector3.forward)*Vector3.up));

				// 줌

				if(this.step.is_acrossing_time(0.3f)) {

					this.ice_zoom_fcurve.setSlopeAngle(70.0f, 5.0f);
					this.ice_zoom_fcurve.setDuration(0.3f);
					this.ice_zoom_fcurve.start();
				}

				this.ice_zoom_fcurve.execute(Time.deltaTime);

				this.ice_bar.setLocalScale(Vector3.one*Mathf.Lerp(0.2f, 0.8f, this.ice_zoom_fcurve.getValue()));

				// -------------------------------------------------------- //
				// 당첨 말풍선.

				if(this.step.is_acrossing_time(0.3f)) {

					this.spring.k      = 750.0f;
					this.spring.reduce = 0.87f;
					this.spring.start(-0.75f);
					this.sprite_atari.setVisible(true);
				}

				this.spring.execute(Time.deltaTime);

				float scale = this.spring.position + 1.0f;

				this.sprite_atari.setScale(new Vector2(scale, scale));
			}
			break;

			// 아이스 스틱을 던진다.
			case STEP.STICK_THROW:
			{
				// -------------------------------------------------------- //
				// カメラ.

				float	dolly = camera_module.getDistance();

				camera_module.dolly(dolly*(1.0f - 0.05f*Time.deltaTime));

				// -------------------------------------------------------- //
				//

				Vector3		dir = CameraControl.get().gameObject.getPosition() - this.player.gameObject.getPosition();

				dir = Quaternion.AngleAxis(-90.0f, Vector3.up)*dir.Y(0.0f);

				this.player.control.cmdSmoothHeadingTo(this.player.gameObject.getPosition() + dir);
			}
			break;

			// 아이스 스틱이 날아온다.
			case STEP.STICK_FLYING:
			{
				// -------------------------------------------------------- //
				// 카메라.

				this.camera_fcurve.execute(Time.deltaTime);

				CameraModule.Posture	posture = CameraModule.lerp(this.cam_posture1, this.cam_posture0, this.camera_fcurve.getValue());

				camera_module.setPosture(posture);

				// -------------------------------------------------------- //
				// 당첨 말풍선.

				this.atari_fcurve.execute(Time.deltaTime);
				this.sprite_atari.setScale(Vector2.one*Mathf.Lerp(1.0f, 0.0f, this.atari_fcurve.getValue()));

				// -------------------------------------------------------- //
				//  뒤 풍선.
			
				this.ougi_fcurve.execute(Time.deltaTime);
				this.ougi_position.y = Mathf.Lerp(ougi_pos_y1, ougi_pos_y0, this.ougi_fcurve.getValue());

				this.ougi.transform.position = this.ougi_position;

				// -------------------------------------------------------- //
				// 이쪽으로 날아오는 아이스 스틱.

				this.ice_fcurve.execute(Time.deltaTime);
				this.tracer.proceedToDistance(this.ice_fcurve.getValue()*this.tracer.curve.calcTotalDistance());

				Vector2		on_curve_position = this.tracer.cv.position.xz()*480.0f/2.0f;

				// 커브의 종단점이 아이템 윈도의 아이콘 위치가 되도록 보정한다.
				do {

					if(this.slot_index < 0) {

						break;
					}
					if(this.ice_fcurve.getValue() <= 0.5f) {

						break;
					}

					float	blend_rate = Mathf.InverseLerp(0.5f, 1.0f, this.ice_fcurve.getValue());

					Vector2		icon_position = ItemWindow.get().getIconPosition(Item.SLOT_TYPE.MISC, this.slot_index);

					Vector2		curve_end = this.tracer.curve.cvs.back().position.xz()*480.0f/2.0f;

					on_curve_position += (icon_position - curve_end)*blend_rate;

				} while(false);

				this.sprite_ice_bar.setPosition(on_curve_position);
				this.sprite_ice_bar.setAngle(this.step.get_time()*360.0f*5.0f);
				this.sprite_ice_bar.getMaterial().SetFloat("_BlendRate", this.ice_fcurve.getValue());
			}
			break;
		}
	}

	public override void	onGUI()
	{
	}

	// 이벤트 실행 중?.
	public override  bool	isInAction()
	{
		bool	ret = !(this.step.get_current() == STEP.IDLE && this.step.get_next() == STEP.NONE);

		return(ret);
	}
}
