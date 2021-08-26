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

namespace magazynier.Player_teams
{
    public class ControllerMGComp : ThingComp
    {
        public Pawn dudetofollow;
        public int egg;
        public bool busy = false;
        public List<Thing> myinventory
        {
            get
            {
                Pawn myself = (Pawn)this.parent;
                
                return myself.inventory.innerContainer.ToList();
            }
        }
        public bool AIAMILEADER = false;
        public bool AmIBreacher
        {
            get
            {
                Pawn myself = (Pawn)this.parent;
              
                CombatExtended.CompAmmoUser ammo = new CompAmmoUser();
                bool result = false;
                if (myself.equipment.Primary == null)
                {
                    result = false;
                }
                else
                {
                    var dood = myself.equipment.Primary.TryGetComp<MagazineUser>();
                    if(dood == null)
                    {
                        var dooder = myself.equipment.Primary.TryGetComp<CompAmmoUser>();
                        result = false;
                        if (dooder != null)
                        {
                            if (dooder.CurrentAmmo.ammoClass.defName == "BuckShot")
                            {
                                result = true;
                            }

                        }
                    }
                    else
                    {
                        if(dood.CurrentAmmo.ammoClass.defName == "BuckShot")
                        {
                            result = true;
                        }
                        else
                        {
                            result = false;
                        }
                       
                    }
                }
               
                return result;
            }
        }
        public List<Pawn> friends
        {
            get
            {
                Pawn myself = (Pawn)this.parent;
                return Find.CurrentMap.mapPawns.FreeColonists.FindAll(P => P.TryGetComp<ControllerMGComp>().dudetofollow == myself);
            }
        }
        public bool AmIAmmoMan
        {
            get
            {
                Pawn myself = (Pawn)this.parent;
                MagazineUser mag = new MagazineUser();
                bool result = false;
                if(dudetofollow != null && dudetofollow.equipment.Primary != null && dudetofollow.equipment.Primary.TryGetComp<MagazineUser>() != null)
                {
                    mag = dudetofollow.equipment.Primary.TryGetComp<MagazineUser>();
                    result = myself.inventory.innerContainer.Any(O => O.TryGetComp<Gazine>() != null && O.TryGetComp<Gazine>().well == mag.well);
                }
                //Log.Message("Am I amm man: " + result.ToString());
                return result;
            }
        }
        public List<Pawn> rangefinders
        {
            get
            {
                return friends.FindAll(L => L.TryGetComp<ControllerMGComp>().AmIRangeFinder);
            }
        }
        public float baserange
        {
            get 
            {
                var verbprops = (VerbPropertiesCE)dudetofollow.equipment.Primary.def.Verbs.Find(P => P is VerbPropertiesCE);
                Log.Message(verbprops.range.ToString() + " baserange");
                return verbprops.range;
            }
        }
        public float staticrange;
        public float change;
        public bool AmIRangeFinder
        {
            get
            {
                Pawn myself = (Pawn)this.parent;
               
                if (myself.equipment.Primary != null)
                {
                    bool result = false;
                    result = myself.equipment.Primary.def == CE_ThingDefOf.Gun_BinocularsRadio;
                    return result;
                }
                else
                {
                    return false;
                }
                
               
            }
        }
        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
        {
            return base.CompFloatMenuOptions(selPawn);

        }
        public override IEnumerable<FloatMenuOption> CompMultiSelectFloatMenuOptions(List<Pawn> selPawns)
        {
            return base.CompMultiSelectFloatMenuOptions(selPawns);
        }
        public int opl = 10;
        public override void CompTick()
        {
            Pawn me = (Pawn)this.parent;
            if (opl != 2)
            {
                
               
                if(opl == 4)
                {
                    
                    if(me.equipment.Primary != null)
                    {
                        if(me.equipment.Primary.TryGetComp<CompAmmoUser>() != null)
                        {
                            if (me.equipment.Primary.TryGetComp<CompAmmoUser>().CurrentAmmo.ammoClass.defName == "BuckShot")
                            {
                                if (me.Faction != Faction.OfPlayer)
                                {
                                    //Log.Error(me.Name.ToString());
                                    if (idkutil.dudes(me).Count > 0)
                                    {
                                        Pawn ghj = idkutil.dudes(me).RandomElement();
                                        Log.Message(ghj.Name.ToString());
                                        idkutil.juice(idkutil.dudes(me), ghj);
                                        Log.Error(idkutil.roomstoattack().ToString());
                                    }

                                }
                            }
                        }
                       
                    }
                    
                   
                }
               
                opl--;
            }

            ///if (me.Faction == Faction.OfPlayer | dudetofollow != null && me.jobs.curJob.def != JobDefOf.FollowClose && me.jobs.curJob.def != JobDefOf.AttackStatic && !dudetofollow.IsAdjacentToCardinalOrInside(me) && !dudetofollow.Position.AdjacentTo8WayOrInside(me.Position) && !dudetofollow.Dead && !busy)
            ///{
               /// me.jobs.StartJob(new Job { def = JobDefOf.FollowClose, targetA = dudetofollow, followRadius = 1 }, JobCondition.InterruptForced);
            ///}
            if (me.Faction == Faction.OfPlayer && dudetofollow != null && dudetofollow.Position.AdjacentTo8WayOrInside(me.Position) && !dudetofollow.Dead && AmIRangeFinder && dudetofollow.equipment.Primary != null)
            {

               
                if (egg != 1)
                {
                    var verbprops = (VerbPropertiesCE)dudetofollow.equipment.Primary.TryGetComp<CompEquippable>().PrimaryVerb.verbProps;
                    change = verbprops.range;
                    var ghj = verbprops.MemberwiseClone();
                    
                    ghj.range = verbprops.range + me.skills.skills.Find(O => O.def == SkillDefOf.Shooting).levelInt;
                    dudetofollow.equipment.Primary.TryGetComp<CompEquippable>().PrimaryVerb.verbProps = ghj;
                    //change = ghj.range - verbprops.range;
                    Log.Message(dudetofollow.equipment.Primary.TryGetComp<CompEquippable>().PrimaryVerb.verbProps.range.ToString());
                        
                    //Log.Message("Is Karim great :" + (ghj.range == 1).ToString());
                    ++egg;

                }


                

            }

            if (me.Faction == Faction.OfPlayer && dudetofollow != null && dudetofollow.Position.AdjacentTo8WayOrInside(me.Position) && !dudetofollow.Dead && AmIAmmoMan && dudetofollow.equipment.Primary != null)
            {
                var dudeslist = dudetofollow.inventory.innerContainer.ToList();
                var mylist = me.inventory.innerContainer.ToList();
                var maglist = dudeslist.Where(K => K.TryGetComp<Gazine>() != null).ToList();
                var something = dudetofollow.equipment.Primary.TryGetComp<MagazineUser>().well;

            }



            base.CompTick();
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if(dudetofollow != null)
            {
                yield return new Command_Action
                {
                    icon = ContentFinder<Texture2D>.Get("Guns/Gluger/512Gluger"),
                    defaultLabel = "Split up from team",
                    action = delegate
                    {
                        if (AmIRangeFinder)
                        {
                            busy = false;
                            var verbprops = (VerbPropertiesCE)dudetofollow.equipment.Primary.TryGetComp<CompEquippable>().PrimaryVerb.verbProps;
                            verbprops.range = change;
                            Log.Message(verbprops.range.ToString());
                            egg = 0;
                        }
                      
                        dudetofollow = null;
                       
                    }
                };
            }
        }
    }
    [HarmonyPatch(typeof(FloatMenuMakerMap), "AddHumanlikeOrders")]
    public static class FloatMenuMakerCarryAdder
    {

