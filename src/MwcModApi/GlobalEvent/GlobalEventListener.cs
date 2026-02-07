using System;

namespace MwcModApi.GlobalEvent
{
	public class GlobalEventListener
	{
		/// <summary>
		/// The type of event
		/// </summary>
		public GlobalEventType type { get; protected set; }

		/// <summary>
		/// The action executed when the event triggers
		/// </summary>
		public Action action { get; protected set; }

		/// <summary>
		/// Marks the EventListener to be deleted from the EventListener list
		/// (When being deleted/removed while in an Event)
		/// </summary>
		public bool delete { internal set; get; } = false;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="type">The type of event</param>
		/// <param name="action">The action executed when the event triggers</param>
		public GlobalEventListener(GlobalEventType type, Action action)
		{
			this.type = type;
			this.action = action;
		}
	}
}