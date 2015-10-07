using UnityEngine;
using System.Collections;

public class CombatIndicationSpawner : MonoBehaviour {

	private static GameObject CombatIndicatorPrefab;

	private static float radius = 3;
	 
	public static void Start(){
		CombatIndicatorPrefab = (GameObject) Resources.Load ("/CombatIndicator/CombatIndicator.prefab");
		if (CombatIndicatorPrefab == null)
			throw new MissingReferenceException ("Missing /CombatIndicator/CombatIndicator.prefab");
	}
	
	public static void spawnHits(Vector3 center, int num){
		spawnInCircle (center, num, CombatIndicatorController.Type.HIT);
	}

	public static void spawnMisses(Vector3 center, int num){
		spawnInCircle (center, num, CombatIndicatorController.Type.MISS);
	}

	private static void spawnInCircle(Vector3 center, int num, CombatIndicatorController.Type type){
		for (float i =0; i < 360; i+=(360/num)) {
			Vector3 pos = (Quaternion.Euler(0,i,0) * (Vector3.forward * radius)) + center;

			GameObject c = Instantiate<GameObject>(CombatIndicatorPrefab);
			c.GetComponent<CombatIndicatorController>().set (pos,type);
		}
	}
}
