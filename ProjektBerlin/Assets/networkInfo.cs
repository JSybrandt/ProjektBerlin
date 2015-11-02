using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.NetworkInformation;

public class networkInfo : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
//	IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
		NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
		OnScreenLog.Add(String.Format("Interface information for {0}.{1}     ","host", "domain"));
		foreach (NetworkInterface adapter in nics)
		{
			IPInterfaceProperties properties = adapter.GetIPProperties();
			OnScreenLog.Add(adapter.Description);
			OnScreenLog.Add(String.Empty.PadLeft(adapter.Description.Length,'='));
			OnScreenLog.Add(String.Format("  Interface type .......................... : {0}", adapter.NetworkInterfaceType));
			
			
			foreach ( IPAddress dns in properties.DnsAddresses )
    			OnScreenLog.Add(String.Format("\tDNS: {0}", dns));
    		foreach ( IPAddressInformation anycast in properties.AnycastAddresses )
    			OnScreenLog.Add(String.Format("  IP Address (anycast)..................... : {0}", anycast.Address));
    		foreach ( IPAddressInformation multicast in properties.MulticastAddresses )
    			OnScreenLog.Add(String.Format("  IP Address (multicast)................... : {0}", multicast.Address));
    		foreach ( IPAddressInformation unicast in properties.UnicastAddresses )
    			OnScreenLog.Add(String.Format("  IP Address (unicast)..................... : {0}", unicast.Address));
			
			
			OnScreenLog.Add(String.Format("  Physical Address ........................ : {0}", adapter.GetPhysicalAddress().ToString()));
			OnScreenLog.Add(String.Format("  Is receive only.......................... : {0}", adapter.IsReceiveOnly));
			OnScreenLog.Add(String.Format("  Multicast................................ : {0}", adapter.SupportsMulticast));

		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	

}
