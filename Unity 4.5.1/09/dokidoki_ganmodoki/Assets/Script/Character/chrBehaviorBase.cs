using UnityEngine;
using System.Collections;

// 비헤이비어의 기저 클래스.
public class chrBehaviorBase : MonoBehaviour {

	public chrController control = null;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	// 생성 직후에 호출됩니다.
	public virtual void	initialize()
	{
	}

	// 게임 시작 시에 한 번만 호출됩니다.
	public virtual void	start()
	{
	}

	// 매 프레임 호출됩니다.
	public virtual void	execute()
	{
	}

	// 프리셋 텍스트를 반환합니다.
	// 프리셋 텍스트의 말풍선(NPC의 말풍선)을 표시할 때 호출됩니다.
	public virtual string	getPresetText(int text_id)
	{
		return("");
	}

	// 다른 캐릭터에 터치된(옆에서 클릭) 때 호출됩니다.
	public virtual void		touchedBy(chrController toucher)
	{

	}

	// 근접공격이 히트했을 때 호출됩니다.
	public virtual void		onMeleeAttackHitted(chrBehaviorBase other)
	{
	}

	// 대미지를 받았을 때 호출됩니다.
	public virtual void		onDamaged()
	{
	}

	// 당했을 때 호출됩니다.
	public virtual void		onVanished()
	{
	}

	// 삭제하기 직전에 호출됩니다.
	public virtual void		onDelete()
	{
	}

	// ================================================================ //

	// GameObject에 추가된 비헤이비어를 가져옵니다.
	public static T getBehaviorFromGameObject<T>(GameObject go) where T : chrBehaviorBase
	{
		T	behavior = null;

		do {

			chrController	control = go.GetComponent<chrController>();

			if(control == null) {

				continue;
			}

			behavior = control.behavior as T;

			if(behavior == null) {

				continue;
			}

		} while(false);

		return(behavior);
	}
}
