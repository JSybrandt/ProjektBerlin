using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SonyNpCommerceStore : IScreen
{
	MenuLayout menu;

	public SonyNpCommerceStore()
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

#if UNITY_PS4 || UNITY_PS3 // Note; these events are not available for PSP2
		Sony.NP.Commerce.OnProductCategoryBrowseStarted += OnSomeEvent;
		Sony.NP.Commerce.OnProductCategoryBrowseFinished += OnSomeEvent;
#endif
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

		if (menu.AddItem("Browse Category", commerceReady))
		{
			// Open the Playstation Store to a specified category, pass category ID or "" to open at the root category.
			SonyNpCommerce.ErrorHandler(Sony.NP.Commerce.BrowseCategory(""));
		}

		if (menu.AddBackIndex("Back"))
		{
			stack.PopMenu();
		}
	}

	void OnSomeEvent(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Event: " + msg.type);
	}
}
