using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class SonyNpTicketing : IScreen
{
	MenuLayout menuTicketing;
	bool gotTicket = false;
	Sony.NP.Ticketing.Ticket ticket;

	public SonyNpTicketing()
	{
		Initialize();
	}

	public MenuLayout GetMenu()
	{
		return menuTicketing;
	}

	public void Initialize()
	{
		menuTicketing = new MenuLayout(this, 450, 34);

		Sony.NP.Ticketing.OnGotTicket += OnGotTicket;
		Sony.NP.Ticketing.OnError += OnError;
	}

	public void OnEnter()
	{
	}

	public void OnExit()
	{
	}

	Sony.NP.ErrorCode ErrorHandler(Sony.NP.ErrorCode errorCode = Sony.NP.ErrorCode.NP_ERR_FAILED)
	{
		if (errorCode != Sony.NP.ErrorCode.NP_OK)
		{
			Sony.NP.ResultCode result = new Sony.NP.ResultCode();
			Sony.NP.Ticketing.GetLastError(out result);
			if (result.lastError != Sony.NP.ErrorCode.NP_OK)
			{
				OnScreenLog.Add("Error: " + result.className + ": " + result.lastError + ", sce error 0x" + result.lastErrorSCE.ToString("X8"));
				return result.lastError;
			}
		}

		return errorCode;
	}

	public void Process(MenuStack stack)
	{
		menuTicketing.Update();

		bool ready = Sony.NP.User.IsSignedInPSN && !Sony.NP.Ticketing.IsBusy();
		if (menuTicketing.AddItem("Request Ticket", ready))
		{
			ErrorHandler(Sony.NP.Ticketing.RequestTicket());
		}

#if !UNITY_PS3	// Not supported on PS3.
		if (menuTicketing.AddItem("Request Cached Ticket", ready))
		{
			ErrorHandler(Sony.NP.Ticketing.RequestCachedTicket());
		}
#endif

		if (menuTicketing.AddItem("Get Ticket Entitlements", gotTicket))
		{
			Sony.NP.Ticketing.TicketEntitlement[] entitlements = Sony.NP.Ticketing.GetTicketEntitlements(ticket);

			OnScreenLog.Add("Ticket contains " + entitlements.Length + " entitlements");
			for (int i = 0; i < entitlements.Length; i++)
			{
				OnScreenLog.Add("Entitlement " + i);
				OnScreenLog.Add(" " + entitlements[i].id + " rc: " + entitlements[i].remainingCount + " cc: " + entitlements[i].consumedCount + " type: " + entitlements[i].type);
			}
		}
		
		if (menuTicketing.AddBackIndex("Back"))
		{
			stack.PopMenu();
		}
	}

	void OnGotTicket(Sony.NP.Messages.PluginMessage msg)
	{
		ticket = Sony.NP.Ticketing.GetTicket();
		gotTicket = true;
		OnScreenLog.Add("GotTicket");
		OnScreenLog.Add(" dataSize: " + ticket.dataSize);

		Sony.NP.Ticketing.TicketInfo info = Sony.NP.Ticketing.GetTicketInfo(ticket);
		OnScreenLog.Add(" Issuer ID: " + info.issuerID);
		DateTime it = new DateTime(info.issuedDate, DateTimeKind.Utc);
		OnScreenLog.Add(" Issue date: " + it.ToLongDateString() + " - " + it.ToLongTimeString());
		DateTime et = new DateTime(info.expireDate, DateTimeKind.Utc);
		OnScreenLog.Add(" Expire date: " + et.ToLongDateString() + " - " + et.ToLongTimeString());
		OnScreenLog.Add(" Account ID: 0x" + info.subjectAccountID.ToString("X8"));
		OnScreenLog.Add(" Online ID: " + info.subjectOnlineID);
		OnScreenLog.Add(" Service ID: " + info.serviceID);
		OnScreenLog.Add(" Domain: " + info.subjectDomain);
		OnScreenLog.Add(" Country Code: " + info.countryCode);
		OnScreenLog.Add(" Language Code: " + info.languageCode);
		OnScreenLog.Add(" Age: " + info.subjectAge);
		OnScreenLog.Add(" Chat disabled: " + info.chatDisabled);
		OnScreenLog.Add(" Content rating: " + info.contentRating);
	}

	void OnError(Sony.NP.Messages.PluginMessage msg)
	{
		ErrorHandler();
	}
}
