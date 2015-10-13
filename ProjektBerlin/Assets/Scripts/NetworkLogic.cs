using UnityEngine;
using System.Collections;

public class NetworkLogic : MonoBehaviour {

    public Controller gameLogic;
    public NetworkView nView;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    [RPC]
    public void init()
    {
        nView = this.GetComponent<NetworkView>();
        gameLogic = GameObject.Find("GameLogic").GetComponent<Controller>();
        gameLogic.nLogic = this;
        gameLogic.nLogicView = nView;

        if(Network.isClient)
            nView.RPC("begin", RPCMode.AllBuffered);
    }

    [RPC]
    public void begin()
    {
        gameLogic.begin();
    }

    [RPC]
    public void setTurn(bool turn)
    {
        gameLogic.setTurn(turn);
    }
}
