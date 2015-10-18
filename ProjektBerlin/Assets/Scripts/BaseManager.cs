using UnityEngine;
using System.Collections;

public class BaseManager : MonoBehaviour {

	//Added lights for showing targeted.
	private GameObject myLight;
	[HideInInspector]
	public Light lightPiece;

	public int maxHealth = 100;
	public int currentHealth;

    [RPC]
	public void enableLight()
	{
		lightPiece.enabled = true;
	}
	
    [RPC]
	public void disableLight()
	{
		lightPiece.enabled = false;
	}

	public void init(float atkRadius = 20, float mvRadius = 50)
	{
		currentHealth = maxHealth;

		myLight = new GameObject ();
		myLight.transform.position = transform.position;
		lightPiece = myLight.AddComponent<Light> ();
		lightPiece.color = Color.red;
		lightPiece.intensity = 8;

		lightPiece.enabled = false;
	}

    [RPC]
    public void takeDamage(int dmg, bool hitSpecial)
	{
		currentHealth -= dmg;

        if(currentHealth <= 0)
        {

        }
	}

	void Update(){
		//Updates associated light
		//myLight.transform.position = transform.position;
	}
}
