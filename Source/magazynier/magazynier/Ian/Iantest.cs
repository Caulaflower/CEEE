using System;
using System.Collections.Generic;
using System.Linq;
using CombatExtended.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;
using CombatExtended;
namespace magazynier.Ian
{
	public static class DebugToolsIan
	{
		[DebugAction("CheckIan", "Check Ian", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void CheckIan()
		{
			DefDatabase<IncidentDef>.AllDefs.ToList().Find(G => G.defName == "IanJoins").Worker.TryExecute(new IncidentParms {target = Find.CurrentMap });
			//Log.Message("a");
		}
		[DebugAction("CheckIan", "Check Ian 2", false, false, actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
		private static void CheckIan2()
		{
			DefDatabase<IncidentDef>.AllDefs.ToList().Find(G => G.defName == "IanJoinsree").Worker.TryExecute(new IncidentParms { target = Find.CurrentMap });
			//Log.Message("a");
		}

	}
}
