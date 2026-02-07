namespace MwcModApi.EventSystem
{
	public enum GlobalEventType
	{
		/// <summary>
		/// Triggered when the engine turns on
		/// </summary>
		EngineRunning,

		/// <summary>
		/// Triggered when the engine turns off, either trough ignition or any other "external force"
		/// </summary>
		EngineStalled
	}
}