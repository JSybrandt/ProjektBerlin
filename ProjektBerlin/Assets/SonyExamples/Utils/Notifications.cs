using UnityEngine;
using System.Collections;


public class Notifications : MonoBehaviour
{
	void OnEnable()
	{
#if UNITY_PS3
		PS3SystemUtility.OnSystemNotification += HandlePS3Notification;
#endif
	}
	void OnDisable()
	{
#if UNITY_PS3
		PS3SystemUtility.OnSystemNotification -= HandlePS3Notification;
#endif
	}
	void HandlePS3Notification(uint subsystem, uint index, uint notification, uint status)
	{
#if UNITY_PS3
		// handle system menu notices
		if (subsystem == (uint)PS3SystemConstants.System)
		{
			switch (notification)
			{
				case (uint)PS3SystemConstants.ExitgameRequest:
					Debug.Log("-RECEIVED EXIT GAME REQUEST-");
					Sony.NP.Main.ShutDown();
					break;
				default:
					break;
			}
		}
#endif
	}
}
