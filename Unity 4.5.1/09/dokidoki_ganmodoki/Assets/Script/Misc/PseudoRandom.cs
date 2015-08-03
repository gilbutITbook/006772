using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PseudoRandom : MonoBehaviour {

	protected List<Seed>	seeds;					// 난수의 시드　전 단말을 통해 같은 id면 같은 시드가 된다.

	// ================================================================ //

	public void		create()
	{
		this.seeds = new List<Seed>();
	}

	// 난수 생성 오브젝트를 만든다.
	public Plant	createPlant(string id, int cycle = 16)
	{
		Plant	plant = null;

		do {

			Seed	seed = this.create_seed(id);

			if(seed == null) {

				break;
			}

			plant = new Plant(seed, cycle);

		} while(false);

		return(plant);
	}

	protected Seed	create_seed(string id)
	{
		string	local_account = AccountManager.get().getAccountData(GlobalParam.get().global_account_id).account_id;

		Seed	seed = null;

		seed = this.seeds.Find(x => x.id == id);

		if(seed == null) {

			// 찾지 못했으므로 만든다.
			seed = new Seed(local_account, id);

			this.seeds.Add(seed);

			// [TODO] seeds가 전 단말에서 공통되게 동기화한다.

		} else {

			if(seed.creator == local_account) {
	
				// 같은 id의 시드를 두 번이상 만들려고 함.
				Debug.LogError("Seed \"" + id + "\" already exist.");
				seed = null;

			} else {

				// 다른 플레이어가 만든 같은 시드가 있었다.
			}
		}

		return(seed);
	}

	// ================================================================ //

	protected static PseudoRandom	instance = null;

	public static PseudoRandom	get()
	{
		if(PseudoRandom.instance == null) {

			GameObject	go = new GameObject("PseudoRandom");

			PseudoRandom.instance = go.AddComponent<PseudoRandom>();
			PseudoRandom.instance.create();
		}

		return(PseudoRandom.instance);
	}

	// ================================================================ //

	// 난수의 시드.
	public class Seed {

		public Seed(string creator, string id)
		{
			this.seed = 0;	// (임시)　타이머 등으로 하나?.
			this.creator = creator;
			this.id = id;
		}
		public int	getSeed()
		{
			return(this.seed);
		}
		protected int	seed;
		public string	creator;		// 생성한 계정 이름.
		public string	id;				// 고유한 id.　전단말 공통으로 유니크.
	}

	// 난수 생성 오브젝트.
	public class Plant {

		protected List<float>	values;
		protected int			read_index;
		protected Seed			seed;

		public Plant(Seed seed, int cycle)
		{
			this.seed = seed;

			Random.seed = this.seed.getSeed();

			this.values = new List<float>();

			for(int i = 0;i < cycle;i++) {

				this.values.Add(Random.Range(0.0f, 1.0f));
			}

			this.read_index = 0;
		}

		public float	getRandom()
		{
			float	random = this.values[this.read_index];

			this.read_index = (this.read_index + 1)%this.values.Count;

			return(random);
		}

		public int		getRandomInt(int max)
		{
			int		random = (int)(this.getRandom()*(float)max);

			random %= max;

			return(random);
		}
	}
}


