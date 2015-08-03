using UnityEngine;
using System.Collections;

public class AcountData {

	public string	account_id;			// 어카운트 이름. 이번에는 캐릭터 이름을 고정("Toufuya" 등).
	public int		global_index;		// 모든 단말을 통해 고유한 인덱스.
	public int		local_index;		// 단말 내에서의 인덱스. 로컬 플레이어가 0. 단말마다 다릅니다.

	public string	avator_id;			// 아바타 이름("Toufuya" 등). account_id와 같습니다. 
	public string	label;				// 한글표기.

	public Color	favorite_color;		// 마음에 드는 색.
}

public class AcountManager : MonoBehaviour {

	protected AcountData[]	account_datas = null;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Start()
	{
		this.account_datas = new AcountData[4];

		for(int i = 0;i < 4;i++) {

			this.account_datas[i] = new AcountData();
			this.account_datas[i].global_index = i;

			// 실제로 플레어가 단말에 접속했을 때 결정합니다.
			this.account_datas[i].local_index  = -1;
		}

		this.account_datas[0].account_id = "Toufuya";
		this.account_datas[0].avator_id = this.account_datas[0].account_id;
		this.account_datas[0].label     = "두부장수";
		this.account_datas[0].favorite_color = Color.cyan;

		this.account_datas[1].account_id = "Daizuya";
		this.account_datas[1].avator_id = this.account_datas[1].account_id;
		this.account_datas[1].label     = "콩장수";
		this.account_datas[1].favorite_color = Color.green;

		this.account_datas[2].account_id = "Zundaya";
		this.account_datas[2].avator_id = this.account_datas[2].account_id;
		this.account_datas[2].label     = "풋콩장수";
		this.account_datas[2].favorite_color = Color.cyan;

		this.account_datas[3].account_id = "Irimameya";
		this.account_datas[3].avator_id = this.account_datas[3].account_id;
		this.account_datas[3].label     = "볶은콩장수";
		this.account_datas[3].favorite_color = Color.green;
	}
	
	void	Update()
	{
	}

	// ================================================================ //

	public AcountData		getAccountData(int global_index)
	{
		return(this.account_datas[global_index]);
	}

	public AcountData		getAccountData(string account_id)
	{
		foreach (AcountData account in this.account_datas) {
			if (account.account_id == account_id) {
				return account;
			}
		}

		return this.account_datas[0];
	}

	// ================================================================ //

	private	static AcountManager	instance = null;

	public static AcountManager	getInstance()
	{
		if(AcountManager.instance == null) {

			AcountManager.instance = GameObject.Find("GameRoot").GetComponent<AcountManager>();
		}

		return(AcountManager.instance);
	}

	public static AcountManager	get()
	{
		return(AcountManager.getInstance());
	}

}

