using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 쿼리 결과를 받기 위한 클래스.
public  class QueryBase {

	public QueryBase(string account_id)
	{
		this.account_id = account_id;
	}

	public virtual string	getType()	{ return("null"); }

	public bool		isDone()		{ return(this.is_done); }
	public bool		isSuccess()		{ return(this.is_success); }
	public bool		isExpired()		{ return(this.is_expired); }
	public bool		isNotifyOnly()	{ return(this.is_notify_only);  }

	public void		setNotifyOnly(bool is_notify_only) { this.is_notify_only = is_notify_only; }

	public void		set_done(bool is_done)    	     { this.is_done = is_done; }
	public void		set_success(bool is_success)     { this.is_success = is_success; }
	public void		set_expired(bool is_expired)     { this.is_expired = is_expired; }

	protected bool		is_done    = false;		// 커맨드 실행이 끝났는가?.
	protected bool		is_success = false;		// 성공했나?(pick이면 아이템을 픽업).
	protected bool		is_expired = false;		// 이제 필요 없나?.
	protected bool		is_notify_only = false;	// 통지만   동기화를 기다리지 않는다.

	public string		account_id;				// 이 쿼리를 작성한 사람.
	public float		timer;	
	public float		timeout = 5.0f;			// 이 쿼리의 타임아웃.
};

// 쿼리: 아이템을 주우려고 할 때.
public  class QueryItemPick : QueryBase {

	public QueryItemPick(string acount_id, string target) : base(acount_id)
	{
		this.target = target;
	}

	public override string	getType()	{ return("item.pick"); }

	public string			target  = null;
};

// 쿼리：아이템을 사용했을 때.
public  class QueryItemDrop : QueryBase {

	public QueryItemDrop(string acount_id, string target) : base(acount_id)
	{
		this.target = target;
		this.is_notify_only = true;
	}

	public override string	getType()	{ return("item.drop"); }

	public string			target;
	public bool				is_drop_done;		// true ... 이미 드롭(서버에 통달할 뿐).
};

// 쿼리：몬스터 리스폰.
public  class QuerySpawn : QueryBase {
	
	public QuerySpawn(string acount_id, string monster_id) : base(acount_id)
	{
		this.monster_id = monster_id;
	}
	
	public override string	getType()	{ return("spawn"); }
	
	public string			monster_id;
};

// 쿼리 : 채팅.
public  class QueryTalk : QueryBase {

	public QueryTalk(string acount_id, string words) : base(acount_id)
	{
		this.words = words;
	}

	public override string	getType()	{ return("talk"); }

	public string			words;
};

// 쿼리：무기 선택 씬에서 문으로 들어감.
public  class QuerySelectDone : QueryBase {

	public QuerySelectDone(string acount_id) : base(acount_id)
	{
	}

	public override string	getType()	{ return("select.done"); }
};

// 쿼리：무기선택 씬 끝(전원 문으로 들어갔다).
public  class QuerySelectFinish : QueryBase {

	public QuerySelectFinish(string acount_id) : base(acount_id)
	{
	}

	public override string	getType()	{ return("select.finish"); }
};

// 쿼리：소환수.
public  class QuerySummonBeast : QueryBase {

	public QuerySummonBeast(string acount_id, string type) : base(acount_id)
	{
		this.is_notify_only = true;
		this.type = type;
	}

	public override string	getType()	{ return("summon.beast"); }

	public string	type = "Dog";		// 소환수 타입(개 또는 고양이).
};

// 쿼리：케이크 획득 수.
public  class QueryCakeCount: QueryBase {

	public QueryCakeCount(string acount_id, int count) : base(acount_id)
	{
		this.count = count;
	}

	public override string	getType()	{ return("cake.count"); }

	public int	count = 0;			// 케이크를 획득한 수.
};

// ---------------------------------------------------------------- //

// 쿼리 매니저.
public class QueryManager : MonoBehaviour {

	protected Network 				m_network = null;

	protected List<QueryBase>		queries = new List<QueryBase>();		// 쿼리.

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start() 
	{
		if(dbwin.root().getWindow("query") == null) {

			this.create_debug_window();
		}

		// Network 클래스의 컴포넌트를 획득.
		GameObject	obj = GameObject.Find("Network");
		
		if(obj != null) {

			this.m_network = obj.GetComponent<Network>();
		}
	}
	
	void	Update()
	{
		this.process_query();
	}

