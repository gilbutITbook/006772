using UnityEngine;
using System.Collections;

// 비헤이비어의 기저 클래스.
public class chrBehaviorBase : MonoBehaviour {

	public chrController controll = null;

	// ================================================================ //
	// MonoBehaviour로부터 상속.

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

	// 게임 시작할 때 한 번만 호출됩니다.
	public virtual void	start()
	{
	}

	// 프레임마다 호출됩니다.
	public virtual void	execute()
	{
	}

	// 프레임마다 LateUpdate()에서 호출됩니다.
	public virtual void	lateExecute()
	{
	}

	// 정형문을 반환합니다 .
	// 정형문의 말풍선(NPC의 말풍선)을 표시할 때 호출됩니다.
	public virtual string	getPresetText(int text_id)
	{
		return("");
	}

	// 다른 캐릭터에 터치되었을 때（옆에서 클릭）호출됩니다.
	public virtual void		touchedBy(chrController toucher)
	{

	}

	// 외부에서의 컨트롤을 시작합니다.
	public virtual void 	beginOuterControll()
	{
	}

	// 외부에서의 컨트롤을 종료합니다.
	public virtual void		endOuterControll()
	{
	}
}
