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
	public virtual void		onGUI() {}

	// 이벤트가 실행 중인가?.
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

	protected EventBase		next_event    = null;		// 실행 시작할 이벤트.
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
	
				this.current_event = null;

				this.activateEventBoxAll();
			}
		}

		// 이벤트 시작.
		if(this.current_event == null) {

			if(this.next_event != null) {
	
				this.current_event = this.next_event;
				this.current_event.initialize();
				this.current_event.start();
	
				this.next_event = null;
	
				this.deactivateEventBoxAll();
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

	// 이벤트를 시작합니다.
	public T		startEvent<T>() where T : EventBase, new()
	{
		T	new_event = null;

		if(this.next_event == null) {

			new_event       = new T();
			this.next_event = new_event;
		}

		return(new_event);
	}

	// 이벤트 박스를 전부 활성화합니다.
	public void		activateEventBoxAll()
	{
		 EventBoxLeave[]	boxes = this.getEventBoxes();

		foreach(var box in boxes) {

			box.activate();
		}
	}

	// 이벤트 박스를 전부 비활성화합니다..
	public void		deactivateEventBoxAll()
	{
		 EventBoxLeave[]	boxes = this.getEventBoxes();

		foreach(var box in boxes) {

			box.deactivate();
		}
	}
	
	// 이벤트 박스를 전부 가져옵니다.
	public EventBoxLeave[]	getEventBoxes()
	{
		GameObject[]	gos = GameObject.FindGameObjectsWithTag("EventBox");

		gos = System.Array.FindAll(gos, x => x.GetComponent<EventBoxLeave>() != null);

		EventBoxLeave[]	boxes= new EventBoxLeave[gos.Length];

		foreach(var i in System.Linq.Enumerable.Range(0, gos.Length)) {

			boxes[i] = gos[i].GetComponent<EventBoxLeave>();
		}

		return(boxes);
	}

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

