using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SonyNpCommerceInGameStore : IScreen
{
	MenuLayout menu;

	bool sessionCreated = false;

#if UNITY_PSP2
	string testCategoryID = "ED1633-NPXB01864_00-WEAPS_01";
	string testProductID = "ED1633-NPXB01864_00-A000010000000000";
	string[] testProductSkuIDs = new string[] { "ED1633-NPXB01864_00-A000010000000000-E001", "ED1633-NPXB01864_00-A000020000000000-E001" };
#elif UNITY_PS3
	// TODO: These should be changed to use the PS3 IDs.
	string testCategoryID = "ED1633-NPXB01864_00-WEAPS_01";
	string testProductID = "ED1633-NPXB01864_00-A000010000000000";
	string[] testProductSkuIDs = new string[] { "ED1633-NPXB01864_00-A000010000000000-E001", "ED1633-NPXB01864_00-A000020000000000-E001" };
#elif UNITY_PS4
	// TODO: These should be changed to use the PS4 IDs.
	string testCategoryID = "IV0002-NPXX51362_00-WEAPS_01";
	string testProductID = "IV0002-NPXX51362_00-A000010000000000";
	string[] testProductSkuIDs = new string[] { "IV0002-NPXX51362_00-A000010000000000-E001", "IV0002-NPXX51362_00-A000020000000000-E001" };
#else
	string testCategoryID = "";
	string testProductID = "";
	string[] testProductSkuIDs = new string[] { "" };
#endif

	public SonyNpCommerceInGameStore()
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

		Sony.NP.Commerce.OnSessionCreated += OnSessionCreated;
		Sony.NP.Commerce.OnSessionAborted += OnSomeEvent;
		Sony.NP.Commerce.OnGotCategoryInfo += OnGotCategoryInfo;
		Sony.NP.Commerce.OnGotProductList += OnGotProductList;
		Sony.NP.Commerce.OnGotProductInfo += OnGotProductInfo;
		Sony.NP.Commerce.OnCheckoutStarted += OnSomeEvent;
		Sony.NP.Commerce.OnCheckoutFinished += OnSomeEvent;

#if UNITY_PS4 || UNITY_PS3 // Note; these events are not available for PSP2
		Sony.NP.Commerce.OnProductBrowseStarted += OnSomeEvent;
		Sony.NP.Commerce.OnProductBrowseSuccess += OnSomeEvent;
		Sony.NP.Commerce.OnProductBrowseAborted += OnSomeEvent;
		Sony.NP.Commerce.OnProductBrowseFinished += OnSomeEvent;
		Sony.NP.Commerce.OnProductVoucherInputStarted += OnSomeEvent;
		Sony.NP.Commerce.OnProductVoucherInputFinished += OnSomeEvent;
#endif
	}

	public void CreateSession()
	{
		SonyNpCommerce.ErrorHandler(Sony.NP.Commerce.CreateSession());
	}

	public void OnEnter()
	{
		CreateSession();
		Sony.NP.Commerce.ShowStoreIcon(Sony.NP.Commerce.StoreIconPosition.Center);
	}

	public void OnExit()
	{
		Sony.NP.Commerce.HideStoreIcon();
	}

	public void Process(MenuStack stack)
	{
		bool commerceReady = Sony.NP.User.IsSignedInPSN && sessionCreated && !Sony.NP.Commerce.IsBusy();

		menu.Update();

		if (menu.AddItem("Category Info", commerceReady))
		{
			// Request info for a specified category, pass category ID or "" to get the root category.
			SonyNpCommerce.ErrorHandler(Sony.NP.Commerce.RequestCategoryInfo(""));
		}
		
		if (menu.AddItem("Product List", commerceReady))
		{
			// Request the product list for a specified category, pass category ID or "" to get the root category.
			SonyNpCommerce.ErrorHandler(Sony.NP.Commerce.RequestProductList(testCategoryID));
		}

		if (menu.AddItem("Product Info", commerceReady))
		{
			// Request detailed product info, pass a product ID.
			SonyNpCommerce.ErrorHandler(Sony.NP.Commerce.RequestDetailedProductInfo(testProductID));
		}

		if (menu.AddItem("Browse Product", commerceReady))
		{
			// Open the Playstation Store to a specified product, pass product ID.
			SonyNpCommerce.ErrorHandler(Sony.NP.Commerce.BrowseProduct(testProductID));
		}
		
		if (menu.AddItem("Checkout", commerceReady))
		{
			// Open the Playstation store checkout to purchase a specified list of products, pass an array of product Sku IDs.
			Sony.NP.Commerce.CommerceProductInfo[] products = Sony.NP.Commerce.GetProductList();
			SonyNpCommerce.ErrorHandler(Sony.NP.Commerce.Checkout(testProductSkuIDs));
		}

		if (menu.AddItem("Redeem Voucher", commerceReady))
		{
			SonyNpCommerce.ErrorHandler(Sony.NP.Commerce.VoucherInput());
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

	void OnSessionCreated(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Commerce Session Created");
		sessionCreated = true;
	}

	void OnGotCategoryInfo(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Got Category Info");
		Sony.NP.Commerce.CommerceCategoryInfo category = Sony.NP.Commerce.GetCategoryInfo();
		OnScreenLog.Add("Category Id: " + category.categoryId);
		OnScreenLog.Add("Category Name: " + category.categoryName);
		OnScreenLog.Add("Category num products: " + category.countOfProducts);
		OnScreenLog.Add("Category num sub categories: " + category.countOfSubCategories);

		for (int i = 0; i < category.countOfSubCategories; i++)
		{
			Sony.NP.Commerce.CommerceCategoryInfo subCategory = Sony.NP.Commerce.GetSubCategoryInfo(i);
			OnScreenLog.Add("SubCategory Id: " + subCategory.categoryId);
			OnScreenLog.Add("SubCategory Name: " + subCategory.categoryName);
			if (i == 0)
			{
				// Just for testing; request info for the 1st sub-category.
				SonyNpCommerce.ErrorHandler(Sony.NP.Commerce.RequestCategoryInfo(subCategory.categoryId));
			}
		}
	}

	void OnGotProductList(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Got Product List");
		Sony.NP.Commerce.CommerceProductInfo[] products = Sony.NP.Commerce.GetProductList();
		foreach (Sony.NP.Commerce.CommerceProductInfo product in products)
		{
			OnScreenLog.Add("Product: " + product.productName + " - " + product.price);
		}
	}

	void OnGotProductInfo(Sony.NP.Messages.PluginMessage msg)
	{
		OnScreenLog.Add("Got Detailed Product Info");
		Sony.NP.Commerce.CommerceProductInfoDetailed product = Sony.NP.Commerce.GetDetailedProductInfo();
		OnScreenLog.Add("Product: " + product.productName + " - " + product.price);
		OnScreenLog.Add("Long desc: " + product.longDescription);
	}

}
