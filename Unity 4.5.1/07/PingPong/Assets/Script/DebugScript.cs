using UnityEngine;
using System.Collections;
 
public class DebugScript : MonoBehaviour {
    System.IO.StreamWriter m_writer;
    GameObject m_inputManager;
    GameObject m_serverBar;
    GameObject m_clientBar;
	 
    void Awake() {
        string filename = Application.dataPath + "/DebugScript.log";
        m_writer = new System.IO.StreamWriter(filename);
        m_writer.WriteLine("DebugStart");

        m_inputManager = GameObject.Find("InputManager");
        m_serverBar = null;
        m_clientBar = null;
    }

    void FixedUpdate() {
        if(m_serverBar == null){
            m_serverBar = GameObject.Find("ServerBar");
        }
        if(m_clientBar == null){
            m_clientBar = GameObject.Find("ClientBar");
        }

        InputManager im = m_inputManager.GetComponent<InputManager>();
        MouseData data = im.GetMouseData(0);
        
        string str = "--\n";
        str += "data0.frame:" + data.frame + "\n";

        data = im.GetMouseData(1);
        str += "data1.frame:" + data.frame + "\n";

        if (m_serverBar) {
            str += "server.pos:" + m_serverBar.transform.position.ToString() + "\n";
        }
        if (m_clientBar) {
            str += "client.pos:" + m_clientBar.transform.position.ToString() + "\n";
        }

        GameObject[] objs = GameObject.FindGameObjectsWithTag("Block");
        str += "Block:" + objs.Length;
        foreach (GameObject o in objs) {
            str += o.transform.position.ToString();
        }

		if (m_writer != null) {
	        m_writer.WriteLine(str);
    	    m_writer.Flush();
		}
    }

    void OnDestroy() { 
		if (m_writer != null) {
	        m_writer.WriteLine("DebugEnd");
    	    m_writer.Close();
		}
    }
}
