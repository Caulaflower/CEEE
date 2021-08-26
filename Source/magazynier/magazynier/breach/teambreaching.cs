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
using Verse.Sound;
using UnityEngine;

namespace magazynier.breach
{
   public class BreachTeamComp : ThingComp
   {
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            List<FloatMenuOption> cringe = new List<FloatMenuOption>();
            if(selPawn.TryGetComp<magazynier.Player_teams.ControllerMGComp>()?.friends?.Any(s => s.TryGetComp<magazynier.Player_teams.ControllerMGComp>().AmIBreacher) ?? false)
            {
                Pawn breacher = selPawn.TryGetComp<magazynier.Player_teams.ControllerMGComp>().friends.Find(s => s.TryGetComp<magazynier.Player_teams.ControllerMGComp>().AmIBreacher);
                cringe.Add(new FloatMenuOption("Team: Breach", delegate
                {
                    breacher.TryGetComp<magazynier.Player_teams.ControllerMGComp>().busy = true;
                    Job boj = JobMaker.MakeJob(SchizophreniaUtil.idk());
                    boj.targetA = this.parent;
                    boj.verbToUse = breacher.equipment.Primary.TryGetComp<CompEquippable>().PrimaryVerb;
                    breacher.jobs.StartJob(boj, JobCondition.InterruptForced);
                    
                }));
            }
            return cringe;
        }

    }
	public class ShotgunDoor : JobDriver
	{
		private Building_Door Door
		{
			get
			{
				return base.TargetThingA as Building_Door;
			}
		}
		

		
		private const TargetIndex door = TargetIndex.A;



		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{

			return this.pawn.Reserve(this.job.GetTarget(door), this.job, 1, -1, null);
		}
		

		protected override IEnumerable<Toil> MakeNewToils()
		{

            Toil toil1 = Toils_Goto.GotoCell(Door.Position, PathEndMode.Touch);
            Verb_ShootWithMag jh = GetActor().equipment.Primary.TryGetComp<CompEquippable>().PrimaryVerb as Verb_ShootWithMag;
           
            toil1.AddPreTickAction(delegate 
            {

              
                Job fgh = JobMaker.MakeJob(JobDefOf.Goto);
                fgh.targetA = GetActor();
                GetActor().TryGetComp<magazynier.Player_teams.ControllerMGComp>().dudetofollow.jobs.StartJob(fgh, JobCondition.InterruptForced);
                Pawn leader = GetActor().TryGetComp<magazynier.Player_teams.ControllerMGComp>().dudetofollow;
                List<Pawn> friens = leader.TryGetComp<magazynier.Player_teams.ControllerMGComp>().friends.Where(L => L != GetActor()).ToList();
                foreach (Pawn fren in friens)
                {
                    fren.TryGetComp<magazynier.Player_teams.ControllerMGComp>().busy = true;
                    Job job = JobMaker.MakeJob(JobDefOf.Goto);
                    job.targetA = leader.RandomAdjacentCell8Way();
                    fren.jobs.StartJob(job, JobCondition.InterruptForced);
                }


            });
           
            
            yield return toil1;
            //Toil toil2 = Toils_General.Wait(120);
        
            Toil toil2 = Toils_Combat.CastVerb(door);
          



            yield return toil2;
            toil2.AddFinishAction(delegate
            {
               
                BipodStatDefOf.breachsound.PlayOneShot(SoundInfo.InMap(GetActor(), MaintenanceType.None));
             
                Door.StartManualOpenBy(GetActor());
                //GetActor().jobs.EndCurrentJob(JobCondition.Succeeded);


                //GetActor().TryGetComp<magazynier.Player_teams.ControllerMGComp>().busy = false; 
            });
            Toil toil3 = Toils_General.Wait(1);
            yield return toil3;
           

		}



	}
	public static class SchizophreniaUtil
    {
        public static Action schizo1()
        {
            return delegate
            {
              
            };
        }
        public static JobDef idk()
        {
            return DefDatabase<JobDef>.AllDefs.ToList().Find(v => v.defName == "shootoutdoor");
        }
        public static List<IntVec3> schizotiles(Building_Door building)
        {
            List<IntVec3> pobb = new List<IntVec3>();
            pobb.AddRange(building.CellsAdjacent8WayAndInside().ToList());
            pobb.Remove(building.Position);
            Rot4 IamGOingInanse = building.Rotation;
            if(IamGOingInanse == Rot4.South)
            {
                Log.Message("idk south"); 
            }
            if (IamGOingInanse == Rot4.North)
            {
                pobb.RemoveAll(o => o.x != building.InteractionCell.x);
                pobb.RemoveAll(d => d.z != building.InteractionCell.z);
                Log.Message("idk north");
                pobb.Add(building.InteractionCell);
            }
            if (IamGOingInanse == Rot4.East)
            {
                //pobb.RemoveAll(o => o.z != building.InteractionCell.z - 1 && o.x != building.InteractionCell.x);
                Log.Message("idk east");
            }
            if (IamGOingInanse == Rot4.West)
            {
                Log.Message("idk west");
            }
            foreach(IntVec3 ghj in pobb)
            {
                Log.Message(ghj.ToString());
            }
           
            return pobb;
        }
    }
}
