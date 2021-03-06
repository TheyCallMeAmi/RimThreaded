﻿using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;
using Verse;
using Verse.AI;

namespace RimThreaded
{
    [StaticConstructorOnStartup]
    public class Verse_PawnDestinationReservationManager_Patch
    {
        private static readonly Material DestinationMat = MaterialPool.MatFrom("UI/Overlays/ReservedDestination");
        private static readonly Material DestinationSelectionMat = MaterialPool.MatFrom("UI/Overlays/ReservedDestinationSelection");
        public static ConcurrentDictionary<Faction, PawnDestinationReservationManager.PawnDestinationSet> reservedDestinations = new ConcurrentDictionary<Faction, PawnDestinationReservationManager.PawnDestinationSet>();



        public static bool Notify_FactionRemoved(PawnDestinationReservationManager __instance, Faction faction)
        {

                reservedDestinations.TryRemove(faction, out _);
            
            return false;
        }

        public static bool GetPawnDestinationSetFor(PawnDestinationReservationManager __instance, ref PawnDestinationReservationManager.PawnDestinationSet __result, Faction faction)
        {
            PawnDestinationReservationManager.PawnDestinationSet value = reservedDestinations.GetOrAdd(faction, new PawnDestinationReservationManager.PawnDestinationSet());
            __result = value;
            return false;
        }
        public static PawnDestinationReservationManager.PawnDestinationSet GetPawnDestinationSetFor2(Faction faction)
        {
            PawnDestinationReservationManager.PawnDestinationSet value = reservedDestinations.GetOrAdd(faction, new PawnDestinationReservationManager.PawnDestinationSet());
            return value;
        }
        public static bool Reserve(PawnDestinationReservationManager __instance, Pawn p, Job job, IntVec3 loc)
        {
            if (p.Faction == null)
                return false;
            Pawn claimant;
            if (p.Drafted && p.Faction == Faction.OfPlayer && (__instance.IsReserved(loc, out claimant) && claimant != p) && (!claimant.HostileTo((Thing)p) && claimant.Faction != p.Faction) && (claimant.mindState == null || claimant.mindState.mentalStateHandler == null || !claimant.mindState.mentalStateHandler.InMentalState || claimant.mindState.mentalStateHandler.CurStateDef.category != MentalStateCategory.Aggro && claimant.mindState.mentalStateHandler.CurStateDef.category != MentalStateCategory.Malicious))
                claimant.jobs.EndCurrentJob(JobCondition.InterruptForced, true, true);
            ObsoleteAllClaimedBy2(p);
            PawnDestinationReservationManager.PawnDestinationSet destinationSet = GetPawnDestinationSetFor2(p.Faction);
            List<PawnDestinationReservationManager.PawnDestinationReservation> list = destinationSet.list;
            lock (list)
            {
                list.Add(new PawnDestinationReservationManager.PawnDestinationReservation()
                {
                    target = loc,
                    claimant = p,
                    job = job
                });
            }
            
            return false;
        }
        public static void ObsoleteAllClaimedBy2(Pawn p)
        {
            if (p.Faction == null)
                return;
            PawnDestinationReservationManager.PawnDestinationSet destinationSet = GetPawnDestinationSetFor2(p.Faction);
            List<PawnDestinationReservationManager.PawnDestinationReservation> list = destinationSet.list;
            lock (list)
            {
                for (int index = 0; index < list.Count; ++index)
                {
                    if (list[index].claimant == p)
                    {
                        list[index].obsolete = true;
                        if (list[index].job == null)
                        {
                            list[index] = list[list.Count - 1];
                            list.RemoveLast();
                            --index;
                        }
                    }
                }
            }
            return;
        }
        public static bool ObsoleteAllClaimedBy(PawnDestinationReservationManager __instance, Pawn p)
        {
            if (p.Faction == null)
                return false;
            PawnDestinationReservationManager.PawnDestinationSet destinationSet = GetPawnDestinationSetFor2(p.Faction);
            List<PawnDestinationReservationManager.PawnDestinationReservation> list = destinationSet.list;
            lock (list)
            {
                for (int index = 0; index < list.Count; ++index)
                {
                    if (list[index].claimant == p)
                    {
                        list[index].obsolete = true;
                        if (list[index].job == null)
                        {
                            list[index] = list[list.Count - 1];
                            list.RemoveLast();
                            --index;
                        }
                    }
                }
            }
            return false;
        }

