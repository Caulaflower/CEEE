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
    public class GizmoForNades : CombatExtended.CompRangedGizmoGiver
    {
        public override void Notify_Equipped(Pawn pawn)
        {
            VerbPropertiesCE cE1 = this.parent.TryGetComp<CompEquippable>().PrimaryVerb.verbProps as VerbPropertiesCE;
            VerbPropertiesCE cE2 = cE1.MemberwiseClone() as VerbPropertiesCE;
            cE2.range = cE1.range * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation);
            Log.Message((cE1.range * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation)).ToString() + "before");
            this.parent.TryGetComp<CompEquippable>().PrimaryVerb.verbProps = cE2;
            Log.Message(this.parent.TryGetComp<CompEquippable>().PrimaryVerb.verbProps.range.ToString());
            base.Notify_Equipped(pawn);
        }
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            Verb_ShootCEOneUse ammouser = this.parent.TryGetComp<CompEquippable>().PrimaryVerb as Verb_ShootCEOneUse;
            ProjectilePropertiesCE projpropsce = ammouser.verbProps.defaultProjectile.projectile as ProjectilePropertiesCE;

            yield return new Command_Action
            {
                defaultDesc = "idk even",
                icon = ContentFinder<Texture2D>.Get("Bipods/closed_bipod"),
                defaultLabel = check(projpropsce.flyOverhead),
                action = delegate
                {
                    float smth = projpropsce.speed;
                    bool dool = projpropsce.flyOverhead;
                    if (dool)
                    {
                        projpropsce.flyOverhead = false;
                       
                    }
                    else
                    {
                        projpropsce.flyOverhead = true;
                       
                    }
                    



                }
            };
        }
        public string check(bool somebood)
        {
            if (somebood)
            {
                return "Throw over walls";
            }
            else
            {
                return "Throw normally";
            }
        }
    }

    public class GizmoForOtherNades : CombatExtended.CompRangedGizmoGiver
    {
        
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            CompAmmoUser ammouser = this.parent.TryGetComp<CompAmmoUser>();
            ProjectilePropertiesCE projpropsce = ammouser.CurAmmoProjectile.projectile as ProjectilePropertiesCE;
            Log.Message("vghbjnkm");
            yield return new Command_Action
            {
                defaultDesc = "idk even",
                icon = ContentFinder<Texture2D>.Get("Bipods/closed_bipod"),
                defaultLabel = check(projpropsce.flyOverhead),
                action = delegate
                {
                    float smth = projpropsce.speed;
                    bool dool = projpropsce.flyOverhead;
                    if (dool)
                    {
                        projpropsce.flyOverhead = false;

                    }
                    else
                    {
                        projpropsce.flyOverhead = true;

                    }




                }
            };
        }
        public string check(bool somebood)
        {
            if (somebood)
            {
                return "Throw over walls";
            }
            else
            {
                return "Throw normally";
            }
        }
        public string check1(bool somebood)
        {
            if (somebood)
            {
                return "Shoot indirectly";
            }
            else
            {
                return "Shoot directly";
            }
        }
    }
}
