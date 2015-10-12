using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class NetManager : NetworkManager {

	// Use this for initialization
	//void Start () {
	
	//}
	
	//// Update is called once per frame
	//void Update () {
	
	//}

    public override void OnServerConnect(NetworkConnection conn)
    {
        base.OnServerConnect(conn);
        Debug.Log("OnPlayerConnected");
    }
}
