﻿using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace RimThreaded
{

    public class ThingOwnerUtility_Patch
    {

		public static bool AppendThingHoldersFromThings(List<IThingHolder> outThingsHolders, IList<Thing> container)
		{
			if (container == null)
			{
				return false;
			}
			int i = 0;
			int count = container.Count;
			while (i < count)
			{
				IThingHolder thingHolder = container[i] as IThingHolder;
				if (thingHolder != null)
				{
					lock (outThingsHolders)
					{
						outThingsHolders.Add(thingHolder);
					}
				}
				ThingWithComps thingWithComps = container[i] as ThingWithComps;
				if (thingWithComps != null)
				{
					List<ThingComp> allComps = thingWithComps.AllComps;
					for (int j = 0; j < allComps.Count; j++)
					{
						IThingHolder thingHolder2 = allComps[j] as IThingHolder;
						if (thingHolder2 != null)
						{
							lock (outThingsHolders)
							{
								outThingsHolders.Add(thingHolder2);
							}
						}
					}
				}
				i++;
			}
			return false;
		}

		public static bool GetAllThingsRecursively(IThingHolder holder, List<Thing> outThings, bool allowUnreal = true, Predicate<IThingHolder> passCheck = null)
		{
			outThings.Clear();
			if (passCheck != null && !passCheck(holder))
			{
				return false;
			}
			Stack<IThingHolder> tmpStack = new Stack<IThingHolder>();
			tmpStack.Push(holder);
			while (tmpStack.Count != 0)
			{
				IThingHolder thingHolder = tmpStack.Pop();
				if (allowUnreal || ThingOwnerUtility.AreImmediateContentsReal(thingHolder))
				{
					ThingOwner directlyHeldThings = thingHolder.GetDirectlyHeldThings();
					if (directlyHeldThings != null)
					{
						outThings.AddRange(directlyHeldThings);
					}
				}
				List<IThingHolder> tmpHolders = new List<IThingHolder>();
				thingHolder.GetChildHolders(tmpHolders);
				for (int i = 0; i < tmpHolders.Count; i++)
				{
					if (passCheck == null || passCheck(tmpHolders[i]))
					{
						tmpStack.Push(tmpHolders[i]);
					}
				}
			}
			//tmpStack.Clear();
			//tmpHolders.Clear();
			return false;
		}

	}
}
