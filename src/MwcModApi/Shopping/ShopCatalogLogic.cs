using MSCLoader;
using MwcModApi.Tools;
using UnityEngine;

namespace MwcModApi.Shopping
{
	public class ShopCatalogLogic : MonoBehaviour
	{
		private ShopLocationData shopLocationData;
		private Shop shop;

		void Update()
		{
			if (!gameObject.IsLookingAt())
			{
				return;
			}

			UserInteraction.GuiInteraction("Open catalog");
			if (UserInteraction.LeftMouseDown)
			{
				shop.shopInterface.Open(shopLocationData);
			}
		}

		public void Init(ShopLocationData shopLocationData)
		{
			this.shopLocationData = shopLocationData;
			shop = Shop.GetInstance();
		}
	}
}