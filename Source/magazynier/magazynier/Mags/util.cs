using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse.AI;
using Verse.Sound;
using RimWorld;
using CombatExtended;
using Verse;

namespace magazynier.Mags
{
    
    public static class SupportUtil
    {
        public static void amungus(Pawn pap, ref float swaychange)
        {
            Pawn pawn = pap;
            List<IntVec3> allergy = pap.CellsAdjacent8WayAndInside().ToList();
            switch (pap.Rotation.ToString())
            {
                case "2":
                    allergy.RemoveAll(D => D.z > pawn.Position.z);
                    allergy.RemoveAll(G => G.x != pawn.Position.x);
                  

                    break;
                case "0":
                    allergy.RemoveAll(D => D.z < pawn.Position.z);
                    allergy.RemoveAll(G => G.x != pawn.Position.x);
                   
                    break;
                case "3":
                    allergy.RemoveAll(D => D.x > pawn.Position.x);
                    allergy.RemoveAll(G => G.z != pawn.Position.z);
                   
                    break;
                case "1":
                    allergy.RemoveAll(D => D.x <= pawn.Position.x);

                    allergy.RemoveAll(G => G.z != pawn.Position.z);
                  
                    break;
            }
            allergy.Remove(pawn.Position);
            List<Thing> things = new List<Thing>();
          
            foreach (IntVec3 cough in allergy)
            {
                if(cough.GetThingList(pap.Map).Count > 0)
                {
                    foreach(Thing khu in cough.GetThingList(pap.Map))
                    {
                        //Log.Message(khu.def.defName);
                    }
                    things.AddRange(cough.GetThingList(pap.Map));
                }
               
            }
            bool reee(Thing thing)
            {
                if(thing is Building | thing is Pawn)
                {
                    return false;
                }
                else
                {
                    return true;
                }
               
            }
            bool check2(Thing thing)
            {
                if(thing is Pawn)
                {
                    Pawn thingpawn = (Pawn)thing;
                    Pawn testpawn = ThingMaker.MakeThing(ThingDefOf.Human) as Pawn;
                    if (thingpawn.RaceProps.baseBodySize > testpawn.RaceProps.baseBodySize)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if(thing is Building)
                    {
                        Building buidlingthing = (Building)thing;
                        if(buidlingthing.def.fillPercent >= 0.7f && buidlingthing.def.fillPercent > 0.05f)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }

            things.RemoveAll(F => reee(F));
            things.RemoveAll(F => check2(F));
            things.RemoveAll(F => F.def.fillPercent == 0);
            foreach(Thing fing in things)
            {
                if(things.Count > 0)
                {
                    Log.Message(fing.def.defName + " " + fing.def.fillPercent.ToString());

                }
                
            }
            if(things.Count > 0)
            {
                if(things.Last() is Pawn)
                {
                    Pawn pawn1 = things.Last() as Pawn;
                    swaychange = 1 + pawn1.RaceProps.baseBodySize;
                }
                else
                {
                    swaychange = 1 + things.Last().def.fillPercent;
                }
              
                //Log.Error(swaychange.ToString() + " test");
            }
            else
            {
                swaychange = 1f;
                //Log.Error(swaychange.ToString() + " test");
            }
           


        }
    }
}
