using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using UnityEngine;
using CombatExtended;
using Verse.AI;
using Verse.Sound;
using HarmonyLib;
using HarmonyMod;
using System.Reflection;
using System.Reflection.Emit;


namespace magazynier
{
    [HarmonyPatch(typeof(VerbTracker), "VerbsTick")]
    static class Harmony_VerbTracker_Modify_VerbsTickMAGAZYNIER
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
        {
            int patchPhase = 0;
            var verb = il.DeclareLocal(typeof(VerbTracker));
            var verbCE = il.DeclareLocal(typeof(magazynier.Verb_LaunchProjectileCE_With_ChangeAble_Projectile));
            var failBranch = il.DefineLabel();

            foreach (CodeInstruction instruction in instructions)
            {
                // locate the instruction ldloc.0 which is right after the callvirt that does the VerbTick.
                if (patchPhase == 1 && instruction.opcode == OpCodes.Ldloc_0)
                {
                    // load the VerbTracker object stored earlier
                    yield return new CodeInstruction(OpCodes.Ldloc, verb);
                    // convert the VerbTracker object to a magazynier.Verb_LaunchProjectileCE_With_ChangeAble_Projectile object (from here just VerbCE)
                    yield return new CodeInstruction(OpCodes.Isinst, typeof(magazynier.Verb_LaunchProjectileCE_With_ChangeAble_Projectile));
                    // store the VerbCE object into another local variable.
                    yield return new CodeInstruction(OpCodes.Stloc, verbCE);
                    // Load the just stored VerbCE object onto the stack.
                    yield return new CodeInstruction(OpCodes.Ldloc, verbCE);

                    // branch to failure if the current object is null.
                    yield return new CodeInstruction(OpCodes.Brfalse, failBranch);
                    // restore the VerbCE object onto the stack again...
                    yield return new CodeInstruction(OpCodes.Ldloc, verbCE);
                    // callvirt the method VerbTickCE() (no args, void return).
                    yield return new CodeInstruction(OpCodes.Callvirt, typeof(magazynier.Verb_LaunchProjectileCE_With_ChangeAble_Projectile).GetMethod("VerbTickCE", AccessTools.all));

                    // modify the current instruction (should be ldloc.0) to have the label for fail condition.
                    instruction.labels.Add(failBranch);

                    // done
                    patchPhase = 2;
                }

                // locate the VerbTick instruction, want to insert before that.
                if (patchPhase == 0 && instruction.opcode == OpCodes.Callvirt && (instruction.operand as MethodInfo) != null && (instruction.operand as MethodInfo).Name.Equals("VerbTick"))
                {
                    // store the retrieved VerbTracker object in a local variable.
                    yield return new CodeInstruction(OpCodes.Stloc, verb);
                    // push the JUST stored local variable back onto the stack for use by the next instruction.
                    yield return new CodeInstruction(OpCodes.Ldloc, verb);
                    patchPhase = 1;
                }


                yield return instruction;
            }
        }
    }
}
