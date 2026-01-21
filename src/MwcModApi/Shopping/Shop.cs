using System;
using System.Collections.Generic;
using System.Linq;
using MSCLoader;
using MwcModApi.Caching;
using MwcModApi.Shopping.Location;
using MwcModApi.Tools;
using UnityEngine;

namespace MwcModApi.Shopping
{
	public class Shop
	{
		private static Shop instance = null;

		internal ShopInterface shopInterface;

		public Dictionary<ShopLocationOption, ShopLocation> shopLocations { get; protected set; }

		public static class SpawnLocation
		{
			public static class Psk
			{
				public static Vector3 Counter { get; } = new Vector3(-1733.651f, 4.440871f, 919.0931f);
			}

			public static class Fleetari
			{
				public static Vector3 Backroom { get; } = new Vector3(1558.975f, 5.2f, 741.894f);
				public static Vector3 Counter { get; } = new Vector3(1555.082f, 6f, 737.622f);
				public static Vector3 Outside { get; } = new Vector3(1552.154f, 5f, 732.755f);
			}
		}

		internal static class Prefabs
		{
			public static GameObject shopInterface;
			public static GameObject modPanel;
			public static GameObject partPanel;
			public static GameObject cartItem;
			internal static GameObject shopCatalog;
		}

		internal Shop()
		{
			instance = this;
			shopInterface = new ShopInterface();

			shopLocations = new Dictionary<ShopLocationOption, ShopLocation>()
			{
				{ ShopLocationOption.Fleetari, new Fleetari() },
				{ ShopLocationOption.Psk, new Psk() },
			};
		}

		public static Shop GetInstance()
		{
			return instance;
		}

		public ShopLocation GetShopLocation(ShopLocationOption shopLocation)
		{
			if (!shopLocations.TryGetValue(shopLocation, out ShopLocation shopLocationData))
			{
				throw new Exception($"Unsupported/Uninitialized ShopLocationOption enum received: {shopLocation}");
			}

			return shopLocationData;
		}

		public void Add(ShopBaseInfo baseInfo, ShopLocation shopLocation, ShopItem[] shopItems)
		{
			foreach (var shopItem in shopItems) {
				Add(baseInfo, shopLocation, shopItem);
			}
		}

		public void Add(ShopBaseInfo baseInfo, ShopLocation shopLocation, ShopItem shopItem)
		{
			shopItem.SetBaseInfo(baseInfo);
			ModItem modItem = shopLocation.items.FirstOrDefault(modItemCached => modItemCached.mod == baseInfo.mod);

			if (modItem == null) {
				modItem = new ModItem(shopLocation, shopInterface, baseInfo.mod);
				shopLocation.items.Add(modItem);
			}

			modItem.Add(shopItem);

			shopItem.Create(shopInterface);
		}

		internal static void LoadAssets(AssetBundle assetBundle)
		{
			Prefabs.shopCatalog = assetBundle.LoadAsset<GameObject>("shop_catalog.prefab");
			Prefabs.shopInterface = assetBundle.LoadAsset<GameObject>("shop_interface.prefab");
			Prefabs.partPanel = assetBundle.LoadAsset<GameObject>("part_panel.prefab");
			Prefabs.modPanel = assetBundle.LoadAsset<GameObject>("mod_panel.prefab");
			Prefabs.cartItem = assetBundle.LoadAsset<GameObject>("cart_item.prefab");
		}
	}
}