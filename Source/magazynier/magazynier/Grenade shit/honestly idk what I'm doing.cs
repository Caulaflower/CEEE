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


namespace magazynier.Grenade_shit
{
	public class SficzBeltSomething : JobDriver
	{
		private ThingWithComps weapon
		{
			get
			{
				return base.TargetThingA as ThingWithComps;
			}
		}
		private ThingWithComps magazine
		{
			get
			{
				return base.TargetThingB as ThingWithComps;

			}
		}

		private const TargetIndex amogusImgoinginsane = TargetIndex.B;



		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{

			return this.pawn.Reserve(this.job.GetTarget(amogusImgoinginsane), this.job, 1, -1, null);
		}
		public Gazine MagazineThingComp
		{
			get
			{
				return this.magazine.TryGetComp<Gazine>();
			}
		}
		public MagazineUser MagazineUserComp
		{
			get
			{
				return this.weapon.TryGetComp<MagazineUser>();
			}
		}
		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil idfk = Toils_Goto.Goto(TargetIndex.B, PathEndMode.ClosestTouch);
			yield return idfk;
			Toil somethink = Toils_Haul.TakeToInventory(TargetIndex.B, 1);
			yield return somethink;
			
			Log.Message(TargetIndex.C.ToString());
			Toil foil = Toils_Goto.Goto(TargetIndex.C, PathEndMode.InteractionCell);
			yield return foil;

			Toil toil = new Toil();


			toil = Toils_General.Wait(60 * TargetB.Thing.TryGetComp<Gazine>().reloadTime);




			toil.AddFinishAction(delegate
			{
				if (MagazineUserComp.loadedmag != null)
				{
					//Log.Error(MagazineUserComp.loadedmag.ToString());

					Thing nic = new Thing();
					MagazineUserComp.loadedmag.stackCount = 1;
					ThingWithComps Magazynierowanie = MagazineUserComp.loadedmag;

					GenThing.TryDropAndSetForbidden(Magazynierowanie, this.pawn.Position, this.Map, ThingPlaceMode.Direct, out nic, false);
					Magazynierowanie.TryGetComp<Gazine>().loadedAmmoAmount = MagazineUserComp.CurMagCount;
					Magazynierowanie.TryGetComp<Gazine>().loadedAmmo = MagazineUserComp.CurrentAmmo;

				}
				MagazineUserComp.magazinesize = magazine.TryGetComp<Gazine>().MagazineSize;
				MagazineUserComp.curMagCountInt = magazine.TryGetComp<Gazine>().loadedAmmoAmount;
				MagazineUserComp.currentAmmoInt = magazine.TryGetComp<Gazine>().loadedAmmo;
				MagazineUserComp.CurrentAmmo = magazine.TryGetComp<Gazine>().loadedAmmo;
				MagazineUserComp.Props.magazineSize = magazine.TryGetComp<Gazine>().MagazineSize;
				//Log.Message(MagazineUserComp.CurrentAmmo.ToString());



				//Log.Message(magazine.ToString());
				MagazineUserComp.loadedmag = magazine;

				//Log.Message(MagazineUserComp.loadedmag.ToString());
				if (magazine.stackCount > 1)
				{
					--magazine.stackCount;
				}
				else
				{
					magazine.Destroy();
				}

			});

			yield return toil;


		}



	}
}