        [HarmonyPostfix]
        public static void AddHumanlikeOrdersPostfix69(Vector3 clickPos, Pawn pawn, List<FloatMenuOption> opts)
        {
            bool flag = !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) && !pawn.Drafted;
            if (!flag)
            {
                foreach (LocalTargetInfo localTargetInfo in GenUI.TargetsAt(clickPos, TargetingParameters.ForArrest(pawn), true, null))
                {
                    Pawn mate = (Pawn)localTargetInfo.Thing;

                    bool complexcheck = mate.TryGetComp<ControllerMGComp>().rangefinders == null | mate.TryGetComp<ControllerMGComp>().rangefinders.Count < 2 | mate.TryGetComp<ControllerMGComp>().friends?.Count < 7;
                    if (mate.TryGetComp<ControllerMGComp>().dudetofollow != null | mate == null | mate.Dead | mate.Faction != Faction.OfPlayer | mate.equipment.Primary == null | !complexcheck | !mate.equipment.Primary?.def.weaponTags?.Any(G => G == "SniperRifle" | G == "CE_MachineGun" | G == "GunHeavy") ?? true )
                    {
                        return;
                    }
                    else
                    {
                        opts.Add(FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("Squad up", delegate { pawn.drafter.Drafted = true; pawn.TryGetComp<ControllerMGComp>().dudetofollow = mate;  pawn.jobs.StartJob(new Job { def = JobDefOf.FollowClose, targetA = mate, followRadius = 1 }, JobCondition.InterruptForced); }, MenuOptionPriority.Low, null, mate, 0f, null, null), pawn, mate, "Already teamed up with" ));
                    }
                    
                      
                }
            }
        }
    }
}
