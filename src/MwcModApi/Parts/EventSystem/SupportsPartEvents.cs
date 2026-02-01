using System;
using System.Collections.Generic;

namespace MwcModApi.Parts.EventSystem
{
	/// <summary>
	/// Proxy class for interface.
	/// </summary>
	public class SupportsPartEvents : ISupportsPartEvents
	{
		/// <summary>
		/// Stores all events that a developer may have added to this part object
		/// </summary>
		protected Dictionary<PartEvent.Time, Dictionary<PartEvent.Type, PartEventListenerCollection>> events =
			new Dictionary<PartEvent.Time, Dictionary<PartEvent.Type, PartEventListenerCollection>>();

		public SupportsPartEvents()
		{
			foreach (PartEvent.Time eventTime in Enum.GetValues(typeof(PartEvent.Time)))
			{
				Dictionary<PartEvent.Type, PartEventListenerCollection> TypeDict =
					new Dictionary<PartEvent.Type, PartEventListenerCollection>();

				foreach (PartEvent.Type Type in Enum.GetValues(typeof(PartEvent.Type)))
				{
					TypeDict.Add(Type, new PartEventListenerCollection());
				}

				events.Add(eventTime, TypeDict);
			}
		}

		/// <inheritdoc />
		public PartEventListener AddEventListener(
			PartEvent.Time eventTime,
			PartEvent.Type Type,
			Action action,
			bool invokeActionIfConditionMet = true
		)
		{
			PartEventListener partEventListener = new PartEventListener(eventTime, Type, action);
			events[eventTime][Type].Add(partEventListener);
			return partEventListener;
		}

		/// <inheritdoc />
		public bool RemoveEventListener(PartEventListener partEventListener)
		{
			var collection = GetEventListeners(partEventListener.eventTime, partEventListener.type);
			return collection.Contains(partEventListener) && collection.Remove(partEventListener);
		}

		/// <inheritdoc />
		public PartEventListenerCollection GetEventListeners(PartEvent.Time eventTime, PartEvent.Type Type)
		{
			return events[eventTime][Type];
		}
	}
}