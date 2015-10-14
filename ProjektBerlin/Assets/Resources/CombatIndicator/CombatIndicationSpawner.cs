using UnityEngine;
using System.Collections;

public class CombatIndicationSpawner : MonoBehaviour {

	private static GameObject CombatIndicatorPrefab;

	private static float radius = 5;
	 
	public static void instantiate(){
		CombatIndicatorPrefab = (GameObject) Resources.Load ("CombatIndicator/CombatIndicator");
		if (CombatIndicatorPrefab == null)
			throw new MissingReferenceException ("Missing /CombatIndicator/CombatIndicator.prefab");
	}
	
	public static void spawnHits(Vector3 center, int num){
		spawnInCircle (center, num, 0); //0 = HIT
	}

	public static void spawnMisses(Vector3 center, int num){
		spawnInCircle (center, num, 1); //1 = MISS
	}

	private static void spawnInCircle(Vector3 center, int num, int type){
		if(num>0)
			for (float i =0; i < 360; i+=(360/num)) {
				Vector3 pos = (Quaternion.Euler(0,i,0) * (Vector3.forward * radius)) + center + (Vector3.up*3);

				GameObject c = (GameObject)Network.Instantiate(CombatIndicatorPrefab,pos,Quaternion.identity,0);
                c.GetComponent<NetworkView>().RPC("set", RPCMode.All,pos,type);
				//c.GetComponent<CombatIndicatorController>().set (pos,type);
			}
	}
}
