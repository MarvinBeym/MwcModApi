using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MwcModApi.Parts;
using MwcModApi.Tools;
using UnityEngine;

namespace MwcModApi.PaintingSystem
{
	public class PaintingSystemLogic : MonoBehaviour
	{
		private PaintingStorage paintingStorage;

		void Update()
		{
			if (!PaintingSystem.IsPainting() || !gameObject.IsLookingAt()) {
				return;
			}

			Color color = PaintingSystem.GetCurrentColor();

			foreach (var pair in paintingStorage.GetGameObjectMaterialConfig()) {
				foreach (Material material in pair.Value) {
					paintingStorage.SetColorOfMaterial(material, color);
				}
			}
		}

		internal void Init(PaintingStorage paintingStorage)
		{
			this.paintingStorage = paintingStorage;
		}
	}
}