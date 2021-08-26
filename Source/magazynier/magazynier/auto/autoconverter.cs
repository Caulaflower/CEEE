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

namespace magazynier
{
	public class placeholderWinda : Window
	{
		private static readonly Vector2 Test = new Vector2(100f, 140f);

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(600f, 600f);
			}
		}


		public placeholderWinda(Map mapa, Building bench)
		{
			mmap = mapa;
			build = bench;

		}
		public Building build;
		public Map mmap;
		public Texture sometextureidk;
		public ThingWithComps gun;
		public ThingWithComps placeholder;
		public override void DoWindowContents(Rect inRect)
		{

			Rect rect1 = new Rect(inRect);
			rect1.width = 100f;
			rect1.height = 100f;
			rect1 = rect1.CenteredOnXIn(inRect);
			rect1 = rect1.CenteredOnYIn(inRect);
			rect1.x += -190f;
			rect1.y += 190f;
			Rect position = new Rect(rect1.xMin + (rect1.width - placeholderWinda.Test.x) / 2f - 10f, rect1.yMin + 20f, placeholderWinda.Test.x, placeholderWinda.Test.y);

			if (Widgets.ButtonText(rect1, "Select gun to convert to full auto"))
			{

				List<Thing> thingWiths = mmap.listerThings.AllThings.FindAll(A => A is ThingWithComps && A.TryGetComp<CompFireModes>() != null && A.TryGetComp<CompFireModes>().Props.aimedBurstShotCount <= 2 && A.TryGetComp<CompEquippable>().PrimaryVerb is magazynier.Verb_ShootWithMag && !A.TryGetComp<MagazineUser>().convertedtofullauto).ToList();
				List<ThingWithComps> list1 = new List<ThingWithComps> { };
				foreach (Thing abc in thingWiths)
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
			Rect position2 = new Rect(rect2.xMin + (rect2.width - placeholderWinda.Test.x) / 2f - 10f, rect2.yMin + 20f, placeholderWinda.Test.x, placeholderWinda.Test.y);
			if (gun != null)
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
			Rect position3 = new Rect(rect1.xMin + (rect1.width - placeholderWinda.Test.x) / 2f - 10f, rect1.yMin + 20f, placeholderWinda.Test.x, placeholderWinda.Test.y);

			
			Rect rect4 = new Rect(inRect);
			rect4.width = 100f;
			rect4.height = 100f;
			rect4 = rect4.CenteredOnXIn(inRect);
			rect4 = rect4.CenteredOnYIn(inRect);
			rect4.x += 0f;
			rect4.y += 190f;
			Rect position4 = new Rect(rect1.xMin + (rect1.width - placeholderWinda.Test.x) / 2f - 10f, rect1.yMin + 20f, placeholderWinda.Test.x, placeholderWinda.Test.y);
			if (Widgets.ButtonText(rect4, "finish"))
			{
				mmap.mapPawns.FreeColonists.FindAll(P => P.health.capacities.CapableOf(PawnCapacityDefOf.Moving)).RandomElement().jobs.StartJob(new Job { def = BipodStatDefOf.converttofullauto, targetA = gun, targetB = this.build});
				//magazynier.Verb_ShootWithMag abcdef = (magazynier.Verb_ShootWithMag)gun.TryGetComp<CompEquippable>().PrimaryVerb;
				//gun.TryGetComp<MagazineUser>().convertedtofullauto = true;


				this.Close();
			}
		}
	}


	class ConvertToFull : JobDriver
	{



		private const TargetIndex autogun = TargetIndex.A;
		private const TargetIndex bench = TargetIndex.B;
		private const int NuzzleDuration = 500;
		public ThingWithComps gunthingwithcomps;
		public MagazineUser gunsmaguser
		{
			get
			{
				return gunthingwithcomps.TryGetComp<MagazineUser>();
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return this.pawn.Reserve(this.job.GetTarget(autogun), this.job, 1, -1, null);
		}


		protected override IEnumerable<Toil> MakeNewToils()
		{
			gunthingwithcomps = TargetA.Thing as ThingWithComps;
			yield return Toils_Haul.StartCarryThing(autogun);
			yield return Toils_Haul.CarryHauledThingToCell(bench);
			yield return Toils_Goto.Goto(bench, PathEndMode.InteractionCell);
			Toil toil = Toils_General.Wait(600);
			
			toil.AddFinishAction(delegate
			{
				gunsmaguser.convertedtofullauto = true;
			});
			yield return toil;


		}
	}
}
