using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Item {


public enum CATEGORY {

	NONE = -1,

	KEY = 0,		// 문 열쇠.
	FLOOR_KEY,		// 플로어 이동문 열쇠
	SODA_ICE,		// 소다 아이스      사용하면 체력 회복.
	CANDY,			// 캔디.
	FOOD,			// 먹을 거    즉석에서 체력회복.
	WEAPON,			// 무기.
	ETC,			// 기타.

	NUM,
};

// Slot の ID
public enum SLOT_TYPE {

	NONE = -1,

	KEY = 0,			// 방 열쇠.
	FLOOR_KEY,			// 플로어 열쇠.
	CANDY,				// 캔디.
	MISC,				// 범용.

	NUM,
};

// 아이템을 갖고 있을 때 캐릭터에 부여되는 특혜.
public class Favor {

	public Favor()
	{
		this.option0 = (object)"";
	}

	public Favor	clone()
	{
		return(this.MemberwiseClone() as Favor);
	}

	public CATEGORY	category = CATEGORY.NONE;

	public object	option0;		// 옵션 파라미터   제1 (아이스 당첨 / 꽝 등).
};

// 캐릭터가 아이템을 가질 수 있는 곳.
public class Slot {

	public string			item_id  = "";
	public Favor			favor    = null;		// 아이템 효과.
	public bool				is_using = false;		// 사용 중?.

	public Slot()
	{
		this.initialize();
	}

	// 초기화.
	public void	initialize()
	{
		this.item_id = "";
		this.favor = null;
		this.is_using = false;
	}

	// 비었는가?.
	public bool isVacant()
	{
		return(this.favor == null);
	}
};
	
// 한 캐릭터가 가질 수 있는 아이템.
public class SlotArray {

	public const int	MISC_NUM = 2;

	public Slot			candy;		// 캔디.
	public List<Slot>	miscs;		// 범용.

	public SlotArray()
	{
		this.candy = new Slot();
		this.miscs = new List<Slot>();

		for(int i = 0;i < MISC_NUM;i++) {

			this.miscs.Add(new Slot());
		}
	}

	// 범용 빈 슬롯을 찾는다.
	public int	getEmptyMiscSlot()
	{
		int		slot_index = this.miscs.FindIndex(x => x.isVacant());

		return(slot_index);
	}
};

// 열쇠 색상..
public enum KEY_COLOR {

	NONE = -1,

	PINK = 0,		// "key00"
	YELLOW,			// "key01"
	GREEN,			// "key02"
	BLUE,			// "key03"

	PURPLE,			// "key04" 플로어 열쇠.

	NUM,
};

// 열쇠용 메소드.
public class Key {

	// 열쇠 타입명(플레이어 이름)을 얻는다.
	public static string	getTypeName(KEY_COLOR color)
	{
		string	type_name = "key" + ((int)color).ToString("D2");

		return(type_name);
	}

	// 타입명으로 열쇠 컬러를 가져온다.
	public static KEY_COLOR	getColorFromTypeName(string type_name)
	{
		KEY_COLOR	color = KEY_COLOR.NONE;

		do {

			if(!type_name.StartsWith("key")) {

				break;
			}

			string	color_string = type_name.Substring(3, 2);
			int		color_int;

			if(!int.TryParse(color_string, out color_int)) {

				break;
			}

			color = (KEY_COLOR)color_int;

		} while(false);

		return(color);
	}

	// 열쇠의 인스턴스 이름 얻기.
	public static string	getInstanceName(KEY_COLOR color, Map.RoomIndex room_index)
	{
		string	instance_name = Key.getTypeName(color) + "_" + room_index.x.ToString() + room_index.z.ToString();

		return(instance_name);
	}

	// 인스턴스 이름으로 열쇠 색 얻기.
	public static KEY_COLOR	getColorFromInstanceName(string name)
	{
		return(Key.getColorFromTypeName(name));
	}
}

// 무기 체인지 아이템용 메소드.
public class Weapon {

	// 아이템 이름 앞에서 타입을 가져온다.
	public static SHOT_TYPE		getShotType(string name)
	{
		SHOT_TYPE	shot_type = SHOT_TYPE.NONE;

		do {
			char[] delimiterChars = { '_', '.' };
				string[] item_name = name.Split(delimiterChars);

			if(item_name.Length < 3) {

				break;
			}
			if(item_name[0] != "shot") {

				break;
			}

			switch(item_name[1]) {
	
				case "negi":
				{
					shot_type = SHOT_TYPE.NEGI;
				}
				break;
	
				case "yuzu":
				{
					shot_type = SHOT_TYPE.YUZU;
				}
				break;
			}

		} while(false);

		return(shot_type);
	}
}

} // namespace Item {

public class ItemDef : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
