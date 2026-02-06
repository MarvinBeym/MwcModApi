using MwcModApi.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using HutongGames.PlayMaker;
using MSCLoader;
using MwcModApi.Caching;
using UnityEngine;
using UnityEngine.UI;
using static MwcModApi.Shopping.Shop;

namespace MwcModApi.Shopping
{
	internal class ShopInterface
	{
		private const int SCROLL_SENSITIVITY = 50;

		private GameObject gameObject;
		internal GameObject partsPanel;
		internal GameObject modsPanel;
		internal GameObject cartPanel;
		private Text btnBuyTextComp;
		internal GameObject partsList;
		internal GameObject modsList;
		internal GameObject cartList;
		private Text moneyComp;
		private Text totalCostComp;
		private Button btnBuyComp;
		private List<ShopItem> shoppingCart = new List<ShopItem>();

		private Color textColor = new Color(1, 1, 0, 1);

		private float totalCost = 0;
		private FsmBool playerInMenu;
		private FsmBool playerStop;
		internal bool open = false;
		private GameObject gameMenuGUI;

		internal ShopInterface()
		{
			playerInMenu = FsmVariables.GlobalVariables.FindFsmBool("PlayerInMenu");
			playerStop = FsmVariables.GlobalVariables.FindFsmBool("PlayerStop");
			gameMenuGUI = Cache.Find("Systems/OptionsMenu");
			gameObject = GameObject.Instantiate(Shop.Prefabs.shopInterface);
			gameObject.SetActive(false);
			partsPanel = gameObject.FindChild("panel/shop/parts_panel");
			modsPanel = gameObject.FindChild("panel/shop/mods_panel");
			cartPanel = gameObject.FindChild("panel/cart");

			partsPanel.FindChild("btnBack").GetComponent<Button>().onClick.AddListener(OnBack);
			cartPanel.FindChild("btnClose").GetComponent<Button>().onClick.AddListener(Close);

			moneyComp = cartPanel.FindChild("money").GetComponent<Text>();
			totalCostComp = cartPanel.FindChild("totalCost").GetComponent<Text>();

			btnBuyComp = cartPanel.FindChild("btnBuy").GetComponent<Button>();
			btnBuyComp.onClick.AddListener(OnCheckout);
			btnBuyTextComp = btnBuyComp.gameObject.FindChild("Text").GetComponent<Text>();

			GameObject partsListContainer = partsPanel.FindChild("parts_list/list");
			partsListContainer.GetComponent<ScrollRect>().scrollSensitivity = SCROLL_SENSITIVITY;
			
			GameObject modsListContainer = modsPanel.FindChild("mod_list/list");
			partsListContainer.GetComponent<ScrollRect>().scrollSensitivity = SCROLL_SENSITIVITY;

			GameObject cartListContainer = cartPanel.FindChild("cart_list/list");
			partsListContainer.GetComponent<ScrollRect>().scrollSensitivity = SCROLL_SENSITIVITY;

			partsList = partsListContainer.FindChild("grid");
			modsList = modsListContainer.FindChild("grid");
			cartList = cartListContainer.FindChild("grid");
		}

		internal void Open(ShopLocation shopLocation)
		{
			open = true;
			playerInMenu.Value = true;
			playerStop.Value = true;

			modsPanel.SetActive(true);
			partsPanel.SetActive(false);
			moneyComp.text = Player.money.ToString();
			gameObject.SetActive(true);

			EmptyShoppingCart();

			foreach (ModItem modItem in shopLocation.items) {
				modItem.Show(true);
			}

			foreach (var shopLocationModItems in Shop.GetInstance().shopLocations) {
				if (shopLocationModItems.Value == shopLocation) {
					continue;
				}

				foreach (ModItem modItem in shopLocationModItems.Value.items) {
					modItem.Show(false);
				}
			}
		}

		internal bool IsOpen()
		{
			return open;
		}

		internal void Close()
		{
			open = false;
			playerInMenu.Value = false;
			playerStop.Value = false;
			gameMenuGUI.SetActive(false);

			OnBack();
			gameObject.SetActive(false);
		}

		internal void OnBack()
		{
			modsPanel.SetActive(true);
			partsPanel.SetActive(false);
		}

		internal void OnOpenShop(ShopLocation shopLocation, ModItem modItemToOpen)
		{
			modsPanel.SetActive(false);
			partsPanel.SetActive(true);

			modItemToOpen.Open();

			foreach (ModItem modItem in shopLocation.items.Where(modItem => modItem != modItemToOpen)) {
				modItem.Close();
			}
		}

		internal void OnAddToCart(ShopItem shopItem)
		{
			if (shoppingCart.Contains(shopItem)) {
				if (shopItem.multiPurchase) {
					shopItem.IncreaseCount();
					totalCost += shopItem.baseItemPrize;
					totalCostComp.text = totalCost.ToString();
				}

				return;
			}

			shopItem.AddToCart();

			shoppingCart.Add(shopItem);
			totalCost += shopItem.baseItemPrize;
			totalCostComp.text = totalCost.ToString();

			SetBuyPossible(totalCost < Player.money);
		}

		internal void SetBuyPossible(bool possible)
		{
			if (possible) {
				totalCostComp.color = textColor;
				btnBuyTextComp.color = textColor;
				btnBuyComp.enabled = true;
			} else {
				totalCostComp.color = Color.red;
				btnBuyTextComp.color = Color.red;
				btnBuyComp.enabled = false;
			}
		}

		private void EmptyShoppingCart()
		{
			totalCost = 0;
			totalCostComp.text = totalCost.ToString();
			foreach (ShopItem item in shoppingCart) {
				GameObject.Destroy(item.cartItemGameObject);
			}

			shoppingCart.Clear();
		}

		internal void OnRemoveFromCart(ShopItem shopItem)
		{
			totalCost -= shopItem.baseItemPrize;
			totalCostComp.text = totalCost.ToString();
			if (!shopItem.multiPurchase) {
				GameObject.Destroy(shopItem.cartItemGameObject);
				shoppingCart.Remove(shopItem);
			} else {
				if (shopItem.itemCount <= 1) {
					GameObject.Destroy(shopItem.cartItemGameObject);
					shoppingCart.Remove(shopItem);
				} else {
					shopItem.DecreaseCount();
				}
			}
		}

		internal void OnCheckout()
		{
			Player.money -= totalCost;
			totalCost = 0;
			totalCostComp.text = totalCost.ToString();
			foreach (var shopItem in shoppingCart) {
				if (shopItem.multiPurchase) {
					for (var i = 0; i < shopItem.itemCount; i++) {
						shopItem.onPurchaseAction.Invoke();
					}
				} else {
					shopItem.onPurchaseAction.Invoke();
					GameObject.Destroy(shopItem.partItemGameObject);
				}

				GameObject.Destroy(shopItem.cartItemGameObject);
			}

			shoppingCart.Clear();

			foreach (var shopLocationItem in Shop.GetInstance().shopLocations) {
				foreach (ModItem modItem in shopLocationItem.Value.items) {
					modItem.UpdatePartCount();
				}
			}

			gameObject.PlayCheckout();
			Close();
		}
	}
}