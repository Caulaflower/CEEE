using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using RimWorld;
using CombatExtended;
using HarmonyLib;
using HarmonyMod;
using UnityEngine;
using Verse.Sound;
namespace magazynier.Player_teams
{
	public static class idkutil
	{
		public static List<Pawn> dudes(Pawn pawn)
		{
			return Find.CurrentMap.mapPawns.AllPawns.FindAll(oof => oof.Faction == pawn.Faction);

		}
		public static void juice(List<Pawn> juiice, Pawn leader)
		{
			foreach (Pawn pawn in juiice)
			{
				if(pawn.TryGetComp<ControllerMGComp>() != null)
				{
					if (Rand.Chance(0.33f))
					{
						pawn.TryGetComp<ControllerMGComp>().dudetofollow = leader;
					}
					
				}
			}
		}
		public static List<Room> roomstoattack()
		{
			List<Room> restul = new List<Room>();
			List<Pawn> colonistst = Find.CurrentMap.mapPawns.AllPawns.FindAll(O => O.Faction == Faction.OfPlayer);
			foreach(Pawn pwan in colonistst)
			{
				if (pwan.GetRoom() != null)
				{
					Log.Message(pwan.GetRoom().ToString());
					restul.Add(pwan.GetRoom());
				}
				
			}
			//Log.Message(restul.First().BorderCells.First().ToString());
			//Log.Message(restul.First().BorderCells.ToList().Find(frghjk => frghjk.GetThingList(Find.CurrentMap).Any(P => P.def == ThingDefOf.Door)).ToString() ?? "frgthjkl");
			return restul;
		}
		//public static List<Pawn> closedudes
	}



}
