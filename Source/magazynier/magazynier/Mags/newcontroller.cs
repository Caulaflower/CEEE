using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyMod;
using HarmonyLib;
using CombatExtended.HarmonyCE;
using CombatExtended.Compatibility;

namespace magazynier
{
    public class JustInject : Mod
    {


        public JustInject(ModContentPack content) : base(content)
        {


            var harmony = new Harmony("Caulaflower.mgagzines.magazines");
            try
            {
                Log.Message("9x17 is also known as ...");
                harmony.PatchAll();
               
            }
            catch (Exception e)
            {
                Log.Error("Failed to apply 1 or more harmony patches! See error:");
                Log.Error(e.ToString());
            }

            LongEventHandler.QueueLongEvent(magInjector.Inject, "", false, null);




        }

    }
}
