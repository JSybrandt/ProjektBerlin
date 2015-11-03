using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SonyNpCommerceEntitlements : IScreen
{
	MenuLayout menu;

	public SonyNpCommerceEntitlements()
	{
		Initialize();
	}

	public MenuLayout GetMenu()
	{
		return menu;
	}

	public void Initialize()
	{
		menu = new MenuLayout(this, 450, 34);

		Sony.NP.Commerce.OnGotEntitlementList += OnGotEntitlementList;
		Sony.NP.Commerce.OnConsumedEntitlement += OnConsumedEntitlement;
	}

	public void OnEnter()
	{
	}

	public void OnExit()
	{
	}

	public void Process(MenuStack stack)
	{
		bool commerceReady = Sony.NP.User.IsSignedInPSN && !Sony.NP.Commerce.IsBusy();

		menu.Update();
		
		if (menu.AddItem("Get Entitlement List", commerceReady))
		{
			// Request the users entitlement list.
			SonyNpCommerce.ErrorHandler(Sony.NP.Commerce.RequestEntitlementList());
		}

		if (menu.AddItem("Consume Entitlement", commerceReady))
		{
			// Consume an entitlement.
			Sony.NP.Commerce.CommerceEntitlement[] ents = Sony.NP.Commerce.GetEntitlementList();
			if (ents.Length > 0)
			{
				// Fully consume the first entitlement in the currently cached entitlement list.
				SonyNpCommerce.ErrorHandler(Sony.NP.Commerce.ConsumeEntitlement(ents[0].id, ents[0].remainingCount));
			}
		}

		if (menu.AddBackIndex("Back"))
		{
			stack.PopMenu();
		}
	}

	void OnGotEntitlementList(Sony.NP.Messages.PluginMessage msg)
	{
		Sony.NP.Commerce.CommerceEntitlement[] ents = Sony.NP.Commerce.GetEntitlementList();
		OnScreenLog.Add("Got Entitlement List, ");
		if (ents.Length > 0)
		{
			foreach (Sony.NP.Commerce.CommerceEntitlement ent in ents)
			{
				OnScreenLog.Add(" " + ent.id + " rc: " + ent.remainingCount + " cc: " + ent.consumedCount + " type: " + ent.type);
			}
		}
		else
		{
			OnScreenLog.Add("You do not have any entitlements.");
		}
	}

	void OnConsumedEntitlement(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Consumed Entitlement");
	}

}
