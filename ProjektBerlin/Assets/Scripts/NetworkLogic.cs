using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkLogic : MonoBehaviour {

    public Controller gameLogic;
    public NetworkView nView;

    public int latSize = 0, latIndex = 0, timerIndex = 0;
    public float rpcTimer = 0, latency = 0, avgLatency = 0, callAgain = 0;
    public float[] timers = new float[30];
    private bool timerRunning = false;

    // Update is called once per frame
    void Update()
    {
        if (Controller.getIsRunning())
        {
            if (callAgain > 0.25f)
            {
                timerRunning = true;
                callAgain = 0;
                nView.RPC("callRPC", RPCMode.Others);
            }

            if(!timerRunning)
                callAgain += Time.deltaTime;
            else
                rpcTimer += Time.deltaTime;
        }
    }

    [RPC]
    public void callRPC()
    {
        nView.RPC("returnRPC", RPCMode.Others);
    }

    [RPC]
    public void returnRPC()
    {
        if (latSize < 30)
        {
            timers[latSize] = rpcTimer;
            avgLatency = (avgLatency * latSize + rpcTimer)/(latSize+1);
            latSize++;
        }
        else
        {
            timers[latIndex] = rpcTimer;
            int prev = (latIndex + latSize - 1) % latSize;
            avgLatency = (avgLatency*latSize + timers[latIndex] - timers[prev])/latSize;
            latIndex = (latIndex + 1) % latSize;
        }

        rpcTimer = 0;
        timerRunning = false;
    }

    void OnGUI()
    {
        GUILayout.Label("Latency Values");
        GUILayout.Label("Average latency: " + avgLatency);
        GUILayout.Label("Recent latency: " + latency);
    }

    [RPC]
    public void init()
    {
        nView = GetComponent<NetworkView>();
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
        Debug.Log("NETWORK: Set turn called");
        if (turn)
            Debug.Log("Isn't my turn");
        else
            Debug.Log("Is my turn");

        //I don't have any turns left, set his turn back on.
        if (!gameLogic.hasActiveSquads() && turn)
        {
            Debug.Log("Passing turn back");
            gameLogic.nextTurn();   //This will automatically check and call for round over, then pass back if it isn't.
            return;
        }

        gameLogic.setTurn(turn);
    }

    [RPC]
    public void nextRound(bool turn)
    {
        Debug.Log("NETWORK: Next round networked called");
        gameLogic.nextRound();
    }

    [RPC]
    public void otherRoundOver()
    {
        Debug.Log("NETWORK: Other round over called");
        gameLogic.isOtherRoundOver = true;
    }

    [RPC]
    public void gameOver(int playerWinner)
    {
        Debug.Log("NETWORK: Game Over called!");
        gameLogic.gameOver(playerWinner);
    }
}
