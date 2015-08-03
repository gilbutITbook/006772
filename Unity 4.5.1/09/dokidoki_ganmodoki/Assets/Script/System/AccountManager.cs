using UnityEngine;
using System.Collections;

public class AccountData {

	public string	account_id;			// 어카운트 이름. 이번은 캐릭터 이름으로 고정.("Toufuya" 등).
	public int		global_index;		// 모든 단말에서 고유한 인덱스.
	public int		local_index;		// 단말 내에서의 인덱스. 로컬 플레이어가 0.단말 별로 다르다.

	public string	avator_id;			// 아바타 이름("Toufuya" 등). account_id와 같다.
	public string	label;				// 라벨.

	public Color	favorite_color;		// 마음에 드는 색..
}

public class AccountManager : MonoBehaviour {

	protected AccountData[]	account_datas = null;

	// ================================================================ //
	// MonoBehaviour에서 상속.

	void	Awake()
	{
		this.account_datas = new AccountData[4];

		for(int i = 0;i < 4;i++) {

			this.account_datas[i] = new AccountData();
			this.account_datas[i].global_index = i;

			// 실제로 플레이어가 단말에 접속했을 때 결정한다.
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
	
	void	Start()
	{
	}

	void	Update()
	{
	}

	// ================================================================ //

	// 글로벌 グローバルインデックスでアカウントデーターを探す.
	public AccountData		getAccountData(int global_index)
	{
		return(this.account_datas[global_index]);
	}

	// 어카운트 이름으로 어카운트 데이터를 찾는다.
	public AccountData		getAccountData(string account_id)
	{
		foreach (AccountData account in this.account_datas) {
			if (account.account_id == account_id) {
				return account;
			}
		}

		return this.account_datas[0];
	}

	// 어카운트 이름으로 글로벌 인덱스를 얻는다.
	public int		accountID_to_GlobalIndex(string account_id)
	{
		int		global_index = -1;

		AccountData		account = this.getAccountData(account_id);

		if(account != null) {

			global_index = account.global_index;
		}

		return(global_index);
	}

	// ================================================================ //

	private	static AccountManager	instance = null;

	public static AccountManager	getInstance()
	{
		if(AccountManager.instance == null) {

			AccountManager.instance = GameObject.Find("GameRoot").GetComponent<AccountManager>();
		}

		return(AccountManager.instance);
	}

	public static AccountManager	get()
	{
		return(AccountManager.getInstance());
	}

}