        public static bool ReleaseAllObsoleteClaimedBy(PawnDestinationReservationManager __instance, Pawn p)
        {
            if (p.Faction == null)
                return false;
            PawnDestinationReservationManager.PawnDestinationSet destinationSet = GetPawnDestinationSetFor2(p.Faction);
            List<PawnDestinationReservationManager.PawnDestinationReservation> list = destinationSet.list;
            int index = 0;
            lock (list)
            {
                while (index < list.Count)
                {
                    if (list[index].claimant == p && list[index].obsolete)
                    {
                        list[index] = list[list.Count - 1];
                        list.RemoveLast();
                    }
                    else
                        ++index;
                }
            }
            return false;
        }

        public static bool ReleaseAllClaimedBy(PawnDestinationReservationManager __instance, Pawn p)
        {
            if (p.Faction == null)
                return false;
            PawnDestinationReservationManager.PawnDestinationSet destinationSet = GetPawnDestinationSetFor2(p.Faction);
            List<PawnDestinationReservationManager.PawnDestinationReservation> list = destinationSet.list;
            int index = 0;
            lock (list)
            {
                while (index < list.Count)
                {
                    if (list[index].claimant == p)
                    {
                        list[index] = list[list.Count - 1];
                        list.RemoveLast();
                    }
                    else
                        ++index;
                }
            }
            return false;            
        }

        public static bool ReleaseClaimedBy(PawnDestinationReservationManager __instance, Pawn p, Job job)
        {
            if (p.Faction == null)
                return false;
            PawnDestinationReservationManager.PawnDestinationSet destinationSet = GetPawnDestinationSetFor2(p.Faction);
            List<PawnDestinationReservationManager.PawnDestinationReservation> list = destinationSet.list;
            lock (list)
            {
                for (int index = 0; index < list.Count; ++index)
                {
                    if (list[index].claimant == p && list[index].job == job)
                    {
                        list[index].job = null;
                        if (list[index].obsolete)
                        {
                            list[index] = list[list.Count - 1];
                            list.RemoveLast();
                            --index;
                        }
                    }
                }
            }
            return false;
        }

        public static bool DebugDrawReservations(PawnDestinationReservationManager __instance)
        {

            foreach (KeyValuePair<Faction, PawnDestinationReservationManager.PawnDestinationSet> reservedDestination in reservedDestinations)
            {
                List<PawnDestinationReservationManager.PawnDestinationReservation> list = reservedDestination.Value.list;
                lock (list)
                {
                    foreach (PawnDestinationReservationManager.PawnDestinationReservation destinationReservation in list)
                    {
                        IntVec3 target = destinationReservation.target;
                        MaterialPropertyBlock properties = new MaterialPropertyBlock();
                        properties.SetColor("_Color", reservedDestination.Key.Color);
                        Vector3 s = new Vector3(1f, 1f, 1f);
                        Matrix4x4 matrix = new Matrix4x4();
                        matrix.SetTRS(target.ToVector3ShiftedWithAltitude(AltitudeLayer.MetaOverlays), Quaternion.identity, s);
                        Graphics.DrawMesh(MeshPool.plane10, matrix, DestinationMat, 0, Camera.main, 0, properties);
                        if (Find.Selector.IsSelected((object)destinationReservation.claimant))
                            Graphics.DrawMesh(MeshPool.plane10, matrix, DestinationSelectionMat, 0);
                    }
                }
                    
            }
            
            return false;
        }

        public static bool IsReserved(PawnDestinationReservationManager __instance, ref bool __result, IntVec3 loc, out Pawn claimant)
        {

            foreach (KeyValuePair<Faction, PawnDestinationReservationManager.PawnDestinationSet> reservedDestination in reservedDestinations)
            {
                List<PawnDestinationReservationManager.PawnDestinationReservation> list = reservedDestination.Value.list;
                lock (list)
                {
                    for (int index = 0; index < list.Count; ++index)
                    {
                        if (list[index].target == loc)
                        {
                            claimant = list[index].claimant;
                            __result = true;
                            return false;
                        }
                    }
                }
            }
            
            claimant = null;
            __result = false;
            return false;
        }



    }

}


