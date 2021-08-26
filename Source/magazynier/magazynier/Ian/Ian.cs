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
	public class idfkman : IncidentWorker_RaidEnemy
	{
		
		protected override bool CanFireNowSub(IncidentParms parms)
		{
			bool flag = base.CanFireNowSub(parms) && parms.target != null;
			if (flag)
			{
				Map map = (Map)parms.target;
				List<Map> list = (from x in Find.Maps
								  where x.IsPlayerHome
								  select x).ToList<Map>();
				bool flag2 = map != null && list.Contains(map);
				if (flag2)
				{
					return true;
				}
			}
			return false;
		}

		
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			bool flag = parms.target != null;
			if (flag)
			{
				Map map = (Map)parms.target;
				bool flag2 = map != null;
				if (flag2)
				{
					return UtilityEventGunJesus.ProtectionFee(map, parms);
				}
			}
			return false;
		}
	}
	public static class UtilityEventGunJesus
	{
		public static bool ProtectionFee(Map map, IncidentParms parms)
		{
			int fee = 1;
			IEnumerable<Thing> silver = UtilityThingy.GetSilverInHome(map);
			int amountSilverInHome = UtilityThingy.GetAmountSilverInHome(silver);
			if (silver.ToList().Any(C => C.def.defName == "32french"))
			{
				fee = 500;
			}
			
			bool flag = parms.faction == null;
			if (flag)
			{
				parms.faction = Find.FactionManager.RandomEnemyFaction(false, false, false, TechLevel.Undefined);
			}
			DiaOption item = new DiaOption("OK".Translate())
			{
				resolveTree = true
			};
			DiaNode diaNode = new DiaNode("dia1".Translate(parms.faction, fee) + fee.ToString() + " " +  silver.Last().Label);
			DiaNode diaNode2 = new DiaNode("dia2".Translate(parms.faction));
			DiaNode diaNode3 = new DiaNode("dia3".Translate(parms.faction));
			diaNode2.options.Add(item);
			diaNode3.options.Add(item);
			DiaOption diaOption = new DiaOption("dia4".Translate())
			{
				action = delegate ()
				{
					DefDatabase<IncidentDef>.AllDefs.ToList().Find(G => G.defName == "IanJoinsree").Worker.TryExecute(new IncidentParms { target = Find.CurrentMap });
					UtilityThingy.RemoveSilverFromHome(silver, fee);
					
					
				},
				link = diaNode2
			};
			diaNode.options.Add(diaOption);
			DiaOption item2 = new DiaOption("dia5".Translate())
			{
				action = delegate ()
				{
					
					
				},
				link = diaNode3
			};
			diaNode.options.Add(item2);
			bool flag2 = amountSilverInHome < fee;
			if (flag2)
			{
				diaOption.Disable("dia6".Translate());
			}
			Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, "".Translate()));
			Find.Archive.Add(new ArchivedDialog(diaNode.text, "dia7".Translate(), null));
			return true;
		}
	}
	public static class UtilityThingy
	{
		public static int GetAmountSilverInHome(IEnumerable<Thing> silver)
		{
			return silver.Sum((Thing x) => x.stackCount);
		}

	
		public static void RemoveSilverFromHome(IEnumerable<Thing> silver, int debt)
		{
			while (debt > 0 && silver.Count<Thing>() > 0)
			{
				int stackCount = silver.First<Thing>().stackCount;
				bool flag = stackCount >= debt;
				if (flag)
				{
					silver.First<Thing>().SplitOff(debt);
					break;
				}
				debt = -stackCount;
				silver.First<Thing>().Destroy(DestroyMode.Vanish);
			}
		}
		public static IEnumerable<Thing> GetSilverInHome(Map map)
		{
			HashSet<Thing> yieldedThings = new HashSet<Thing>();
			IEnumerable<IntVec3> home = map.areaManager.Home.ActiveCells;
			ThingDef f = DefDatabase<ThingDef>.AllDefs.ToList().Find(P => P.defName == "FAMASGtwo");
			if (Rand.Chance(0.5f))
			{
				f = DefDatabase<ThingDef>.AllDefs.ToList().Find(O => O.defName == "32french");
			}
			if(f == null)
			{
				f = DefDatabase<ThingDef>.AllDefs.ToList().Find(P => P.defName == "FAMASGtwo");
			}
			foreach (IntVec3 cell in home)
			{
				List<Thing> thingList = cell.GetThingList(map);
				int num;
				for (int i = 0; i < thingList.Count; i = num + 1)
				{
					Thing t = thingList[i];
					
					bool flag = t.def == f && !yieldedThings.Contains(t);
					if (flag)
					{
						yieldedThings.Add(t);
						yield return t;
					}
					t = null;
					num = i;
				}
				thingList = null;
				
			}
			
			yield break;
			
			
		}
	}
	[DefOf]
	public static class IanDefOf
	{

		


	}
}
