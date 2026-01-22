using MSCLoader;
using MwcModApi.Tools;
using UnityEngine;

namespace MwcModApi.Shopping
{
	public class ShopCatalogLogic : MonoBehaviour
	{
		private ShopLocation shopLocationData;
		private Shop shop;

		void Update()
		{
			if (!gameObject.IsLookingAt() || shop.shopInterface.IsOpen())
			{
				return;
			}

			UserInteraction.GuiInteraction("Open catalog");
			if (UserInteraction.LeftMouseDown)
			{
				shop.shopInterface.Open(shopLocationData);
			}
		}

		public void Init(ShopLocation shopLocationData)
		{
			this.shopLocationData = shopLocationData;
			shop = Shop.GetInstance();
		}
	}
}