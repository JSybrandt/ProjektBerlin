using UnityEngine;
using System.Collections;

public class UnitLightController : MonoBehaviour {
    public GameObject teamLeader;
	// Use this for initialization
	void Start () {
	
	}
	
    void Update ()
    {
        if (Input.GetKeyDown("f"))
        {
            
        }
    }

	// Update is called once per frame
	void FixedUpdate () {
        //transform.position = teamLeader.transform.position - transform.localPosition;
    }
}