	// 쿼리를 등록한다.
	public void	registerQuery(QueryBase query)
	{
		// 케이크 무한 제공 중엔 통신하지 않아도 케이크를 주울 수 있게.
		if(GameRoot.get().isNowCakeBiking()) {

			if(query is QueryItemPick) {

				query.set_done(true);
				query.set_success(true);
			}
		}

		this.queries.Add(query);
	}

	// 완료된 쿼리를 찾는다.
	public List<QueryBase>	findDoneQuery(string account_id)
	{
		List<QueryBase>		done_queries = this.queries.FindAll(x => (x.account_id == account_id && x.isDone()));

		return(done_queries);
	}

	// 특정 형태의 완료된 쿼리를 찾는다.
	public List<QueryBase>	findDoneQuery<T>() where T : QueryBase
	{
		List<QueryBase>		done_queries = this.queries.FindAll(x => ((x as T) != null && x.isDone()));

		return(done_queries);
	}

	// 쿼리를 찾는다.
	public T	findQuery<T>(System.Predicate<T> pred) where T : QueryBase
	{
		T		query = null;

		query = this.queries.Find(x => ((x as T) != null) && pred(x as T)) as T;

		return(query);
	}

	// 쿼리 갱신.
	protected void	process_query()
	{
		// 페일 세이프 & 개발용.
		foreach(var query in this.queries) {

			query.timer += Time.deltaTime;

			if(m_network == null) {

				// GameScene에서 시작했을 때(Title을 거치지 않는다).
				// 네트워크 오브젝트가 만들어지지 않았다.
				query.set_done(true);
				query.set_success(true);

			} else {

				// 타임아웃.
				if(query.timer > query.timeout) {

					query.set_done(true);
					query.set_success(false);
				}
			}
		}

		// 사용이 끝난 쿼리를 삭제한다.
		this.queries.RemoveAll(x => x.isExpired());
	}

	protected void		create_debug_window()
	{

		var		window = dbwin.root().createWindow("query");

		window.createButton("select.done")
			.setOnPress(() =>
			{
				for(int i = 0;i < NetConfig.PLAYER_MAX;i++) {

					var	query = new QuerySelectDone(AccountManager.get().getAccountData(i).account_id);

					QueryManager.get().registerQuery(query);
				}
			});
		window.createButton("select.finish")
			.setOnPress(() =>
			{
				var	query = new QuerySelectFinish("Daizuya");

				QueryManager.get().registerQuery(query);
			});

		window.createButton("summon dog")
			.setOnPress(() =>
			{
				QuerySummonBeast	query_summon = new QuerySummonBeast("Daizuya", "Dog");

				QueryManager.get().registerQuery(query_summon);
			});

		window.createButton("summon neko")
			.setOnPress(() =>
			{
				QuerySummonBeast	query_summon = new QuerySummonBeast("Daizuya", "Neko");

				QueryManager.get().registerQuery(query_summon);
			});

		window.createButton("cake count")
			.setOnPress(() =>
			{
				for(int i = 0;i < PartyControl.get().getFriendCount();i++) {

					chrBehaviorPlayer	friend = PartyControl.get().getFriend(i);

					QueryCakeCount	query_cake = new QueryCakeCount(friend.getAcountID(), (i + 1)*10);

					QueryManager.get().registerQuery(query_cake);
				}
			});

#if false
		window.createButton("버린다")
			.setOnPress(() =>
			{
				chrBehaviorLocal	player = CharacterRoot.get().findCharacter<chrBehaviorLocal>(GameRoot.getInstance().account_name_local);

				player.controll.cmdItemQueryDrop();
			});

		window.createButton("말풍선")
			.setOnPress(() =>
			{
				//chrBehaviorLocal	player = CharacterRoot.get().findCharacter<chrBehaviorLocal>(GameRoot.getInstance().account_name_local);
				chrBehaviorNet	player = CharacterRoot.get().findCharacter<chrBehaviorNet>("Daizuya");

				player.controll.cmdQueryTalk("멀리 있는 사람과 Talk한다", true);
			});

#endif
	}

	// ================================================================ //
	// 인스턴스.

	private	static QueryManager	instance = null;

	public static QueryManager	get()
	{
		if(QueryManager.instance == null) {

			QueryManager.instance = GameObject.Find("GameRoot").GetComponent<QueryManager>();
		}

		return(QueryManager.instance);
	}
}
