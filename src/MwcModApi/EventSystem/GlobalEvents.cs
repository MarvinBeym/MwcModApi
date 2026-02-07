using System;
using System.Collections.Generic;
using MwcModApi.Caching;

namespace MwcModApi.EventSystem
{
	public class GlobalEvents
	{
		protected static GlobalEvents instance;

		/// <summary>
		/// Stores all events that a developer may have added to this part object
		/// </summary>
		internal Dictionary<GlobalEventType, GlobalEventListenerCollection> events = new Dictionary<GlobalEventType, GlobalEventListenerCollection>();

		internal GlobalEvents()
		{
			foreach (GlobalEventType eventType in Enum.GetValues(typeof(GlobalEventType)))
			{
				events.Add(eventType, new GlobalEventListenerCollection());
			}
		}

		public static GlobalEvents GetInstance()
		{
			return instance ?? (instance = new GlobalEvents());
		}

		/// <summary>
		/// Add a global event listener and react to this event being triggered
		/// </summary>
		/// <param name="type">Type of event you want to listen to</param>
		/// <param name="action">The code you want to execute on event trigger</param>
		/// <param name="invokeActionIfConditionMet">Immediately execute the action if it's condition is met</param>
		/// <returns>a reference to a GlobalEventListener object, used for later removal</returns>
		public GlobalEventListener AddEventListener(
			GlobalEventType type,
			Action action,
			bool invokeActionIfConditionMet = true
		)
		{
			GlobalEventListener partEventListener = new GlobalEventListener(type, action);
			events[type].Add(partEventListener);

			if (!invokeActionIfConditionMet) {
				return partEventListener;
			}

			switch (type) {
				case GlobalEventType.EngineRunning:
					if (CarH.running) {
						action.Invoke();
					}
					break;
				case GlobalEventType.EngineStalled:
					if (!CarH.running) {
						action.Invoke();
					}
					break;
			}

			return partEventListener;
		}

		/// <summary>
		/// Remove a previously added event listener again
		/// </summary>
		/// <param name="eventListener">The event listener to remove</param>
		/// <returns>true on success, otherwise false</returns>
		public bool RemoveEventListener(GlobalEventListener eventListener)
		{
			var collection = GetEventListeners(eventListener.type);
			return collection.Contains(eventListener) && collection.Remove(eventListener);
		}

		internal GlobalEventListenerCollection GetEventListeners(GlobalEventType type)
		{
			return events[type];
		}

		/// <summary>
		/// Called when the MwcModApi mod loads to cleanup static data
		/// </summary>
		internal static void LoadCleanup()
		{
			instance = null;
		}
	}
}