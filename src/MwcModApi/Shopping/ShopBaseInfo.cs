using MSCLoader;
using UnityEngine;

namespace MwcModApi.Shopping
{
	public class ShopBaseInfo
	{
		internal Mod mod;
		internal AssetBundle assetBundle;

		public ShopBaseInfo(Mod mod, AssetBundle assetBundle)
		{
			this.mod = mod;
			this.assetBundle = assetBundle;
		}
	}
}