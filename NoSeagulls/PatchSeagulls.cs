using System;
using System.Diagnostics;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using ICities;

namespace NoSeagulls
{
    public class PatchSeagulls
    {
        //private static RedirectCallsState state1, state2, state3, state4, state5, state6, state7, state8;
        //private static bool deployed = false;

        //public static void Deploy()
        //{
        //    if (!deployed)
        //    {
        //        try
        //        {
        //            MethodInfo patch = typeof(PatchSeagulls).GetMethod("Patch", BindingFlags.Instance | BindingFlags.NonPublic);
        //            state1 = RedirectionHelper.RedirectCalls(typeof(ParkAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), patch);
        //            state2 = RedirectionHelper.RedirectCalls(typeof(LandfillSiteAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), patch);
        //            state3 = RedirectionHelper.RedirectCalls(typeof(HarborAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), patch);
        //            state4 = RedirectionHelper.RedirectCalls(typeof(CargoHarborAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), patch);
        //            state5 = RedirectionHelper.RedirectCalls(typeof(ParkAI).GetMethod("TargetAnimals", BindingFlags.Instance | BindingFlags.NonPublic), patch);
        //            state6 = RedirectionHelper.RedirectCalls(typeof(ParkBuildingAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), patch);
        //            state7 = RedirectionHelper.RedirectCalls(typeof(ParkBuildingAI).GetMethod("TargetAnimals", BindingFlags.Instance | BindingFlags.NonPublic), patch);
        //            state8 = RedirectionHelper.RedirectCalls(typeof(IndustryBuildingAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), patch);
        //            deployed = true;
        //        }
        //        catch (Exception e)
        //        {
        //            Util.DebugPrint("Deploy:");
        //            UnityEngine.Debug.LogException(e);
        //        }
        //    }
        //}

        //public static void Revert()
        //{
        //    if (deployed)
        //    {
        //        try
        //        {
        //            RedirectionHelper.RevertRedirect(typeof(ParkAI).GetMethod("TargetAnimals", BindingFlags.Instance | BindingFlags.NonPublic), state5);
        //            RedirectionHelper.RevertRedirect(typeof(ParkAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), state1);
        //            RedirectionHelper.RevertRedirect(typeof(LandfillSiteAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), state2);
        //            RedirectionHelper.RevertRedirect(typeof(HarborAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), state3);
        //            RedirectionHelper.RevertRedirect(typeof(CargoHarborAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), state4);
        //            RedirectionHelper.RevertRedirect(typeof(ParkBuildingAI).GetMethod("TargetAnimals", BindingFlags.Instance | BindingFlags.NonPublic), state7);
        //            RedirectionHelper.RevertRedirect(typeof(ParkBuildingAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), state6);
        //            RedirectionHelper.RevertRedirect(typeof(IndustryBuildingAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), state8);
        //            deployed = false;
        //        }
        //        catch (Exception e)
        //        {
        //            Util.DebugPrint("Revert:");
        //            UnityEngine.Debug.LogException(e);
        //        }
        //    }
        //}

        //private int Patch(ushort buildingID, ref Building data)
        //{
        //    return int.MaxValue;
        //}

        static readonly Stopwatch stopWatch = new Stopwatch();
        static string Millis => stopWatch.ElapsedMilliseconds.ToString();

        // This method is called on the Main thread but we want to run on the Simulation thread.
        internal static void Deploy(IThreading threading)
        {
            stopWatch.Start();
            RedirectionHelper.RedirectCalls(typeof(CitizenManager).GetMethod("GetGroupAnimalInfo", BindingFlags.Instance | BindingFlags.Public),
                typeof(PatchSeagulls).GetMethod("GetGroupAnimalInfo", BindingFlags.Static | BindingFlags.NonPublic));

            try
            {
                threading.QueueSimulationThread(DeployImpl);
            }
            catch (Exception e)
            {
                Util.DebugPrint("Deploy:");
                UnityEngine.Debug.LogException(e);
            }
        }

