using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollisionResult {

	public GameObject	object0 = null;
	public GameObject	object1 = null;
	public bool			is_trigger = false;
	public object		option0 = null;
}

public class CollisionManager : MonoBehaviour {

	public List<CollisionResult>	results = new List<CollisionResult>();

	protected bool	collision_updated = false;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		// '확실하게 컬리전이 계산됐음'을 검출하기 위한 더미 컬리전.
		//
		// 더미 컬리전은 CollisionManager와 반드시 충돌이 발생하는 위치에 두므로.
		//'충돌 발생' = '씬 내의 컬리전 계산이 일주했다'고 간주할 수 있다.

		GameObject	go = new GameObject("DummyCollision");

		go.gameObject.layer = this.gameObject.layer;

		go.AddComponent<SphereCollider>().isTrigger = true;
	}
	
	void	LateUpdate()
	{
	}

	void	FixedUpdate()
	{
		// OnCollisionStay가 호출되는 간격은 Update() 간격보다 길다.
		// 확실하게 컬리전 계산이 이루어진 타이밍에서 결과를 클리어한다.
		if(this.collision_updated) {

			this.clearResults();

			this.collision_updated = false;
		}
	}

	// 트리거에 히트하는 동안 호출되는 메소드.
	void	OnTriggerStay(Collider other)
	{
		this.collision_updated = true;
	}

	// ================================================================ //

	public void		clearResults()
	{
		this.results.Clear();
	}

	public void		removeResult(CollisionResult result)
	{
		this.results.Remove(result);
	}

	// ================================================================ //
	// 인스턴스.

	private	static CollisionManager	instance = null;

	public static CollisionManager	getInstance()
	{
		if(CollisionManager.instance == null) {

			CollisionManager.instance = GameObject.Find("CollisionManager").GetComponent<CollisionManager>();
		}

		return(CollisionManager.instance);
	}
}
