using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ?붾쾭源?臾몄옄瑜??붾㈃???쒖떆?섍린 ?꾪븳 ?대옒??
// OnGUI() ?댁쇅?먯꽌???붾쾭洹??꾨┛?몃? ?ъ슜?????덈떎.
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

	// ?띿뒪???쒖떆.
	public static void	print(string text, float lifetime)
	{
		dbPrint	dp = dbPrint.getInstance();

		dp.add_text(text, 0.0f);
	}
	public static void	print(string text)
	{
		dbPrint.print(text, 0.0f);
	}

	// ?쒖떆 ?꾩튂 ?ㅼ젙.
	public static void	setLocate(int x, int y)
	{
		dbPrint	dp = dbPrint.getInstance();

		dp.set_locate(x, y);
	}

	// 3D ?붾뱶 醫뚰몴瑜?吏?뺥븯???띿뒪???쒖떆.
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
		// 踰꾪띁???볦뿬?덈뒗 ?띿뒪???쒖떆.

		int		x, y;

		foreach(var item in this.items) {

			x = item.x;
			y = item.y;

			GUI.Box(new Rect(x, y, item.text.Length*dbPrint.CHARA_W + 4, dbPrint.CHARA_H), item.text);

			y += dbPrint.CHARA_H;
		}

		// 踰꾪띁 ?대━??

		if(UnityEngine.Event.current.type == UnityEngine.EventType.Repaint) {

			this.clear();
		}
	}

	public void	create()
	{
		this.items = new List<TextItem>();

		this.main_camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
	}

	// 踰꾪띁 ?대━??
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

	// ?쒖떆 ?꾩튂 ?ㅼ젙.
	protected void	set_locate(int x, int y)
	{
		this.locate_x = x*dbPrint.CHARA_W;
		this.locate_y = y*dbPrint.CHARA_H;
	}

	// [pixel] ?쒖떆 ?꾩튂 ?ㅼ젙.
	protected void	set_locate_in_pixels(int x, int y)
	{
		this.locate_x = x;
		this.locate_y = y;
	}

	// ?띿뒪??異붽?.
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
