using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MwcModApi.Caching;
using UnityEngine;

namespace MwcModApi.Shopping.Location
{
	public class Psk : ShopLocation
	{
		public override ShopLocationOption shopLocation => ShopLocationOption.Psk;

		public static class Spawn
		{
			public static Vector3 Counter => new Vector3(-1733.651f, 4.440871f, 919.0931f);
		}

		internal override CatalogData catalogData =>
			new CatalogData(
				Cache.Find("PERAPORTTI/ActiveFunctions/Store/PostOffice/post_table"),
				new Vector3(-0.375f, -0.6f, 0.94f),
				new Vector3(90f, 0f, 0f)
			);
	}
}