using UnityEngine;
using System.Collections;

// 애니메이션이 붙은 노드의  트랜스폼을 강제로 변경합니다.
public class TransformModifier : MonoBehaviour {

	protected struct Mask {

		public bool	x;
		public bool	y;
		public bool	z;
	};

	protected Vector3		initial_position;
	protected Quaternion	initial_rotation;

	protected bool		is_control = false;
	protected Mask		write_mask;

	protected Vector3		position;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
		this.initial_position = this.transform.position;
		this.initial_rotation = this.transform.rotation;

		this.position = this.initial_position;

		this.write_mask.x = true;
		this.write_mask.y = true;
		this.write_mask.z = true;
	}

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	void	LateUpdate()
	{
		if(this.is_control) {

			Vector3		new_position = this.transform.position;

			if(this.write_mask.x) {

				new_position.x = this.position.x;
			}
			if(this.write_mask.y) {

				new_position.y = this.position.y;
			}
			if(this.write_mask.z) {

				new_position.z = this.position.z;
			}
			this.transform.position = new_position;
		}
	}

	// ================================================================ //

	public void setControl(bool is_control)
	{
		this.is_control = is_control;
	}

	public void setWriteMask(string mask)
	{
		this.write_mask.x = false;
		this.write_mask.y = false;
		this.write_mask.z = false;

		foreach(char c in mask) {

			switch(c) {

				case 'x':	this.write_mask.x = true;	break;
				case 'y':	this.write_mask.y = true;	break;
				case 'z':	this.write_mask.z = true;	break;
			}
		}
	}

	public void	setPosition(Vector3 position)
	{
		this.position = position;
	}

	public Vector3	getInitialPosition()
	{
		return(this.initial_position);
	}

	public void reset()
	{
		this.transform.position = this.initial_position;
		this.transform.rotation = this.initial_rotation;
	}

}
