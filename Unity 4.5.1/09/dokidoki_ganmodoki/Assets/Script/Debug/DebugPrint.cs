using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 디버깅 문자를 화면에 표시하기 위한 클래스.
// OnGUI() 이외에서도 디버그 프린트를 사용할 수 있다.
public class dbPrint : MonoBehaviour {

	private static dbPrint	instance = null;

	public struct TextItem {

		public int		x, y;
		public string	text;
		public float	lifetime;
	};

	private List<TextItem>	items;
	private int				locate_x, locate_y;

	//private static int		CHARA_W = 20;
	//private static int		CHARA_H = 20;
	private static int		CHARA_W = 12;
	private static int		CHARA_H = 20;

	public Camera	main_camera;

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

	// 텍스트 표시.
	public static void	print(string text, float lifetime)
	{
		dbPrint	dp = dbPrint.getInstance();

		dp.add_text(text, 0.0f);
	}
	public static void	print(string text)
	{
		dbPrint.print(text, 0.0f);
	}

	// 표시 위치 설정.
	public static void	setLocate(int x, int y)
	{
		dbPrint	dp = dbPrint.getInstance();

		dp.set_locate(x, y);
	}

	// 3D 월드 좌표를 지정하여 텍스트 표시.
	public static void	print3d(Vector3 position, string text)
	{
		dbPrint	dp = dbPrint.getInstance();

		position = dp.main_camera.WorldToScreenPoint(position);

		position.y = Screen.height - position.y;

		dp.set_locate_in_pixels((int)position.x, (int)position.y);
		dp.add_text(text, 0.0f);
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
		// 버퍼에 쌓여있는 텍스트 표시.

		int		x, y;

		foreach(var item in this.items) {

			x = item.x;
			y = item.y;

			GUI.Box(new Rect(x, y, item.text.Length*dbPrint.CHARA_W + 4, dbPrint.CHARA_H), item.text);

			y += dbPrint.CHARA_H;
		}

		// 버퍼 클리어.

		if(UnityEngine.Event.current.type == UnityEngine.EventType.Repaint) {

			this.clear();
		}
	}

	public void	create()
	{
		this.items = new List<TextItem>();

		this.main_camera = GameObject.FindGameObjectWithTag("MainCamera").camera;
	}

	// 버퍼 클리어.
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

	// 표시 위치 설정.
	protected void	set_locate(int x, int y)
	{
		this.locate_x = x*dbPrint.CHARA_W;
		this.locate_y = y*dbPrint.CHARA_H;
	}

	// [pixel] 표시 위치 설정.
	protected void	set_locate_in_pixels(int x, int y)
	{
		this.locate_x = x;
		this.locate_y = y;
	}

	// 텍스트 추가.
	protected void	add_text(string text, float lifetime)
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
