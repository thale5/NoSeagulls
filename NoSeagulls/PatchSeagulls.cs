using System;
using ColossalFramework;
using ICities;

namespace NoSeagulls
{
    public class PatchSeagulls
    {
        // This method is called on the Main thread but we need to run on the Simulation thread.
        internal static void Deploy(IThreading threading)
        {
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

            foreach(FastList<ushort> list in groupAnimals)
                if (list != null)
                {
                    int i = 0;

                    while(i < list.m_size)
                        if (IsSeagull(PrefabCollection<CitizenInfo>.GetPrefab(list[i])))
                        {
                            list.RemoveAt(i);
                            list.Trim();
                        }
                        else
                            i++;
                }

            int n = PrefabCollection<BuildingInfo>.PrefabCount();

            // Performance optimization.
            for (int i = 0; i < n; i++)
            {
                switch (PrefabCollection<BuildingInfo>.GetPrefab((uint) i)?.GetAI())
                {
                    case HarborAI ai:
                        ai.m_animalCount = 0;
                        break;

                    case CargoHarborAI ai:
                        Util.Set(ai, "m_animalCount", 0);
                        break;

                    case LandfillSiteAI ai:
                        ai.m_animalCount = 0;
                        break;
                }
            }
        }

        static void Release()
        {
            CitizenManager cm = Singleton<CitizenManager>.instance;
            CitizenInstance[] buffer = cm.m_instances.m_buffer;
            int n = buffer.Length, count = 0;

            for (int i = 1; i < n; i++)
                if ((buffer[i].m_flags & CitizenInstance.Flags.Created) != CitizenInstance.Flags.None && IsSeagull(buffer[i].Info))
                {
                    cm.ReleaseCitizenInstance((ushort) i);
                    count++;
                }

            if (count > 0)
                Util.DebugPrint("Released", count.ToString(), "seagulls");
        }

        static bool IsSeagull(CitizenInfo prefab) => prefab?.m_citizenAI is BirdAI && prefab.name.ToUpperInvariant().Contains("SEAGULL");
    }
}