        static void DeployImpl()
        {
            try
            {
                Patch();
            }
            catch (Exception e)
            {
                Util.DebugPrint("Patch:");
                UnityEngine.Debug.LogException(e);
            }

            try
            {
                Release();
            }
            catch (Exception e)
            {
                Util.DebugPrint("Release:");
                UnityEngine.Debug.LogException(e);
            }
        }

        static void Patch()
        {
            FastList<ushort>[] groupAnimals = (FastList<ushort>[]) Util.Get(Singleton<CitizenManager>.instance, "m_groupAnimals");
            int k = -1;

            foreach(FastList<ushort> list in groupAnimals)
                if (k++ < 10000 && list != null)
                {
                    int i = 0;

                    while(i < list.m_size)
                    {
                        CitizenInfo prefab = PrefabCollection<CitizenInfo>.GetPrefab(list[i]);

                        if (IsSeagull(prefab))
                        {
                            list.RemoveAt(i);
                            list.Trim();
                            Util.DebugPrint("Patched", prefab.name, i.ToString(), k.ToString());
                        }
                        else
                            i++;
                    }
                }

            int n = PrefabCollection<BuildingInfo>.PrefabCount();

            // Performance optimization.
            for (int i = 0; i < n; i++)
            {
                BuildingInfo prefab = PrefabCollection<BuildingInfo>.GetPrefab((uint) i);

                switch(prefab?.GetAI())
                {
                    case HarborAI ai:
                        ai.m_animalCount = 0;
                        Util.DebugPrint("Cleared", prefab.name, i.ToString());
                        break;

                    case CargoHarborAI ai:
                        Util.Set(ai, "m_animalCount", 0);
                        Util.DebugPrint("Cleared", prefab.name, i.ToString());
                        break;

                    case LandfillSiteAI ai:
                        ai.m_animalCount = 0;
                        Util.DebugPrint("Cleared", prefab.name, i.ToString());
                        break;
                }
            }
        }

        static void Release()
        {
            int count = 0;
            CitizenManager cm = Singleton<CitizenManager>.instance;
            CitizenInstance[] buffer = cm.m_instances.m_buffer;
            int n = buffer.Length;

            for (int i = 1; i < n; i++)
                if ((buffer[i].m_flags & CitizenInstance.Flags.Created) != CitizenInstance.Flags.None && IsSeagull(buffer[i].Info))
                {
                    cm.ReleaseCitizenInstance((ushort) i);
                    count++;
                    Util.DebugPrint("Released", buffer[i].Info.name, i.ToString());
                }

            if (count > 0)
                Util.DebugPrint("Released", count.ToString(), "seagulls");
        }

        static bool IsSeagull(CitizenInfo prefab) => prefab?.m_citizenAI is BirdAI && prefab.name.ToUpperInvariant().Contains("SEAGULL");

        static int cc = 0;

        static CitizenInfo GetGroupAnimalInfo(CitizenManager cm, ref Randomizer r, ItemClass.Service service, ItemClass.SubService subService)
        {
            Util.DebugPrint("GGAI", cc.ToString(), Millis, service.ToString(), subService.ToString());
            cc++;
            FastList<ushort>[] groupAnimals = (FastList<ushort>[]) Util.Get(cm, "m_groupAnimals");
            int groupIndex = GetGroupIndex(service, subService);
            FastList<ushort> fastList = groupAnimals[groupIndex];

            if (fastList == null || fastList.m_size == 0)
                return null;

            groupIndex = r.Int32((uint) fastList.m_size);
            return PrefabCollection<CitizenInfo>.GetPrefab(fastList.m_buffer[groupIndex]);
        }

        static int GetGroupIndex(ItemClass.Service service, ItemClass.SubService subService)
        {
            if (subService != 0)
                return (int) (subService + 24 - 1);
            return (int) (service - 1);
        }
    }
}
