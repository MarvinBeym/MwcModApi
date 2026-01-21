using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MwcModApi.Caching;
using UnityEngine;

namespace MwcModApi.Shopping.Location
{
	public class Fleetari : ShopLocationData
	{
		public override Shop.ShopLocation shopLocation => Shop.ShopLocation.Fleetari;

		public static class Spawn
		{
			public static Vector3 Backroom => new Vector3(1558.975f, 5.2f, 741.894f);
			public static Vector3 Counter => new Vector3(1555.082f, 6f, 737.622f);
			public static Vector3 Outside => new Vector3(1552.154f, 5f, 732.755f);
		}

		internal override CatalogData catalogData => new CatalogData(
			Cache.Find("REPAIRSHOP/inspection_desk"),
			new Vector3(-0.9f, -0.2f, 0.35f),
			new Vector3(0, -90f, -90f)
		);
	}
}
