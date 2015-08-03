using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 케이크 무한제공 시 케이크 흩어뿌리기.
public class CakeTrolley : MonoBehaviour {

	public static string	PAT5 =	  ".**...**."
									+ "*..*.*..*"
									+ "*...*...*"
									+ ".*.....*."
									+ "..*...*.."
									+ "...*.*..."
									+ "....*....";

#if false
	public static string	PAT5 =	  "...***..."
									+ ".**...**."
									+ "*.......*"
									+ "*.......*"
									+ ".*.....*."
									+ ".*..*..*."
									+ "..**.**..";
#endif

	protected const int	PATTERN_WIDTH  = 9;
	protected const int	PATTERN_HEIGHT = 7;

	protected const int	FIRST_SERVE_COUNT = 3;

	protected List<ItemController>	cakes = new List<ItemController>();

	protected bool	is_started = false;
	protected float	create_timer = -1.0f;


	protected int	first_serve = FIRST_SERVE_COUNT;

	// MonoBehaviour에서 상속.
	// ================================================================ //

	void	Start()
	{
	}
	protected Vector3	set_position = Vector3.zero;
	protected float		set_degree = 0.0f;

	protected void	calc_next_set_position()
	{
		float	radius = Random.Range(12.0f, 15.0f);

		this.set_degree = this.set_degree + Random.Range(-9.0f, 9.0f)*10.0f;

		Vector3	v = Quaternion.AngleAxis(this.set_degree, Vector3.up)*Vector3.forward*radius;

		this.set_position += v;

		float	AREA_SIZE = 30.0f;

		if(this.set_position.x < -AREA_SIZE/2.0f) {

			this.set_position.x += AREA_SIZE;

		} else if(this.set_position.x > AREA_SIZE/2.0f) {

			this.set_position.x -= AREA_SIZE;
		}
		if(this.set_position.z < -AREA_SIZE/2.0f) {

			this.set_position.z += AREA_SIZE;

		} else if(this.set_position.z > AREA_SIZE/2.0f) {

			this.set_position.z -= AREA_SIZE;
		}

		this.set_position.y = 0.0f;
	}

	void 	Update()
	{
		// 플레이어가 주운 것을 목록에서 제외.
		this.cakes.RemoveAll(x => x == null);

		//

		this.create_timer += Time.deltaTime;

		do {

			if(!this.is_started) {

				break;
			}

			if(this.first_serve > 0) {

				if(this.create_timer < 0.5f) {
	
					break;
				}

				if(this.first_serve == FIRST_SERVE_COUNT) {

					this.set_position = PartyControl.get().getLocalPlayer().control.getPosition() + new Vector3(0.5f, 0.0f, 0.0f);
					this.set_position.y = 0.0f;

				} else {

					this.calc_next_set_position();
				}
				this.first_serve--;

			} else {

				if(this.create_timer < 0.3f) {
	
					break;
				}
				if(this.cakes.Count > 30) {
	
					break;
				}

				this.calc_next_set_position();
			}

			//

			this.create_cakes(this.set_position);

			this.create_timer = 0.0f;

		} while(false);
	}

	public void		startServe()
	{
		this.is_started = true;
		this.first_serve = FIRST_SERVE_COUNT;
		this.create_timer = 0.0f;
	}

	public void		stopServe()
	{
		this.is_started = false;
	}

	// 케이크를 전부 삭제.
	public void		deleteAllCakes()
	{
		foreach(var cake in this.cakes) {

			if(cake == null) {

				continue;
			}
			ItemManager.get().deleteItem(cake.name);
		}
	}

	protected int		create_count = 0;

	protected void	create_cakes(Vector3 position)
	{
		string	local_player_id = PartyControl.get().getLocalPlayer().getAcountID();

		string	pattern = CakeTrolley.PAT5;


		for(int z = 0;z < PATTERN_HEIGHT;z++) {

			for(int x = 0;x < PATTERN_WIDTH;x++) {

				if(pattern[z*PATTERN_WIDTH + x] != '*') {

					continue;
				}

				string	name = "cake00_" + this.create_count.ToString("D3");

				float	fx = ((float)x) - (float)PATTERN_WIDTH/2.0f;
				float	fz = (PATTERN_HEIGHT - 1 - (float)z) - (float)PATTERN_HEIGHT/2.0f;

				ItemManager.get().createItem("cake00", name, local_player_id);
				ItemManager.get().setPositionToItem(name, position);

				ItemController	item = ItemManager.get().findItem(name);

				item.behavior.beginBuffet();
				item.behavior.buffet_goal   = position + new Vector3(fx*0.8f, 0.0f,  fz*1.0f);
				item.behavior.buffet_height = 4.0f + (float)((this.create_count*5)%8)/6.0f*0.2f;
				item.transform.localScale   = Vector3.one*0.5f;

				this.cakes.Add(item);

				this.create_count++;
			}
		}
	}

	// ================================================================ //
	// 인스턴스.

	private	static CakeTrolley	instance = null;

	public static CakeTrolley	get()
	{
		if(CakeTrolley.instance == null) {

			CakeTrolley.instance = GameObject.Find("CakeTrolley").GetComponent<CakeTrolley>();
		}

		return(CakeTrolley.instance);
	}
}
