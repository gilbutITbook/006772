using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 디버그 문자를 화면에 표시하는 클래스.
// OnGUI() 이외에서도 디버그 프린트를 사용할 수 있습니다.
public class dbPrint : MonoBehaviour {

	private static dbPrint	instance = null;

	public struct TextItem {

		public int		x, y;
		public string	text;
		public float	lifetime;
	};

	private List<TextItem>	items;
	private int				locate_x, locate_y;

	private static int		CHARA_W = 20;
	private static int		CHARA_H = 20;


	// ------------------------------------------------------------------------ //

	public static dbPrint	getInstance()
	{
		if(dbPrint.instance == null) {

			GameObject	go = new GameObject("DebugPrint");

			dbPrint.instance = go.AddComponent<dbPrint>();
			dbPrint.instance.create();

			DontDestroyOnLoad(go);
		}

		return(dbPrint.instance);
	}

	// 텍스트를 표시합니다.
	public static void	print(string text, float lifetime)
	{
		dbPrint	dp = dbPrint.getInstance();

		dp.add_text(text, lifetime);
	}

	// 표시 위치를 설정합니다.
	public static void	setLocate(int x, int y)
	{
		dbPrint	dp = dbPrint.getInstance();

		dp.set_locate(x, y);
	}

	// ------------------------------------------------------------------------ //

	void Start ()
	{
		this.clear();
	}
	
	void Update ()
	{

	}

	void OnGUI()
	{
		// 버퍼에 차있는 텍스트를 표시합니다.

		int		x, y;

		foreach(var item in this.items) {

			x = item.x*dbPrint.CHARA_W;
			y = item.y*dbPrint.CHARA_H;

			GUI.Label(new Rect(x, y, item.text.Length*dbPrint.CHARA_W, dbPrint.CHARA_H), item.text);

			y += dbPrint.CHARA_H;
		}

		// 버퍼를 클리어합니다.	

		if(UnityEngine.Event.current.type == UnityEngine.EventType.Repaint) {

			this.clear();
		}
	}

	public void	create()
	{
		this.items = new List<TextItem>();
	}

	// 버퍼를 클리어합니다..
	private void	clear()
	{
		this.locate_x = 10;
		this.locate_y = 10;

		for(int i = 0;i < this.items.Count;i++) {

			TextItem	item = this.items[i];

			if(item.lifetime >= 0.0) {

				item.lifetime -= Time.deltaTime;
	
				this.items[i] = item;
	
				if(item.lifetime <= 0.0f) {
	
					this.items.Remove(this.items[i]);
				}
			}
		}
	}

	// 표시 위치를 설정합니다.
	private void	set_locate(int x, int y)
	{
		this.locate_x = x;
		this.locate_y = y;
	}

	// 텍스트를 추가합니다.
	private void	add_text(string text, float lifetime)
	{
		TextItem	item;

		item.x        = this.locate_x;
		item.y        = this.locate_y;
		item.text     = text;
		item.lifetime = lifetime;

		this.items.Add(item);

		this.locate_y++;
	}
}
