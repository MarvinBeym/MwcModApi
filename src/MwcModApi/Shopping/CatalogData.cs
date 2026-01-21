using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace MwcModApi.Shopping
{
	public class CatalogData
	{
		public GameObject parent { get; }
		public Vector3 position { get; }
		public Vector3 rotation { get; }

		public Vector3 scale { get; protected set; } = new Vector3(1, 1, 1);

		public CatalogData(GameObject parent, Vector3 position, Vector3 rotation)
		{
			this.parent = parent;
			this.position = position;
			this.rotation = rotation;
		}

		public CatalogData(GameObject parent, Vector3 position, Vector3 rotation, Vector3 scale): this(parent, position, rotation)
		{
			this.scale = scale;
		}
	}
}
