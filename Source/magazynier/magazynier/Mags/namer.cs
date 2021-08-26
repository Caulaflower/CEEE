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
using RimWorld.Planet;
namespace magazynier.Mags
{
    public class AmmoNameIdk : WorldComponent
    {
        public bool myExampleBool = true;

        public AmmoNameIdk(World world) : base(world)
        {
        }
        public override void FinalizeInit()
        {
            Log.Message("test");
            foreach(ThingDef thring in DefDatabase<ThingDef>.AllDefs.ToList().Where(oof => oof.comps.Any(oov => oov is CompProperties_MagazineUser)).ToList())
            {
                Log.Message(thring.label);
                CompProperties_MagazineUser willthiswork = (CompProperties_MagazineUser)thring.comps.Find(oov => oov is CompProperties_MagazineUser);
                thring.description += " Used magazine well: " + willthiswork.well.Label;
                StatDef statDef = new StatDef
                {
                    defName = "abumbusissus",
                    label = "Used magazine well",
                    category = StatCategoryDefOf.Basics,
                    description = "Magazine well used in the gun",
                    
                    
                };
                //thring.statBases.Add(new StatModifier {stat = statDef, value = willthiswork.well.Label });
            }
            base.FinalizeInit();
        }

    }
}
