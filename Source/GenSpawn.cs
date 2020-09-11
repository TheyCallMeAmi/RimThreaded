using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace RimThreaded
{

    public class GenSpawn_Patch
	{
        public static bool WipeExistingThings(
          IntVec3 thingPos,
          Rot4 thingRot,
          BuildableDef thingDef,
          Map map,
          DestroyMode mode)
        {
            foreach (IntVec3 c in GenAdj.CellsOccupiedBy(thingPos, thingRot, thingDef.Size))
            {
                Thing[] array;
                List<Thing> list = ThingGrid_Patch.ThingsListAt2(map.thingGrid, c);
                lock(list)
                {
                    array = list.ToArray();
                }
                foreach (Thing thing in array)
                {
                    if (null != thing)
                    {
                        ThingDef d = thing.def;
                        if (null != d)
                        {
                            if (GenSpawn.SpawningWipes(thingDef, thing.def))
                                thing.Destroy(mode);
                        }
                    }
                }
                    
                
            }
            return false;
        }
        public static bool Spawn(ref Thing __result,
      Thing newThing,
      IntVec3 loc,
      Map map,
      Rot4 rot,
      WipeMode wipeMode = WipeMode.Vanish,
      bool respawningAfterLoad = false)
        {
            if (map == null)
            {
                Log.Error("Tried to spawn " + newThing.ToStringSafe<Thing>() + " in a null map.", false);
                __result = (Thing)null;
                return false;
            }
            if (!loc.InBounds(map))
            {
                Log.Error("Tried to spawn " + newThing.ToStringSafe<Thing>() + " out of bounds at " + (object)loc + ".", false);
                __result = (Thing)null;
                return false;
            }
            /*
            IL_0048: ldstr        " out of bounds at "
            IL_004d: stelem.ref
            IL_004e: dup
            IL_004f: ldc.i4.3
            IL_0050: ldarg.1      // loc
            IL_0051: box          Verse.IntVec3
            IL_0056: stelem.ref
            IL_0057: dup
            IL_0058: ldc.i4.4
            IL_0059: ldstr        "."
            IL_005e: stelem.ref
            IL_005f: call         string [mscorlib]System.String::Concat(object[])
            IL_0064: ldc.i4.0
            IL_0065: call         void Verse.Log::Error(string, bool)
            IL_006a: ldnull
            IL_006b: ret

            if (null != newThing) {
                IL_008e: ldarg.1      // newThing
                IL_008f: ldnull
                IL_0090: cgt.un
                IL_0092: stloc.s      V_4
                IL_0094: ldloc.s      V_4
                IL_0096: brfalse      IL_0459

            if (null != newThing.def) {
                IL_009c: ldarg.1      // newThing
                IL_009d: ldfld        class ['Assembly-CSharp']Verse.ThingDef ['Assembly-CSharp']Verse.Thing::def
                IL_00a2: ldnull
                IL_00a3: cgt.un
                IL_00a5: stloc.s      V_5
                IL_00a7: ldloc.s      V_5
                IL_00a9: brfalse      IL_0458


	         ldloc.s 4
	         brfalse IL_0459
             nop
	         ldarg.1
	         ldfld class ['Assembly-CSharp']Verse.ThingDef ['Assembly-CSharp']Verse.Thing::def
	         ldnull
	         cgt.un
	         stloc.s 5
	         ldloc.s 5
	         brfalse IL_0458
            */
            //added null check
            if (null != newThing)
            {
                //added null check
                if (null != newThing.def)
                {
                    if (newThing.def.randomizeRotationOnSpawn)
                        rot = Rot4.Random;
                    CellRect occupiedRect = GenAdj.OccupiedRect(loc, rot, newThing.def.Size);
                    if (!occupiedRect.InBounds(map))
                    {
                        Log.Error("Tried to spawn " + newThing.ToStringSafe<Thing>() + " out of bounds at " + (object)loc + " (out of bounds because size is " + (object)newThing.def.Size + ").", false);
                        __result = (Thing)null;
                        return false;
                    }
                    if (newThing.Spawned)
                    {
                        Log.Error("Tried to spawn " + (object)newThing + " but it's already spawned.", false);
                        __result = newThing;
                        return false;
                    }
                    switch (wipeMode)
                    {
                        case WipeMode.Vanish:
                            GenSpawn.WipeExistingThings(loc, rot, (BuildableDef)newThing.def, map, DestroyMode.Vanish);
                            break;
                        case WipeMode.FullRefund:
                            GenSpawn.WipeAndRefundExistingThings(loc, rot, (BuildableDef)newThing.def, map);
                            break;
                        case WipeMode.VanishOrMoveAside:
                            GenSpawn.CheckMoveItemsAside(loc, rot, newThing.def, map);
                            GenSpawn.WipeExistingThings(loc, rot, (BuildableDef)newThing.def, map, DestroyMode.Vanish);
                            break;
                    }
                    if (newThing.def.category == ThingCategory.Item)
                    {
                        foreach (IntVec3 intVec3 in occupiedRect)
                        {
                            foreach (Thing thing in intVec3.GetThingList(map).ToList<Thing>())
                            {
                                if (thing != newThing && thing.def.category == ThingCategory.Item)
                                {
                                    thing.DeSpawn(DestroyMode.Vanish);
                                    if (!GenPlace.TryPlaceThing(thing, intVec3, map, ThingPlaceMode.Near, (Action<Thing, int>)null, (Predicate<IntVec3>)(x => !occupiedRect.Contains(x)), new Rot4()))
                                        thing.Destroy(DestroyMode.Vanish);
                                }
                            }
                        }
                    }
                    newThing.Rotation = rot;
                    newThing.Position = loc;
                    if (newThing.holdingOwner != null)
                        newThing.holdingOwner.Remove(newThing);
                    newThing.SpawnSetup(map, respawningAfterLoad);
                    if (newThing.Spawned && newThing.stackCount == 0)
                    {
                        Log.Error("Spawned thing with 0 stackCount: " + (object)newThing, false);
                        newThing.Destroy(DestroyMode.Vanish);
                        __result = (Thing)null;
                        return false;
                    }
                    if (newThing.def.passability == Traversability.Impassable)
                    {
                        foreach (IntVec3 c in occupiedRect)
                        {
                            foreach (Thing thing in c.GetThingList(map).ToList<Thing>())
                            {
                                if (thing != newThing && thing is Pawn pawn)
                                    pawn.pather.TryRecoverFromUnwalkablePosition(false);
                            }
                        }
                        __result = newThing;
                        return false;
                    }
                }
            }
            __result = newThing;
            return false;
        }


        public static bool CheckMoveItemsAside(
          IntVec3 thingPos,
          Rot4 thingRot,
          ThingDef thingDef,
          Map map)
        {
            if (thingDef.surfaceType != SurfaceType.None || thingDef.passability == Traversability.Standable)
            {                
                return false;
            }
            CellRect occupiedRect = GenAdj.OccupiedRect(thingPos, thingRot, thingDef.Size);
            foreach (IntVec3 intVec3 in occupiedRect)
            {
                if (intVec3.InBounds(map))
                {
                    CellIndices cellIndices = map.cellIndices;
                    List<Thing> list = ThingGrid_Patch.thingGridDict[map.thingGrid][cellIndices.CellToIndex(intVec3)];
                    lock (list)
                    {
                        foreach (Thing thing in list.ToArray())
                        {
                            //if (!thing.Destroyed)
                            //{
                                if (thing.def.category == ThingCategory.Item)
                                {
                                    thing.DeSpawn(DestroyMode.Vanish);
                                    if (!GenPlace.TryPlaceThing(thing, intVec3, map, ThingPlaceMode.Near, null, (x => !occupiedRect.Contains(x)), new Rot4()))
                                        thing.Destroy(DestroyMode.Vanish);
                                }
                            //}
                        }
                    }
                }
            }
            return false;
        }

    }
}
