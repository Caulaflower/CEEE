using CombatExtended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;
using UnityEngine;
namespace magazynier
{
   public class BipodComp : CompRangedGizmoGiver
    {
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			if (!ShouldSetUpBipod)
			{
				yield return new Command_Action
				{
					defaultLabel = "Set up bipod",
					defaultDesc = "Set up bipod",

					icon = ContentFinder<Texture2D>.Get("Bipods/closed_bipod"),
					action = delegate
					{
						ShouldSetUpBipod = true;
					}
				};
			}
			else
			{
				yield return new Command_Action
				{
					defaultLabel = "Close bipod",
					defaultDesc = "Close bipod",
					icon = ContentFinder<Texture2D>.Get("Bipods/open_bipod"),
					action = delegate
					{
						ShouldSetUpBipod = false;
					}
				};
			}
				
		}
		public BipodCompP Props
		{
			get
			{
				return (BipodCompP)this.props;
			}
		}
		public bool IsBipodSetUp;
		public ThingWithComps bipodattached;
		public ThingDef Defd;
		public bool ShouldSetUpBipod;
	}
	public class BipodCompP : CompProperties
	{

		public BipodCompP()
		{
			this.compClass = typeof(BipodComp);
		}


	
	}
}
