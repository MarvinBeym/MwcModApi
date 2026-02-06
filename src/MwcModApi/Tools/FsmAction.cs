using System;
using HutongGames.PlayMaker;

namespace MwcModApi.Tools
{
	/// <summary>
	/// A wrapper class for injecting into PlayMakerFSM states
	/// </summary>
	public class FsmAction : FsmStateAction
	{
		public Action action { get; protected set; }

		public FsmAction(Action action)
		{
			this.action = action;
		}

		public override void OnEnter()
		{
			action?.Invoke();
			Finish();
		}
	}
}