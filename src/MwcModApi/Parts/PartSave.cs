using System;
using System.Collections.Generic;
using MwcModApi.Saving;
using UnityEngine;

namespace MwcModApi.Parts
{
	internal class PartSave
	{
		public enum BoughtState
		{
			No,
			Yes,
			NotConfigured
		}

		public bool installed = false;
		public BoughtState bought = BoughtState.NotConfigured;
		public List<Screw> screws = new List<Screw>();
		public SerializableVector3 position = new SerializableVector3();
		public SerializableQuaternion rotation = new SerializableQuaternion();
	}
}