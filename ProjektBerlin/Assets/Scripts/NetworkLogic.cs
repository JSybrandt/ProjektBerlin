using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkLogic : MonoBehaviour {

    public Controller gameLogic;
    public NetworkView nView;

    public float rpcTimer = 0;
    public float latency = 0;
    public int timerIndex = 0;
    public float[] timers = new float[30];
    public int latSize = 0, latIndex = 0;
    private float avgLatency = 0;

    // Update is called once per frame
    void Update()
    {
        if (Controller.getIsRunning())
        {
            if (rpcTimer == 0)
            {
                nView.RPC("callRPC", RPCMode.Others);
            }

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
            avgLatency = avgLatency + timers[latIndex] - timers[prev];
            latIndex = (latIndex + 1) % latSize;
        }

        rpcTimer = 0;
    }

    void OnGUI()
    {
        GUILayout.Label("Latency Values");
        int i = 0;
        while (i < Network.connections.Length)
        {
            GUILayout.Label("Player " + Network.connections[i] + " - " + Network.GetAveragePing(Network.connections[i]) + " ms");
            i++;
        }
        GUILayout.Label("Average latency: " + avgLatency);
        GUILayout.Label("Recent latency: " + latency);
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
