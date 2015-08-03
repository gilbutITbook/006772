using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 이벤트의 베이스 클래스.
public class EventBase {

	protected GameObject	data_holder;

	// ================================================================ //

	public EventBase() {}

	public virtual void		initialize() {}
	public virtual void		start() {}
	public virtual void		execute() {}
	public virtual void		end() {}
	public virtual void		onGUI() {}

	// 이벤트 실행 중?.
	public virtual bool		isInAction()
	{
		return(false);
	}

	protected Vector3	get_locator_position(string locator)
	{
		Vector3		pos = this.data_holder.transform.FindChild(locator).position;

		return(pos);
	}

};

// 이벤트 관리.
public class EventRoot : MonoBehaviour {

	protected EventBase		next_event    = null;		// 실행할 이벤트.
	protected EventBase		current_event = null;		// 실행 중인 이벤트.

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
	}
	
	void	Update()
	{
		// 이벤트 종료.
		if(this.current_event != null) {

			if(!this.current_event.isInAction()) {

				this.current_event.end();
				this.current_event = null;
			}
		}

		// 이벤트 시작.
		if(this.current_event == null) {

			if(this.next_event != null) {
	
				this.current_event = this.next_event;
				this.current_event.initialize();
				this.current_event.start();
	
				this.next_event = null;
			}
		}

		// 이벤트 실행.
		if(this.current_event != null) {

			this.current_event.execute();
		}
	}

	void	OnGUI()
	{
		if(this.current_event != null) {

			this.current_event.onGUI();
		}
	}

	// ================================================================ //

	// 이벤트를 시작한다.
	public T		startEvent<T>() where T : EventBase, new()
	{
		T	new_event = null;

		if(this.next_event == null) {

			new_event       = new T();
			this.next_event = new_event;
		}

		return(new_event);
	}
	public EventBase	getCurrentEvent()
	{
		return(this.current_event);
	}
	public T	getCurrentEvent<T>() where T : EventBase
	{
		T	ev = null;

		if(this.current_event != null) {

			ev = this.current_event as T;
		}

		return(ev);
	}

#if false
	public T		getEventData<T>() where T : MonoBehaviour
	{
		T	event_data = this.gameObject.GetComponent<T>();

		return(event_data);
	}
#endif
#if false
	// 이벤트 박스를 전부 액티브로 한다.
	public void		activateEventBoxAll()
	{

		 EventBoxLeave[]	boxes = this.getEventBoxes();

		foreach(var box in boxes) {

			box.activate();
		}

	}
#endif
#if false
	// 이벤트 박스를 전부 슬립으로 한다.
	public void		deactivateEventBoxAll()
	{

		 EventBoxLeave[]	boxes = this.getEventBoxes();

		foreach(var box in boxes) {

			box.deactivate();
		}

	}
#endif

	// ================================================================ //
	// 인스턴스.

	private	static EventRoot	instance = null;

	public static EventRoot	get()
	{
		if(EventRoot.instance == null) {

			EventRoot.instance = GameObject.Find("EventRoot").GetComponent<EventRoot>();
		}

		return(EventRoot.instance);
	}
}

