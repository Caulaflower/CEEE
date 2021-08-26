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

namespace magazynier.Mags
{
   public class AddInMags : ThingComp
    {
        public Pawn dad
        {
            get
            {
                return this.parent as Pawn;
            }
        }
        public void CreateMags()
        {
            Log.Error("ABCDEF");
            LoadoutPropertiesExtension props = dad.kindDef.GetModExtension<LoadoutPropertiesExtension>();
            Log.Message(props.ToString());
            MagazineUser maguser = (MagazineUser)dad.equipment.Primary.AllComps.Find(C => C is MagazineUser);
            Log.Message(maguser.ToString());
            MagWellDef well = maguser.Props.well;
            Log.Message(well.defName);
            List<ThingDef> defs = DefDatabase<ThingDef>.AllDefs.ToList().Where(F => F.comps.Any(P => P is GasineProp)).ToList();
            List<ThingDef> defs2 = defs.ListFullCopy();
            foreach(ThingDef def in defs)
            {
                GasineProp idkrandomname = (GasineProp)def.comps.Find(O => O is GasineProp);
                if(idkrandomname.MagazineWell != well)
                {
                    defs2.Remove(def);
                }
            }
            for(int i = (int)props.primaryMagazineCount.max; i > 0; i--)
            {
                ThingWithComps magno1 = ThingMaker.MakeThing(defs2.RandomElement()) as ThingWithComps;
                Log.Message(magno1.def.defName);
                magno1.stackCount = (int)Rand.Range(props.primaryMagazineCount.min, props.primaryMagazineCount.max);
                magno1.TryGetComp<Gazine>().loadedAmmoAmount = magno1.TryGetComp<Gazine>().Props.MagazineSize;
                Log.Message(magno1.TryGetComp<Gazine>().loadedAmmoAmount.ToString());
                magno1.TryGetComp<Gazine>().loadedAmmo = maguser.Props.ammoSet.ammoTypes.RandomElement().ammo;
                Log.Message(magno1.TryGetComp<Gazine>().loadedAmmo.ToString());
                dad.inventory.innerContainer.TryAdd(magno1, 1);
                dad.inventory.innerContainer.ToList().Find(G => G.def == magno1.def).TryGetComp<Gazine>().loadedAmmoAmount = magno1.TryGetComp<Gazine>().Props.MagazineSize;
                dad.inventory.innerContainer.ToList().Find(G => G.def == magno1.def).TryGetComp<Gazine>().loadedAmmo = maguser.Props.ammoSet.ammoTypes.RandomElement().ammo;
            }
           
        }
       
       
    }
}
