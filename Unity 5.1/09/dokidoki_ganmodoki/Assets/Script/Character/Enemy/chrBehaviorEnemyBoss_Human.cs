using UnityEngine;
using System.Collections;

// 개발/디버그용으로 사람이 컨트롤 할 수 있게 한 것.
public class chrBehaviorEnemyBoss_Human : chrBehaviorEnemyBoss {
	public override	void	execute()
	{
		base.execute ();
		
		// 스페이스바나 J키로 점프 공격
		if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.J))
		{
			EnemyRoot.getInstance().RequestBossRangeAttack(1.0f, 5.0f);
		}
		
		// C키로 공격
		if (Input.GetKey(KeyCode.C))
		{
			// 돌격
            EnemyRoot.getInstance().RequestBossDirectAttack(focus.getAcountID(), 3.0f);
		}

		// Q키로 퀵 공격
		if(Input.GetKey(KeyCode.Q)) {

			// 퀵 공격
            EnemyRoot.getInstance().RequestBossQuickAttack(focus.getAcountID(), 1.0f);
		}
	}
	
	// [개발용] 최신 파티 정보를 가져와서 플레이어 목록을 갱신한다.
	public override sealed void updateTargetPlayers()
	{
		initializeTargetPlayers();
	}
}
