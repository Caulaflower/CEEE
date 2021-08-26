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
using RimWorld.Planet;
using UnityEngine;

namespace magazynier
{
    class AttachBipod : JobDriver
    {
      

       
        private const TargetIndex BenchTogo = TargetIndex.A;
        private const TargetIndex gun = TargetIndex.B;
        private const TargetIndex bipod = TargetIndex.C;
        private const int NuzzleDuration = 500;
        public ThingWithComps gunthingwithcomps;


        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(BenchTogo), this.job, 1, -1, null);
        }
     

        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.GotoThing(bipod, PathEndMode.OnCell);
            yield return Toils_Haul.TakeToInventory(bipod, 1);
            yield return Toils_Goto.GotoThing(gun, PathEndMode.OnCell);
            yield return Toils_Haul.StartCarryThing(gun);
            yield return Toils_Haul.CarryHauledThingToCell(BenchTogo);
            yield return Toils_Goto.GotoThing(BenchTogo, PathEndMode.InteractionCell);

            Toil toil = Toils_General.Wait(120);
            toil.AddFinishAction(delegate
            {
                gunthingwithcomps = TargetThingB as ThingWithComps;
                gunthingwithcomps.AllComps.Add(new BipodComp {});
                gunthingwithcomps.TryGetComp<BipodComp>().bipodattached = (ThingWithComps)TargetC.Thing;
                pawn.inventory.innerContainer.ToList().Find(S => S.def == TargetC.Thing.def && S.stackCount == 1).Destroy();
                gunthingwithcomps.TryGetComp<MagazineUser>().CheckForThingy();
                
               
               


            });
            yield return toil;
          
          
        }
    }
    class SetUpBipod : JobDriver
    {



        private const TargetIndex bipodgun = TargetIndex.A;
        private const int NuzzleDuration = 500;
        public ThingWithComps gunthingwithcomps;
        public BipodComp gunsbipod
        {
            get
            {
                return gunthingwithcomps.TryGetComp<BipodComp>();
            }
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(bipodgun), this.job, 1, -1, null);
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {
            gunthingwithcomps = TargetA.Thing as ThingWithComps;
            int benz = (int)TargetA.Thing.TryGetComp<BipodComp>().bipodattached.GetStatValue(BipodStatDefOf.timetosetupthebipod);
            //Log.Error((60 * benz).ToString());

            Toil toil = Toils_General.Wait(60 * benz) ;
            
            toil.AddPreInitAction(delegate
            {
               
            });
            toil.AddFinishAction(delegate
            {
                gunsbipod.IsBipodSetUp = true;
               

            });
            yield return toil;


        }
    }

}
