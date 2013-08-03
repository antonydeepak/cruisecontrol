using CruiseControl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CruiseControl
{
	public class Commander
	{
		public BoardStatus _currentBoard;
		List<Command> cmds = new List<Command> ();

		public Commander ()
		{
			_currentBoard = new BoardStatus ();
		}

		// Do not alter/remove this method signature
		public List<Command> GiveCommands ()
		{
			const int CLUTTER = 0;
			const int REPAIR = 1;
			const int RADAR = 2;
			const int EXTRA_COUNTER = 3;
			int myVessels = _currentBoard.MyVesselStatuses.Count;
			int totalCommandsAvl = myVessels + 1;
			int roundNumber = _currentBoard.RoundNumber;
			string strategy = (roundNumber % 2) == 0 ? "defense" : "offense";
			List<Coordinate> myCoordinates = new List<Coordinate> ();
			//collect my vessel locations
			foreach (VesselStatus myStatus in _currentBoard.MyVesselStatuses) {
				myCoordinates.AddRange (myStatus.Location);
			}

			bool isClutterCollectable = _currentBoard.ClusterMissleIsOnGameBoard;
			switch (strategy) {
			case "offense":
				LoadPowerup();
				bool isClutter = _currentBoard.MyPowerUps.Contains (CLUTTER);
				int clutterID = _currentBoard.MyPowerUps.IndexOf (CLUTTER);
				List<VesselStatus> allies = _currentBoard.SonarReportsOfAllies;
				foreach (VesselStatus myStatus in _currentBoard.MyVesselStatuses) {
					var sonarCoods = myStatus.SonarReport;
					var fireableCoords = new List<Coordinate> ();
					//collect fireable coord;
					foreach (Coordinate cood in sonarCoods) {
						if (!myCoordinates.Contains (cood)) {
							fireableCoords.Add (cood);
						}
					}
					//fire
					foreach (Coordinate firecood in fireableCoords) {
						if (totalCommandsAvl > 0) {
							if (isClutter)
								cmds.Add (new Command (){vesselid = myStatus.Id, action = "power_up:" + clutterID, coordinate = new Coordinate { X = firecood.X, Y = firecood.Y }});
							else
								cmds.Add (new Command (){vesselid = myStatus.Id, action = "fire", coordinate = new Coordinate { X = firecood.X, Y = firecood.Y }});

							totalCommandsAvl--;
						}
					}

				}
				if (isClutterCollectable)//get clutter.
					CollectClutter (_currentBoard.ClusterMissleLocation);
				LoadCounters();//load counters in avl.
				break;
			case "defense":
				LoadPowerup();
				LoadCounters();
				if (isClutterCollectable)
					CollectClutter (_currentBoard.ClusterMissleLocation);

				//move random
				string direction = RandomChoice(new string[]{"north","south","east","west"});
				foreach(var myVessel in _currentBoard.MyVesselStatuses)
				{
					cmds.Add (new Command { vesselid = myVessel.Id, action = "move:"+direction});
				}
				break;
			default:
				break;	
			}
			// if roundno. is odd then
			//		offense
			//			Increse power-up : RADAR(prev. round), CLUTTER
			//			Action: fire, if truce accepted {fire using radar}
			//			RADAR: check if the co-or is one of yoours
			//else
			//		defense
			//			power-up : counter measure,repair
			//			REpair damaged vehicles.
			//			action: move,load counter measures, load truce(prob at %4 == 0)
					
			// You can only give as many commands as you have un-sunk vessels. Powerup commands do not count against this number. 
			// You are free to use as many powerup commands at any time. Any additional commands you give (past the number of active vessels) will be ignored.
			//cmds.Add (new Command { vesselid = 1, action = "fire", coordinate = new Coordinate { X = 1, Y = 1 } });
			return cmds;
		}

		private T RandomChoice<T> (IEnumerable<T> source) {
			Random rnd = new Random();
			T result = default(T);
			int cnt = 0;
			foreach (T item in source) {
				cnt++;
				if (rnd.Next(cnt) == 0) {
					result = item;
				}
			}
			return result;
		}

		private void LoadCounters ()
		{
			foreach (VesselStatus myStatus in _currentBoard.MyVesselStatuses) {
				int n = myStatus.CounterMeasures;
				int i=0;
				while(i<n){
					cmds.Add(new Command (){vesselid = myStatus.Id, action = "load_countermeasures"});
					i++;
				}
			}
		}

		private void LoadPowerup()
		{
			for(int i=0;i<_currentBoard.MyPowerUps.Count;i++)
			{
				foreach(VesselStatus myStatus in _currentBoard.MyVesselStatuses)
				{
					cmds.Add (new Command (){vesselid = myStatus.Id, action = "power_up:" + i});
				}
			}
		}

		private void CollectClutter (Coordinate clutterCood)
		{
			foreach(VesselStatus myStatus in _currentBoard.MyVesselStatuses) {
				foreach(Coordinate c in myStatus.Location)
				{
					if(Math.Abs(c.X-clutterCood.X) == 1)
					{
						if(c.X > clutterCood.X)//i M higher move left
						{
							cmds.Add (new Command { vesselid = 1, action = "move:west", coordinate = new Coordinate { X = 1, Y = 1 } });
						}
						else//move right
						{
							cmds.Add (new Command { vesselid = 1, action = "move:east", coordinate = new Coordinate { X = 1, Y = 1 } });
						}
					}
					if(Math.Abs(c.Y - clutterCood.Y) == 1)
					{
						if(c.Y > clutterCood.Y)//i M higher move down
						{
							cmds.Add (new Command { vesselid = 1, action = "move:south", coordinate = new Coordinate { X = 1, Y = 1 } });
						}
						else//move right
						{
							cmds.Add (new Command { vesselid = 1, action = "move:north", coordinate = new Coordinate { X = 1, Y = 1 } });
						}
					}
				}
			}
		}

		// Do NOT modify or remove! This is where you will receive the new board status after each round.
		public void GetBoardStatus (BoardStatus board)
		{
			_currentBoard = board;
		}

		// This method runs at the start of a new game, do any initialization or resetting here 
		public void Reset ()
		{

		}
	}
}