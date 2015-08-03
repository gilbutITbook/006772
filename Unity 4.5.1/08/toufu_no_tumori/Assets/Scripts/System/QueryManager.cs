using UnityEngine;
using System.Collections;


// 쿼리 결과를 받아들이는 클래스.
public  class QueryBase {

	public QueryBase()
	{
	}

	public virtual string	getType()	{ return("null"); }

	public bool		isDone()	{ return(this.is_done); }
	public bool		isSuccess() { return(this.is_success); }
	public bool		isExpired() { return(this.is_expired); }

	public void		set_done(bool is_done)    	 { this.is_done = is_done; }
	public void		set_success(bool is_success) { this.is_success = is_success; }
	public void		set_expired(bool is_expired) { this.is_expired = is_expired; }

	protected bool		is_done    = false;		// 커맨드 실행이 끝났는가?.
	protected bool		is_success = false;		// 성공했는가?(pick이면 아이템을 버릴 수 있다).
	protected bool		is_expired = false;		// 더는 필요없는가?.

	public float	timer;				// 테스트용.
};

// 쿼리: 아이템을 주울 때.
public  class QueryItemPick : QueryBase {

	public QueryItemPick(string target)
	{
		this.target = target;
	}

	public override string	getType()	{ return("item.pick"); }

	public string			target  = null;
	public bool				is_anon = false;	// 연출을 커트?.
};

// 쿼리：아이템을 버릴 때.
public  class QueryItemDrop : QueryBase {

	public QueryItemDrop(string target)
	{
		this.target = target;
		this.is_drop_done = false;
	}

	public override string	getType()	{ return("item.drop"); }

	public string			target;
	public bool				is_drop_done;		// true ... 이미 드롭(서버에 도달하기만).
};

// 쿼리：이사 시작.
public  class QueryHouseMoveStart : QueryBase {

	public QueryHouseMoveStart(string target)
	{
		this.target = target;
	}

	public override string	getType()	{ return("house-move.start"); }

	public string			target;
};

// 쿼리：이사 끝.
public  class QueryHouseMoveEnd : QueryBase {

	public QueryHouseMoveEnd()
	{
	}

	public override string	getType()	{ return("house-move.end"); }

};

// 쿼리 : 채팅.
public  class QueryTalk : QueryBase {

	public QueryTalk(string words)
	{
		this.words = words;
	}

	public override string	getType()	{ return("talk"); }

	public string			words;
};

// ---------------------------------------------------------------- //

// 특별히 할 게 없을지도?.
public class QueryManager : MonoBehaviour {

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start() 
	{
		if(dbwin.root().getWindow("query") == null) {

			this.create_debug_window();
		}
	}
	
	void	Update()
	{
	}

	protected void		create_debug_window()
	{
		var		window = dbwin.root().createWindow("query");

		window.createButton("줍는다")
			.setOnPress(() =>
			{
				chrBehaviorLocal	player = CharacterRoot.get().findCharacter<chrBehaviorLocal>(GameRoot.getInstance().account_name_local);

				player.controll.cmdItemQueryPick("Tarai");
			});

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

		window.createButton("이사 시작")
			.setOnPress(() =>
			{
				chrBehaviorLocal	player = CharacterRoot.get().findCharacter<chrBehaviorLocal>(GameRoot.getInstance().account_name_local);
				//chrBehaviorNet		player = CharacterRoot.get().findCharacter<chrBehaviorNet>(GameRoot.getInstance().account_name_net);

				player.controll.cmdQueryHouseMoveStart("House1");
			});

		window.createButton("이사 끝")
			.setOnPress(() =>
			{
				chrBehaviorLocal	player = CharacterRoot.get().findCharacter<chrBehaviorLocal>(GameRoot.getInstance().account_name_local);
				//chrBehaviorNet		player = CharacterRoot.get().findCharacter<chrBehaviorNet>(GameRoot.getInstance().account_name_net);

				player.controll.cmdQueryHouseMoveEnd();
			});
	}

	// ================================================================ //
	// 인스턴스.

	private	static QueryManager	instance = null;

	public static QueryManager	getInstance()
	{
		if(QueryManager.instance == null) {

			QueryManager.instance = GameObject.Find("GameRoot").GetComponent<QueryManager>();
		}

		return(QueryManager.instance);
	}
}
