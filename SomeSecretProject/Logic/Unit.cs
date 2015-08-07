﻿using System;
using System.Linq;
using Newtonsoft.Json;

namespace SomeSecretProject.Logic
{
	[JsonObject]
	public class Unit
	{
		[JsonProperty("pivot")]
		public Cell pivot;

		[JsonProperty("members")]
		public Cell[] members;

		public Unit Move(MoveType move, int steps = 1)
		{
			switch (move)
			{
				case MoveType.E:
				case MoveType.W:
				case MoveType.SW:
				case MoveType.SE:
				case MoveType.NE:
				case MoveType.NW:
					return new Unit { pivot = pivot.Move(new Vector(move, pivot.y, steps)), members = members.Select(cell => cell.Move(new Vector(move, cell.y, steps))).ToArray() };
				case MoveType.RotateCW:
					return new Unit { pivot = pivot, members = members.Select(cell => cell.RotateCW(pivot)).ToArray() };
				case MoveType.RotateCCW:
					return new Unit { pivot = pivot, members = members.Select(cell => cell.RotateCCW(pivot)).ToArray() };
				default:
					throw new ArgumentOutOfRangeException(move.ToString(), move, null);
			}
		}

		public bool CheckCorrectness(Map map)
		{
			return !members.Any(cell => cell.x < 0 || cell.y < 0 || cell.x >= map.Width || cell.y >= map.Height || map[cell.x, cell.y].filled);
		}

        //returns positions of: topleft, bottomright
	    public Tuple<Cell, Cell> GetSurroundingRectangle()
	    {
	        var tl = new Cell {x = Int32.MaxValue, y = Int32.MaxValue};
            var br = new Cell { x = Int32.MinValue, y = Int32.MinValue };
			foreach (var cell in members)
	        {
	            tl.x = Math.Min(cell.x, tl.x);
	            tl.y = Math.Min(cell.y, tl.y);
	            br.x = Math.Max(cell.x, br.x);
	            br.y = Math.Max(cell.y, br.y);
	}
	        return Tuple.Create(tl, br);
	    }
	}
}