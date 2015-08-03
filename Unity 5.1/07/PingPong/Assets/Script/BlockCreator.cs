using UnityEngine;
using System.Collections;

public class BlockCreator : MonoBehaviour {
    public SushiType m_sushiType; //만들고 싶은 초밥을 지정하기 위해.


	// Use this for initialization
	void Start () {
        Create();
	}
	

    void Create() {
        string blockName = m_sushiType.ToString();
        GameObject block = Instantiate(Resources.Load(blockName), transform.position, transform.rotation) as GameObject;

        block.transform.parent = this.gameObject.transform; //Hierarchy가 흩어지지 않도록.
    }

}
