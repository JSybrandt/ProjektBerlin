﻿using UnityEngine;
using System.Collections;

public class LoadGame : MonoBehaviour
{


    // Use this for initialization
    void Start()
    {

        GameObject SquadPrefab = (GameObject)Resources.Load("Squad");
        if (SquadPrefab == null)
            throw new MissingReferenceException("Failed to find squad prefab");

        for (int i = 0; i < 20; i += 5)
        {
            GameObject newSquad = (GameObject)Instantiate(SquadPrefab, new Vector3(i, 0, -80), Quaternion.identity);
            newSquad.tag = "Player0Squad";
        }

		for (int i = 0; i < 20; i += 5)
		{
			GameObject newSquad = (GameObject)Instantiate(SquadPrefab, new Vector3(i, 0, 65), Quaternion.identity);
			newSquad.tag = "Player1Squad";
		}

        Controller controllerScript = GetComponent<Controller>();
		controllerScript.init ();
        controllerScript.updateSquadList("Player0Squad");
    }

}