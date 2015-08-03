using UnityEngine;
using System.Collections;

public class Kurukuru : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        //transform.Rotate(transform.up, 360*Time.deltaTime, Space.World);
        transform.Rotate(Vector3.up, 720 * Time.deltaTime, Space.Self);
	}
} 
