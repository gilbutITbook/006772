using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossLevelSequenceResult : SequenceBase {

	public enum STEP {

		NONE = -1,

		IDLE = 0,
		WAIT_FRIEND,		// 리모트 플레이어로부터 결과 수신대기.
		DISP_RESULT,		// 결과 표시.
		FADE_OUT_RESULT,	// 결과 표시 페이드 아웃.
		DISP_OSIMAI,		// 끝 표시.

		FINISH,

		NUM,
	};
	public Step<STEP>	step = new Step<STEP>(STEP.NONE);

	// ---------------------------------------------------------------- //

	public int[]	cake_counts;				// 각 플레이어가 케이크를 획득한 수.

	//===================================================================
	
	// 통신모듈.
	protected Network			m_network;
	
	// ================================================================ //

	// 디버그 창 생성 시 호출.
	public override void		createDebugWindow(dbwin.Window window)
	{
	}

	// 레벨 시작 시에 호출.
	public override void		start()
	{
		// 각 플레이어가 케이크를 획득한 수..

		this.cake_counts = new int[NetConfig.PLAYER_MAX];

		for(int i = 0;i < this.cake_counts.Length;i++) {

			if(GameRoot.get().isConnected(i)) {

				this.cake_counts[i] = -1;

			} else {

				this.cake_counts[i] = 0;
			}
		}

		chrBehaviorLocal	local_player = PartyControl.get().getLocalPlayer();

		this.cake_counts[local_player.getGlobalIndex()] = local_player.getCakeCount();

		QueryCakeCount	query_cake = new QueryCakeCount(local_player.getAcountID(), local_player.getCakeCount());

		query_cake.timeout = 20.0f;

		QueryManager.get().registerQuery(query_cake);

		// Network 클래스의 컴포넌트 획득.
		GameObject	obj = GameObject.Find("Network");
		
		if(obj != null) {
			
			this.m_network = obj.GetComponent<Network>();
			
			if (this.m_network != null) {
				this.m_network.RegisterReceiveNotification(PacketId.PrizeResult, OnReceivePrizeResultPacket);
			}
		}

		// 케이크 데이터 송신.
		sendPrizeData();

		this.step.set_next(STEP.WAIT_FRIEND);
	}

	protected class RankData {

		public int		rank;
		public int		account_global_index;
		public int		count;
	}
	protected List<RankData>	ranks;

	protected List<YellDisp>	rank_disps;

	// 매 프레임 호출.
	public override void		execute()
	{
		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크.

		switch(this.step.do_transition()) {

			// 리모트 플레이어로부터의 결과 수신 대기.
			case STEP.WAIT_FRIEND:
			{
				do {

					// 타임업 표시가 사라질 때까지 대기.
					if(Navi.get().getYell() != null) {

						break;
					}

					if(System.Array.Exists(this.cake_counts, x => x < 0)) {

						break;
					}
					this.step.set_next(STEP.DISP_RESULT);

				} while(false);
			}
			break;

			// 결과 표시.
			case STEP.DISP_RESULT:
			{
				do {

					// 랭킹이 전부 표시될 때까지 대기.
					if(this.rank_disps.Exists(x => x == null)) {

						break;
					}
					if(this.rank_disps.Exists(x => x.step.get_current() != YellDisp.STEP.STAY)) {

						break;
					}

					if(!Input.GetMouseButtonDown(0)) {

						break;
					}

					this.step.set_next(STEP.FADE_OUT_RESULT);

				} while(false);
			}
			break;

			// 결과 표시 페이드 아웃.
			case STEP.FADE_OUT_RESULT:
			{
				do {

					// 랭킹이 전부 페이드 아웃할 때까지 대기.
					if(this.rank_disps.Exists(x => x != null)) {

						break;
					}
					this.step.set_next(STEP.DISP_OSIMAI);

				} while(false);
			}
			break;

			// 끝 표시.
			case STEP.DISP_OSIMAI:
			{
				do {

					if(Navi.get().getYell() == null) {

						break;
					}
					if(Navi.get().getYell().step.get_current() != YellDisp.STEP.STAY) {

						break;
					}
					if(!Input.GetMouseButtonDown(0)) {

						break;
					}

					this.step.set_next(STEP.FINISH);

				} while(false);
			}
			break;
		}
				
		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.DISP_RESULT:
				{
					Navi.get().getCakeTimer().destroy();

					this.ranks      = new List<RankData>();
					this.rank_disps = new List<YellDisp>();

					for(int i = 0;i < NetConfig.PLAYER_MAX;i++) {

						/*if(!GameRoot.get().isConnected(i)) {

							continue;
						}*/

						RankData	rank_data = new RankData();

						rank_data.rank = -1;
						rank_data.account_global_index = i;
						rank_data.count = this.cake_counts[i];

						this.ranks.Add(rank_data);
						this.rank_disps.Add(null);

						Debug.Log("Result Cake num[" + i + "]:" + rank_data.count);
					}
					
					if (m_network == null) {

						this.ranks[0].count = 10;
						this.ranks[1].count = 20;
						this.ranks[2].count = 30;
						this.ranks[3].count = 10;
					}

					// 케이크를 획득한 수가 많은 순(같으면 글로벌 인덱스가 작은 순)으로 정렬.
					this.ranks.Sort((x, y) => (x.count != y.count) ? y.count - x.count : x.account_global_index - y.account_global_index);

					// 순위를 매긴다(같은 수에 주의 하면서).
					for(int i = 0;i < this.ranks.Count;i++) {

						if(i == 0) {

							this.ranks[i].rank = i;

						} else {

							if(this.ranks[i].count == this.ranks[i - 1].count) {

								this.ranks[i].rank = this.ranks[i - 1].rank;

							} else {

								this.ranks[i].rank = i;
							}
						}
					}

				}
				break;

				// 결과 표시 페이드 아웃.
				case STEP.FADE_OUT_RESULT:
				{
					foreach(var rank_disp in this.rank_disps) {

						rank_disp.beginFadeOut();
					}
				}
				break;

				// 끝.
				case STEP.DISP_OSIMAI:
				{
					Navi.get().dispatchYell(YELL_WORD.OSIMAI);
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			// 결과 표시.
			case STEP.DISP_RESULT:
			{
				for(int i = 0;i < this.ranks.Count;i++) {

					if(this.rank_disps[i] != null) {

						continue;
					}

					if(i == 0) {

						if(this.step.get_time() < (this.ranks.Count - 2)*1.0f + 2.0f) {

							continue;
						}

					} else {

						if(this.step.get_time() < (this.ranks.Count - i - 1)*1.0f) {

							continue;
						}
					}

					RankData	rank_data = this.ranks[i];

					this.rank_disps[i] = Navi.get().createCakeCount(rank_data.rank + 1, rank_data.account_global_index, rank_data.count);

					this.rank_disps[i].setPosition(new Vector3(0.0f, 150.0f - i*80.0f, 0.0f));
				}
			}
			break;

			// 끝.
			case STEP.DISP_OSIMAI:
			{
			}
			break;
		}

		this.update_queries();

		// ---------------------------------------------------------------- //

	}

	public override bool	isFinished()
	{
		return(this.step.get_current() == STEP.FINISH);
	}

	// ---------------------------------------------------------------- //
	// 쿼리를 갱신.

	protected void	update_queries()
	{
		List<QueryBase>		done_queries = QueryManager.get().findDoneQuery<QueryCakeCount>();

		foreach(var query in done_queries) {

			QueryCakeCount	query_cake = query as QueryCakeCount;

			if(query_cake == null) {

				continue;
			}

			int		global_index = AccountManager.get().accountID_to_GlobalIndex(query_cake.account_id);

			if(global_index < 0) {

				continue;
			}

			this.cake_counts[global_index] = query_cake.count;

			query.set_expired(true);
		}
	}

	// ---------------------------------------------------------------- //
	// 케이트 데이터 송수신.

	private void sendPrizeData()
	{
		PrizeData data = new PrizeData ();
		
		chrBehaviorLocal	local_player = PartyControl.get().getLocalPlayer();
		
		Debug.Log("[CLIENT] sendPrizeData");
		
		// 획득한 케이크 수를 설정.
		data.characterId = local_player.getAcountID();
		data.cakeNum = local_player.getCakeCount();
		
		if (this.m_network != null) {
			PrizePacket packet = new PrizePacket (data);
			
			int serverNode = this.m_network.GetServerNode();
			this.m_network.SendReliable<PrizeData>(serverNode, packet);
			
			Debug.Log("[CLIENT] send cake num[" + data.characterId + "]:" + data.cakeNum);
		}
	}
	
	public void OnReceivePrizeResultPacket(int node, PacketId id, byte[] data)
	{
		PrizeResultPacket packet = new PrizeResultPacket(data);
		PrizeResultData result = packet.GetPacket();
		
		Debug.Log("[CLIENT] ReceivePrizeResultPacket");
		
		for (int i = 0; i < result.cakeDataNum; ++i) {
			
			this.cake_counts[i] = result.cakeNum[i];
			
			Debug.Log("[CLIENT] Cake num[" + i + "]:" + result.cakeNum[i]);
		}
		
		chrBehaviorLocal local_player = PartyControl.get().getLocalPlayer();
		
		QueryCakeCount	query = QueryManager.get().findQuery<QueryCakeCount>(x => x.account_id == local_player.getAcountID());
		
		if (query != null) {
			Debug.Log("[CLIENT]QueryCakeCount done");
			query.set_done(true);
			query.set_success(true);
		}
	}
}
