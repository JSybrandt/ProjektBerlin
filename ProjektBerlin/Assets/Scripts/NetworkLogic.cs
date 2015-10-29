using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkLogic : MonoBehaviour {

    public Controller gameLogic;
    public NetworkView nView;

    public float rpcTimer = 0;
    public int timerIndex = 0;
    public List<float> timers;
    private float avgLatency = 0;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if(Controller.getIsRunning())
        {
            if(rpcTimer == 0)
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
        if(timers.Count < 30)
        {
            timers.Add(rpcTimer);
            avgLatency -= avgLatency / timers.Count;
            avgLatency += rpcTimer/timers.Count;
        }
        else
        {
            avgLatency += timers[timerIndex]/timers.Count - rpcTimer/timers.Count;

            timers[timerIndex] = rpcTimer;
            timerIndex++;
            if(timerIndex > 29)
            {
                timerIndex = 0;
            }
        }

        rpcTimer = 0;
    }

    void OnGUI()
    {
        GUILayout.Label("Player ping values");
        int i = 0;
        while (i < Network.connections.Length)
        {
            GUILayout.Label("Player " + Network.connections[i] + " - " + Network.GetAveragePing(Network.connections[i]) + " ms");
            i++;
        }
        GUILayout.Label("Average latency: " + avgLatency + " Sample size: " + timers.Count);
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
