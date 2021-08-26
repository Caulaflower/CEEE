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

namespace magazynier.BackBlast
{
    public static class assblastutil
    {
        public static List<IntVec3> CellsAtBack(Pawn pawn)
        {
           
            List<IntVec3> cells = pawn.CellsAdjacent8WayAndInside().ToList();
          
            cells.RemoveAll(A => A == pawn.Position);
            switch (pawn.Rotation.ToString())
            {
                case "0":
                    cells.RemoveAll(D => D.z > pawn.Position.z);
                    cells.RemoveAll(G => G.x != pawn.Position.x && G.z == pawn.Position.z);
                    cells.Add(new IntVec3 { x = pawn.Position.x, y = 0, z = pawn.Position.z - 5 });

                    break;
                case "2":
                    cells.RemoveAll(D => D.z < pawn.Position.z);
                    cells.RemoveAll(G => G.x != pawn.Position.x && G.z == pawn.Position.z);
                    cells.Add(new IntVec3 { x = pawn.Position.x, y = 0, z = pawn.Position.z + 5 });
                    break;
                case "1":
                    cells.RemoveAll(D => D.x > pawn.Position.x);
                    cells.RemoveAll(G => G.z != pawn.Position.z && G.z == pawn.Position.x);
                    cells.Add(new IntVec3 { z = pawn.Position.z, y = 0, x = pawn.Position.x - 5 });
                    break;
                case "3":
                    cells.RemoveAll(D => D.x < pawn.Position.x);
                    
                    cells.RemoveAll(G => G.z != pawn.Position.z && G.z == pawn.Position.x);
                    cells.Add(new IntVec3 { z = pawn.Position.z, y = 0, x = pawn.Position.x + 5 });
                    break;
            }
            cells.Remove(pawn.Position);
            
           
            foreach (IntVec3 vec3 in cells)
            {
                Thing idk = new Thing();
                if (vec3.AdjacentTo8Way(pawn.Position))
                {
                    GenExplosionCE.DoExplosion(vec3, pawn.Map, 0.7f, DamageDefOf.Flame, null, 20, 2f, SoundDefOf.Thunder_OnMap, null, null, null, null, 0, 0);
                }
                else
                {
                    GenExplosionCE.DoExplosion(vec3, pawn.Map, 4f, DamageDefOf.Flame, null, 12, 2f, SoundDefOf.Thunder_OnMap, null, null, null, null, 0, 0);
                }
               
                GenThing.TryDropAndSetForbidden(ThingMaker.MakeThing(ThingDefOf.Cloth), vec3, pawn.Map, ThingPlaceMode.Direct, out idk, false);
            }
            return cells;
        }
    }
    public class TestingComp : ThingComp
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                action = delegate
                {
                    Log.Message(this.parent.Rotation.ToString());
                    Log.Error(assblastutil.CellsAtBack((Pawn)this.parent).ToString());
                },
                icon = ContentFinder<Texture2D>.Get("Mag/fud1"),
                defaultDesc = "ghbj n",
                defaultLabel = "jbkhn",
                defaultIconColor = new Color { r = 200, a = 1, b = 1, g = 2}
            };
        }
    }
}
