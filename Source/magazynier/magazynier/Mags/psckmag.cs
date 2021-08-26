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
	public class PackMag : JobDriver
	{
		private AmmoThing ammunition
		{
			get
			{
				return base.TargetThingA as AmmoThing;
			}
		}
		private ThingWithComps magazine
		{
			get
			{
				return base.TargetThingB as ThingWithComps;

			}
		}

		private const TargetIndex AmmoToGoTo = TargetIndex.A;
		private const TargetIndex MagazineToLoad = TargetIndex.B;



		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{

			return this.pawn.Reserve(this.job.GetTarget(AmmoToGoTo), this.job, 1, -1, null);
		}
		public Gazine MagazineThingComp
		{
			get
			{
				return this.magazine.TryGetComp<Gazine>();
			}
		}
		
		protected override IEnumerable<Toil> MakeNewToils()
		{

			int Obamium = MagazineThingComp.MagazineSize - MagazineThingComp.loadedAmmoAmount;
			int Korwinium = new int();
			Toil toil1 = Toils_Goto.GotoThing(AmmoToGoTo, PathEndMode.OnCell);
			yield return toil1;
			Toil toil2 = Toils_Haul.TakeToInventory(AmmoToGoTo, Obamium);
			toil2.AddFinishAction(delegate 
			{
				Korwinium = pawn.inventory.innerContainer.ToList().Find(ABC => ABC.def == ammunition.def).stackCount;
			});
			yield return toil2;
			Toil toil4 = Toils_Goto.GotoThing(MagazineToLoad, PathEndMode.OnCell);
			yield return toil4;
			Toil toil3 = Toils_General.Wait(30 * Obamium);
			toil3.AddFinishAction(delegate	
			{
				//Log.Error(Korwinium.ToString() +" korwinium");
				if(Obamium >= Korwinium)
				{
					//Log.Error(Obamium.ToString());
					if(Obamium != Korwinium)
					{
						pawn.inventory.innerContainer.ToList().Find(ABC => ABC.def == ammunition.def).stackCount -= Korwinium;
						if(pawn.inventory.innerContainer.ToList().Find(ABC => ABC.def == ammunition.def).stackCount <= 0)
						{
							pawn.inventory.innerContainer.ToList().Find(ABC => ABC.def == ammunition.def).Destroy();
						}
					}
					else
					{
						pawn.inventory.innerContainer.ToList().Find(ABC => ABC.def == ammunition.def).Destroy();
					}
					
					MagazineThingComp.loadedAmmo = ammunition.def as AmmoDef;
					MagazineThingComp.loadedAmmoAmount += Korwinium;
				}
				else
				{
					
					MagazineThingComp.loadedAmmo = ammunition.def as AmmoDef;
					MagazineThingComp.loadedAmmoAmount += pawn.inventory.innerContainer.ToList().Find(ABC => ABC.def == ammunition.def).stackCount; 
					pawn.inventory.innerContainer.ToList().Find(ABC => ABC.def == ammunition.def).Destroy();
				}
				
				

			});
			yield return toil3;
			

			
			


		}



	}

	

}
