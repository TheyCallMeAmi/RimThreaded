﻿using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Threading;
using Verse;

namespace RimThreaded
{
    public class ThingGrid_Patch
    {
        public static readonly List<Thing> EmptyThingList = new List<Thing>();
        public static AccessTools.FieldRef<ThingGrid, Map> map = AccessTools.FieldRefAccess<ThingGrid, Map>("map");
        public static Dictionary<ThingGrid, List<Thing>[]> thingGridDict = new Dictionary<ThingGrid, List<Thing>[]>();

        public static void Postfix_Constructor(ThingGrid __instance, Map map)
        {
            ThingGrid_Patch.map(__instance) = map;
            CellIndices cellIndices = map.cellIndices;
            List<Thing>[] thingGrid = new List<Thing>[cellIndices.NumGridCells];
            thingGridDict.Add(__instance, thingGrid);
            for (int i = 0; i < cellIndices.NumGridCells; ++i)
                thingGrid[i] = new List<Thing>(4);
        }

        public static bool RegisterInCell(ThingGrid __instance, Thing t, IntVec3 c)
        {
            Map this_map = map(__instance);
            CellIndices cellIndices = this_map.cellIndices;
            if (!c.InBounds(this_map))
            {
                Log.Warning(t.ToString() + " tried to register out of bounds at " + c + ". Destroying.", false);
                t.Destroy(DestroyMode.Vanish);
            }
            else
            {
                int index = cellIndices.CellToIndex(c);
                lock (thingGridDict[__instance][index]) { 
                    thingGridDict[__instance][index].Add(t);
                }
            }
            return false;
        }

        public static bool DeregisterInCell(ThingGrid __instance, Thing t, IntVec3 c)
        {
            Map this_map = map(__instance);
            CellIndices cellIndices = this_map.cellIndices;
            if (!c.InBounds(this_map))
            {
                Log.Error(t.ToString() + " tried to de-register out of bounds at " + (object)c, false);
            }
            else
            {
                int index = cellIndices.CellToIndex(c);
                List<Thing>[] thingList = thingGridDict[__instance];
                lock (thingList[index])
                {
                    if (!thingList[index].Contains(t))
                        return false;
                    thingList[index].Remove(t);
                }
            }
            return false;
        }

        public static bool ThingsAt(ThingGrid __instance, ref IEnumerable<Thing> __result, IntVec3 c)
        {
            Map this_map = map(__instance);
            CellIndices cellIndices = this_map.cellIndices;
            __result = null;
            if (c.InBounds(this_map))
            {
                __result = thingGridDict[__instance][cellIndices.CellToIndex(c)];
                return false;
            }
            return false;
        }
        public static List<Thing> ThingsListAt2(ThingGrid __instance, IntVec3 c)
        {
            Map this_map = map(__instance);
            CellIndices cellIndices = this_map.cellIndices;
            if (c.InBounds(this_map))
            {
                return thingGridDict[__instance][cellIndices.CellToIndex(c)];
            }
            Log.ErrorOnce("Got ThingsListAt out of bounds: " + c, 495287, false);
            return EmptyThingList;
        }
        public static bool ThingsListAt(ThingGrid __instance, ref List<Thing> __result, IntVec3 c)
        {
            Map this_map = map(__instance);
            CellIndices cellIndices = this_map.cellIndices;
            if (c.InBounds(this_map))
            {
                __result = thingGridDict[__instance][cellIndices.CellToIndex(c)]; 
                return false;
            }
            Log.ErrorOnce("Got ThingsListAt out of bounds: " + c, 495287, false);
            __result = EmptyThingList;
            return false;
        }
        public static bool ThingsListAtFast(ThingGrid __instance, ref List<Thing> __result, IntVec3 c)
        {
            Map this_map = map(__instance);
            CellIndices cellIndices = this_map.cellIndices;
            __result = thingGridDict[__instance][cellIndices.CellToIndex(c)];
            return false;
        }
        public static bool ThingsListAtFast(ThingGrid __instance, ref List<Thing> __result, int index)
        {
            __result = thingGridDict[__instance][index];
            return false;
        }
        public static bool ThingAt(ThingGrid __instance, ref Thing __result, IntVec3 c, ThingCategory cat)
        {
            Map this_map = map(__instance);
            CellIndices cellIndices = this_map.cellIndices;
            if (!c.InBounds(this_map))
            {
                __result = null;
                return false;
            }

            lock (thingGridDict[__instance][cellIndices.CellToIndex(c)])
            {
                List<Thing> thingList = thingGridDict[__instance][cellIndices.CellToIndex(c)];
                foreach (Thing t in thingList)
                {
                    if (t.def.category == cat)
                    {
                        __result = t;
                        return false;
                    }
                }
            }
            __result = null;
            return false;
        }
        public static bool ThingAt(ThingGrid __instance, ref Thing __result, IntVec3 c, ThingDef def)
        {
            Map this_map = map(__instance);
            CellIndices cellIndices = this_map.cellIndices;
            if (!c.InBounds(this_map))
            {
                __result = null;
                return false;
            }
            lock (thingGridDict[__instance][cellIndices.CellToIndex(c)])
            {
                List<Thing> thingList = thingGridDict[__instance][cellIndices.CellToIndex(c)];
                foreach (Thing t in thingList)
                {
                    if (t.def == def)
                    {
                        __result = t;
                        return false;
                    }
                }
            }
            __result = null;
            return false;
        }

        public static bool ThingAt_Apparel(ThingGrid __instance, ref Apparel __result, IntVec3 c)
        {
            Map this_map = map(__instance);
            CellIndices cellIndices = this_map.cellIndices;
            __result = null;
            if (!c.InBounds(this_map))
            {
                __result = default(Apparel);
                return false;
            }
            lock (thingGridDict[__instance][cellIndices.CellToIndex(c)])
            {
                List<Thing> thingList = thingGridDict[__instance][cellIndices.CellToIndex(c)];
                foreach (Thing t in thingList)
                {
                    if (t is Apparel obj)
                    {
                        __result = obj;
                        return false;
                    }
                }
            }
            __result = default(Apparel);
            return false;
        }
        public static bool ThingAt_Building_Door(ThingGrid __instance, ref Building_Door __result, IntVec3 c)
        {
            Map this_map = map(__instance);
            CellIndices cellIndices = this_map.cellIndices;
            __result = null;
            if (!c.InBounds(this_map))
            {
                __result = default(Building_Door);
                return false;
            }
            lock (thingGridDict[__instance][cellIndices.CellToIndex(c)])
            {
                List<Thing> thingList = thingGridDict[__instance][cellIndices.CellToIndex(c)];
                foreach (Thing t in thingList)
                {
                    if (t is Building_Door obj)
                    {
                        __result = obj;
                        return false;
                    }
                }
            }
            __result = default(Building_Door);
            return false;
        }
    }

}
