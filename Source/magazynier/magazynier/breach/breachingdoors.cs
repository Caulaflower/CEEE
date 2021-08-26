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

namespace magazynier
{
    class kickoutdoor : JobDriver
    {



        private const TargetIndex Door = TargetIndex.A;
        
        


        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return this.pawn.Reserve(this.job.GetTarget(Door), this.job, 1, -1, null);
        }


        protected override IEnumerable<Toil> MakeNewToils()
        {
            yield return Toils_Goto.Goto(Door, PathEndMode.Touch);
            Building_Door door2 = TargetA.Thing as Building_Door;
            Toil foil = Toils_General.Wait(60);
            foil.AddFinishAction(delegate 
            {
                
                if (door2.HitPoints > 2 * GetActor().skills.GetSkill(SkillDefOf.Melee).Level)
                {
                    BipodStatDefOf.breachsound.PlayOneShot(SoundInfo.InMap(GetActor(), MaintenanceType.None));
                    door2.HitPoints -= 2 * GetActor().skills.GetSkill(SkillDefOf.Melee).Level;
                   

                }
                else
                {
                    BipodStatDefOf.breachsound.PlayOneShot(SoundInfo.InMap(GetActor(), MaintenanceType.None));
                    door2.StartManualOpenBy(GetActor());

                }
               
               
            });
            yield return foil;
          


        }
    }
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    public static class FloatMenuMakerCarryAdder
    {
       
        [HarmonyPostfix]
        public static void AddHumanlikeOrdersPostfix(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            bool flag = !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && !pawn.Drafted;
            if (!flag)
            {
                foreach (LocalTargetInfo localTargetInfo in GenUI.TargetsAt(clickPos, TargetingParameters.ForBuilding(), true, null))
                {
                    ThingWithComps thingWith = localTargetInfo.Thing as ThingWithComps;
                    bool faj = thingWith.TryGetComp<Gazine>() != null;
                    bool flager = localTargetInfo.Thing is Building_Door;
                    if (flager)
                    {
                        Building_Door target = (Building_Door)localTargetInfo.Thing;

                        bool flag3 = !pawn.CanReserveAndReach(target, PathEndMode.ClosestTouch, Danger.Deadly, 1, -1, null, true);
                        if (!flag3 && flager)
                        {
                            JobDef amongsus = BipodStatDefOf.amogus;
                            Action action = delegate ()
                            {
                                Job job = new Job(amongsus, target);
                                job.count = 1;
                                pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                            };
                            string label = "kick out a door";
                            opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(label, action, MenuOptionPriority.Low, null, target, 0f, null, null), pawn, target, "Already being kicked in by: "));
                        }
                    }
                    

                }
            }
        }
    }
}
