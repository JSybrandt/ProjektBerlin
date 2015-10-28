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
        Debug.Log("Set turn called");
        //I don't have any turns left, set his turn back on.
        if (!gameLogic.hasActiveSquads())
        {
            nView.RPC("setTurn", RPCMode.Others, true); //Could theoretically be a infinite loop
            return;
        }
        gameLogic.setTurn(turn);
    }

    [RPC]
    public void nextRound(bool turn)
    {
        Debug.Log("Next round networked called");
        gameLogic.nextRound();
    }

    [RPC]
    public void otherRoundOver()
    {
        Debug.Log("Other round over called");
        gameLogic.isOtherRoundOver = true;
    }

    [RPC]
    public void gameOver(int playerWinner)
    {
        Debug.Log("Game Over called!");
        gameLogic.gameOver(playerWinner);
    }
}
