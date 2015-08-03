using UnityEngine;
using System.Collections;

// 운반 중인 아이템.
public class ItemCarrier {

	public chrController	character;

	public	ItemController		item  = null;
	public	Vector3				pivot;

	public	float				omega;
	public	float				angle;
	public	float				spin_center;

	public	bool				is_landed;

	public ipModule.Jump		ip_jump = new ipModule.Jump();

	public const float	MIN_OMEGA = 360.0f;			// [degree/sec].
	public const float	ROTATE_RATE = 0.1f*60.0f;
	
	// ================================================================ //

	public ItemCarrier(chrController character)
	{
		this.character = character;
	}

	// 운반시작 연출중?.
	public bool		isInAttachAction()
	{
		return(!this.is_landed);
	}

	// ================================================================ //

	public void		execute()
	{
		do {

			if(this.item == null) {

				break;
			}
			if(this.is_landed) {

				break;
			}

			ItemController	item = this.item;

			this.ip_jump.execute(Time.deltaTime);

			if(!this.ip_jump.isMoving()) {

				this.is_landed = true;
			}

			item.transform.position = this.character.transform.position + this.ip_jump.position;

			// 회전.

			this.angle += this.omega*Time.deltaTime;

			item.transform.rotation = Quaternion.identity;
			item.transform.Translate(this.spin_center*Vector3.up);
			item.transform.Rotate(this.pivot, this.angle);
			item.transform.Translate(-this.spin_center*Vector3.up);

			// 종료 처리.

			if(this.is_landed) {

				item.gameObject.transform.parent      = this.character.gameObject.transform;
				item.gameObject.rigidbody.isKinematic = true;
				item.gameObject.collider.enabled      = true;
			}

		} while(false);

		if(this.is_landed && this.item != null) {

			var q =		Quaternion.FromToRotation(item.transform.forward, this.character.gameObject.transform.forward);

			float	angle;
			Vector3	axis;

			q.ToAngleAxis(out angle, out axis);

			float	min_omega = MIN_OMEGA*Time.deltaTime;

			if(angle <= min_omega) {

				item.transform.rotation = item.transform.rotation;

			} else {

			   	float	rotate_angle = angle*ROTATE_RATE*Time.deltaTime;

				rotate_angle = Mathf.Max(rotate_angle, min_omega);

				q = Quaternion.AngleAxis(rotate_angle, axis);

				item.transform.rotation = q*item.transform.rotation;
			}
		}
	}

	// ================================================================ //

	// 운반 시작.
	public void		beginCarry(ItemController item)
	{
		item.gameObject.rigidbody.isKinematic   = true;
		item.gameObject.collider.enabled        = false;

		Vector3		start = item.transform.position - this.character.transform.position;
		Vector3		goal  = new Vector3(0.0f, chrController.CARRIED_ITEM_HEIGHT, 0.0f);

		this.ip_jump.start(start, goal, chrController.CARRIED_ITEM_HEIGHT + 1.0f);

		this.item = item;

		this.pivot = Quaternion.AngleAxis(90.0f, Vector3.up)*this.ip_jump.xz_velocity();
		this.pivot.Normalize();
		this.omega = 360.0f/(this.ip_jump.t0 + this.ip_jump.t1);
		this.angle = 0.0f;

		this.spin_center = 0.0f;

		switch(this.item.name.ToLower()) {

			case "tarai":	this.spin_center = 0.0f;	break;
			case "negi":	this.spin_center = 0.5f;	break;
			case "yuzu":	this.spin_center = 0.25f;	break;
			case "wan":		this.spin_center = 0.5f;	break;
			case "cat":		this.spin_center = 0.5f;	break;
		}

		this.is_landed = false;
	}

	// 운반 시작(연출은 취소).
	public void		beginCarryAnon(ItemController item)
	{
		this.beginCarry(item);

		this.item.gameObject.transform.parent      = this.character.gameObject.transform;
		this.item.gameObject.rigidbody.isKinematic = true;
		this.item.gameObject.collider.enabled      = true;

		this.item.transform.localPosition = this.ip_jump.goal;
		this.item.transform.rotation      = Quaternion.identity;

		this.is_landed = true;
	}

	// 아이템을 가지고 있는가?.
	public bool		isCarrying()
	{
		return(this.item != null);
	}

	// 운반 중인 아이템을 가져옵니다..
	public ItemController	getItem()
	{
		return(this.item);
	}

	// 운반 종료.
	public void		endCarry()
	{
		this.item = null;
	}

}
