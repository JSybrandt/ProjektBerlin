using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkLogic : MonoBehaviour {

    public Controller gameLogic;
    public NetworkView nView;

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
        if (turn)
            Debug.Log("Isn't my turn");
        else
            Debug.Log("Is my turn");

        //I don't have any turns left, set his turn back on.
        if (!gameLogic.hasActiveSquads() && turn)
        {
            Debug.Log("Passed turn back");
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
