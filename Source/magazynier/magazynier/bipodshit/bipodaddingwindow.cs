using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using CombatExtended;
using Verse.AI;
using Verse.Sound;
using UnityEngine;

namespace magazynier
{
	public class BipodAdderComp : ThingComp
	{
		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			yield return new Command_Action()
			{
				defaultLabel = "add a bipod to a gun",
				defaultDesc = "add a bipod to a gun",
				icon = ContentFinder<Texture2D>.Get("Mag/fud1", true),
				action = delegate
				{
					Find.WindowStack.Add(new BipodWinda(this.parent.Map, (Building)this.parent));
				}
			};
			yield return new Command_Action()
			{
				defaultLabel = "convert a gun to full auto",
				defaultDesc = "convert a gun to full auto",
				icon = ContentFinder<Texture2D>.Get("Mag/fud1", true),
				action = delegate
				{
					Find.WindowStack.Add(new placeholderWinda(this.parent.Map, (Building)this.parent));
				}
			};

		}
		public BipodAdderCompP Props
		{
			get
			{
				return (BipodAdderCompP)this.props;
			}
		}
	}
	public class BipodAdderCompP : CompProperties
	{

		public BipodAdderCompP()
		{
			this.compClass = typeof(BipodAdderComp);
		}



	}
	public class BipodWinda : Window
	{
		private static readonly Vector2 Test = new Vector2(100f, 140f);
		
		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(600f, 600f);
			}
		}


		public BipodWinda(Map mapa, Building bench)
		{
			mmap = mapa;
			build = bench;

		}
		public Building build;
		public Map mmap;
		public Texture sometextureidk;
		public ThingWithComps gun;
		public ThingWithComps bipod;
		public override void DoWindowContents(Rect inRect)
		{

			Rect rect1 = new Rect(inRect);
			rect1.width = 100f;
			rect1.height = 100f;
			rect1 = rect1.CenteredOnXIn(inRect);
			rect1 = rect1.CenteredOnYIn(inRect);
			rect1.x += -190f;
			rect1.y += 190f;
			Rect position = new Rect(rect1.xMin + (rect1.width - BipodWinda.Test.x) / 2f - 10f, rect1.yMin + 20f, BipodWinda.Test.x, BipodWinda.Test.y);
			
			if(Widgets.ButtonText(rect1, "Select gun to add bipod to"))
			{
				List<Thing> thingWiths = mmap.listerThings.AllThings.FindAll(A => A is ThingWithComps && A.TryGetComp<CompEquippable>() != null && A.TryGetComp<CompEquippable>().PrimaryVerb is magazynier.Verb_ShootWithMag).ToList();
				List<ThingWithComps> list1 = new List<ThingWithComps> { };
				foreach(Thing abc in thingWiths)
				{
					ThingWithComps cab = (ThingWithComps)abc;
					list1.Add(cab);
				}
	
				var options3 = new List<FloatMenuOption>
				{

				};
				foreach (ThingWithComps thing in list1)
				{
					FloatMenuOption floatmenuoption = new FloatMenuOption(thing.Label, (delegate
					{
						gun = thing;
					}));

					options3.Add(floatmenuoption);
				}
				Find.WindowStack.Add(new FloatMenu(options3));
			}
			Rect rect2 = new Rect(inRect);
			rect2.width = 350f;
			rect2.height = 350f;
			rect2 = rect2.CenteredOnXIn(inRect);
			rect2 = rect2.CenteredOnYIn(inRect);
			rect2.x += 0f;
			rect2.y += 0f;
			Rect position2 = new Rect(rect2.xMin + (rect2.width - BipodWinda.Test.x) / 2f - 10f, rect2.yMin + 20f, BipodWinda.Test.x, BipodWinda.Test.y);
			if(gun != null)
			{
				GUI.DrawTexture(rect2, gun.def.uiIcon);
			}

			Rect rect3 = new Rect(inRect);
			rect3.width = 100f;
			rect3.height = 100f;
			rect3 = rect3.CenteredOnXIn(inRect);
			rect3 = rect3.CenteredOnYIn(inRect);
			rect3.x += 190f;
			rect3.y += 190f;
			Rect position3 = new Rect(rect1.xMin + (rect1.width - BipodWinda.Test.x) / 2f - 10f, rect1.yMin + 20f, BipodWinda.Test.x, BipodWinda.Test.y);

			if (Widgets.ButtonText(rect3, "Select bipod you want to add to the gun"))
			{
				List<Thing> abcdef = mmap.listerThings.AllThings.Where(A => A.def.statBases?.Any(b => b.stat == BipodStatDefOf.WarmupDecrease) ?? false).ToList();
				List<ThingWithComps> list1 = new List<ThingWithComps> { };
				foreach (Thing abc in abcdef)
				{
					ThingWithComps cab = (ThingWithComps)abc;
					list1.Add(cab);
				}

				var options3 = new List<FloatMenuOption>
				{

				};
				foreach (ThingWithComps thing in list1)
				{
					FloatMenuOption floatmenuoption = new FloatMenuOption(thing.Label, (delegate
					{
						bipod = thing;
					}));

					options3.Add(floatmenuoption);
				}
				Find.WindowStack.Add(new FloatMenu(options3));
			}
			Rect rect4 = new Rect(inRect);
			rect4.width = 100f;
			rect4.height = 100f;
			rect4 = rect4.CenteredOnXIn(inRect);
			rect4 = rect4.CenteredOnYIn(inRect);
			rect4.x += 0f;
			rect4.y += 190f;
			Rect position4 = new Rect(rect1.xMin + (rect1.width - BipodWinda.Test.x) / 2f - 10f, rect1.yMin + 20f, BipodWinda.Test.x, BipodWinda.Test.y);
			if(Widgets.ButtonText(rect4, "finish"))
			{
				if(gun != null && bipod != null)
				{
					mmap.mapPawns.FreeColonists.FindAll(P => P.health.capacities.CapableOf(PawnCapacityDefOf.Moving)).RandomElement().jobs.StartJob(new Job { def = BipodStatDefOf.addbipod, targetA = this.build, targetB = gun, targetC = bipod })
				;
				}
				
				this.Close();
			}
		}
	}
	[DefOf]
	public static class BipodStatDefOf
	{
		
		static BipodStatDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(BipodStatDefOf));
		}
		public static JobDef converttofullauto;
		public static StatDef timetosetupthebipod;
		public static StatDef WarmupDecrease;
		public static JobDef addbipod;
		public static JobDef amogus;
		public static StatDef accuracyincreasebipod;
		public static JobDef Ineedtopiss;
		public static SoundDef breachsound;

	}
}