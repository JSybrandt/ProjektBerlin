/*using System;
namespace AssemblyCSharp
{
	public class Turn
	{
		public enum PlayerAct{
			StartMovement,
			StartCombat,
			Finalize,
			Cancel,
			Nothing
		}

		private int numActions = 0;
		private const int MAX_ACTIONS = 2;
		private int numCancels=0;
		private const int MAX_CANCELS=3;
		private PlayerAct _currentAction = PlayerAct.Nothing;

		public PlayerAct currentAction{
			get{return _currentAction;}
		}

		public Turn ()
		{
		}

		public void updateState(PlayerAct action){
			switch (action) {

			case PlayerAct.Cancel:
				if(_currentAction!=PlayerAct.Nothing){
					numActions--;numCancels++;
					_currentAction=PlayerAct.Nothing;
				}
				break;

			case PlayerAct.StartCombat: 
			case PlayerAct.StartMovement:
				if(numActions < MAX_ACTIONS && currentAction==PlayerAct.Nothing){
					_currentAction = action;
				}
				break;

			case PlayerAct.Finalize:
				if(_currentAction==PlayerAct.StartMovement || _currentAction==PlayerAct.StartCombat){
					numActions++;
					_currentAction = PlayerAct.Nothing;
				}
			}
		}
	}
}





*/