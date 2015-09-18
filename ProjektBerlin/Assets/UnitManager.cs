using UnityEngine;
using System.Collections;

public class UnitManager : MonoBehaviour {
	
	private int _power=2;
	public int power{
		get{return _power;}
		set{_power=Mathf.Max(0,value);}
	}
}
