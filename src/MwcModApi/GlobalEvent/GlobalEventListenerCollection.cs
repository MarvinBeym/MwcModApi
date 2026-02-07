using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MwcModApi.GlobalEvent
{
	/// <summary>
	/// Collection of EventListener objects
	/// </summary>
	internal class GlobalEventListenerCollection
	{
		/// <summary>
		/// Flag set when executing InvokeAll method,
		/// if set to true, EventListener removed using the Remove method will not be executed in the InvokeAll and removed after the InvokeAll is finished
		/// </summary>
		private bool currentlyIterating;

		/// <summary>
		/// Protected writable list of EventListener
		/// </summary>
		private List<GlobalEventListener> _eventListeners;

		/// <summary>
		/// A collection of EventListener
		/// </summary>
		/// <param name="eventListeners">Initial list of EventListener</param>
		internal GlobalEventListenerCollection(List<GlobalEventListener> eventListeners)
		{
			_eventListeners = eventListeners;
		}

		/// <summary>
		/// A collection of EventListener
		/// </summary>
		internal GlobalEventListenerCollection()
		{
			_eventListeners = new List<GlobalEventListener>();
		}

		/// <summary>
		/// Adds a new EventListener to this collection
		/// </summary>
		/// <param name="partEventListener">The EventListener object to add to the collection</param>
		internal void Add(GlobalEventListener partEventListener)
		{
			_eventListeners.Add(partEventListener);
		}

		/// <summary>
		/// Invokes all EventListener, allows EventListener to be removed inside of a EventListener
		/// </summary>
		internal void InvokeAll()
		{
			currentlyIterating = true;

			try {
				_eventListeners.ForEach(
					(partEventListener =>
					{
						if (!partEventListener.delete) {
							partEventListener.action.Invoke();
						}
					})
				);
			} catch {
				currentlyIterating = false;
			}

			currentlyIterating = false;

			for (int i = _eventListeners.Count - 1; i >= 0; i--) {
				if (_eventListeners[i].delete) {
					_eventListeners.RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// Returns if the collection contains a specific EventListener
		/// </summary>
		/// <param name="eventListener">The EventListener object to check</param>
		/// <returns>True if the collection contains the provided EventListener, otherwise false</returns>
		internal bool Contains(GlobalEventListener eventListener)
		{
			return _eventListeners.Contains(eventListener);
		}

		/// <summary>
		/// Removes an EventListener object from the collection
		/// If the InvokeAll is currently being run, the EventListener will not be executed (if it hasn't already) and removed after the InvokeAll is finished
		/// </summary>
		/// <param name="eventListener">The EventListener to remove</param>
		/// <returns>True if the EventListener was removed (or will be removed)</returns>
		internal bool Remove(GlobalEventListener eventListener)
		{
			if (!Contains(eventListener)) {
				return false;
			}

			if (!currentlyIterating) {
				return _eventListeners.Remove(eventListener);
			}

			eventListener.delete = true;
			return true;
		}
	}
}