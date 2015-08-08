using System;
using System.Collections.Generic;
using System.Linq;
using SomeSecretProject.IO;
using SomeSecretProject.Logic;

namespace SomeSecretProject.Algorithm
{
	public class MuggleProblemSolver_MultiUnit : MuggleProblemSolver
	{
		private readonly int unitsAhead;
		private List<Tuple<Unit, IList<MoveType>>> nextUnitsPositions;

		public MuggleProblemSolver_MultiUnit(int unitsAhead)
		{
			this.unitsAhead = unitsAhead;
		}

		public override string Solve(Problem problem, int seed, string[] powerPhrases)
		{
			var solution = "";
			var game = new SolverGame(problem, seed, powerPhrases);
			while (true)
			{
				switch (game.state)
				{
					case GameBase.State.WaitUnit:
						game.Step();
						break;
					case GameBase.State.UnitInGame:
						if (nextUnitsPositions == null || !nextUnitsPositions.Any())
						{
							var bestPositions = FindBestPositions_Recursive(unitsAhead, game.map.Clone(),
								new[] {game.currentUnit}.Concat(game.GetAllRestUnits()).ToArray(), 0);
							nextUnitsPositions = bestPositions.Item2.ToList();
						}
						var wayToBestPosition = nextUnitsPositions.First();
						nextUnitsPositions = nextUnitsPositions.Skip(1).ToList();

						var unitSolution = staticPowerPhraseBuilder.Build(wayToBestPosition.Item2);
						CallEvent(game, unitSolution);
						game.ApplyUnitSolution(unitSolution);
						solution += unitSolution;
						break;
					case GameBase.State.EndInvalidCommand:
					case GameBase.State.EndPositionRepeated:
						throw new InvalidOperationException(string.Format("Invalid state: {0}", game.state));
					case GameBase.State.End:
						return solution;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}

		private static Tuple<double, Tuple<Unit, IList<MoveType>>[]> FindBestPositions_Recursive(int unitsAhead, Map map,
			Unit[] units, int unitIndex)
		{
			if (unitsAhead < 0 || unitIndex >= units.Length)
			{
				return Tuple.Create(0.0, new Tuple<Unit, IList<MoveType>>[0]);
			}
			var reachablePositions = new ReachablePositions(map);
			var evaluatePositions = new EvaluatePositions2(map);
			var unit = units[unitIndex];
			var endPositions = reachablePositions.SingleEndPositions(unit);
			var estimated = new Dictionary<Unit, double>();
			double? bestScore = null;
			var topOrderedPositions = endPositions
				.Select(p =>
				        {
					        double value;
					        if (estimated.TryGetValue(p.Item1, out value))
						        return Tuple.Create(value, p);
					        var score = estimated[p.Item1] = evaluatePositions.Evaluate(p.Item1);
					        return Tuple.Create(score, p);
				        })
				.OrderByDescending(t => t.Item1)
				.TakeWhile(tt =>
				           {
					           if (!bestScore.HasValue)
					           {
						           bestScore = tt.Item1;
						           return true;
					           }
					           return Math.Abs(tt.Item1 - bestScore.Value) < 1e-6;
				           }).ToList();

			if (!topOrderedPositions.Any())
			{
				return Tuple.Create(-1e3, new Tuple<Unit, IList<MoveType>>[0]);
			}

			var bestPosistions = Tuple.Create(double.MinValue, new Tuple<Unit, IList<MoveType>>[0]);
			foreach (var position in topOrderedPositions)
			{
				var newMap = map.Clone();
				newMap.LockUnit(position.Item2.Item1);
				newMap.RemoveLines();

				var nextPositions = FindBestPositions_Recursive(unitsAhead - 1, newMap, units, unitIndex + 1);

				var score = position.Item1 + nextPositions.Item1;
				if (bestPosistions.Item1 < score)
				{
					bestPosistions = Tuple.Create(score, new[] {position.Item2}.Concat(nextPositions.Item2).ToArray());
				}
			}

			return bestPosistions;
		}
	}
}