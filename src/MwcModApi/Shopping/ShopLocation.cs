using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MwcModApi.Tools;
using UnityEngine;

namespace MwcModApi.Shopping
{

	public abstract class ShopLocation
	{
		public abstract ShopLocationOption shopLocation { get; }
		internal abstract CatalogData catalogData { get; }

		internal List<ModItem> items = new List<ModItem>();
		internal GameObject catalog;
		protected ShopCatalogLogic logic;

		protected ShopLocation()
		{
			if (catalogData.parent == null)
			{
				throw new Exception($"Could not find parent for ShopLocation {shopLocation}. Skipping shop location creation!");
			}

			catalog = GameObject.Instantiate(Shop.Prefabs.shopCatalog);
			catalog.transform.SetParent(catalogData.parent.transform);
			catalog.transform.localPosition = catalogData.position;
			catalog.transform.localRotation = Quaternion.Euler(catalogData.rotation);
			catalog.transform.localScale = catalogData.scale;
			catalog.name = $"{shopLocation} Shop Catalog(Clone)";

			logic = catalog.AddComponent<ShopCatalogLogic>();
			logic.Init(this);
		}
	}
}
