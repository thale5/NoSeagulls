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
                Util.DebugPrint("In Deploy");
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
                Util.DebugPrint("In DeployImpl");
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

        // egrep -ri 'GetGroupAnimalInfo.+Garbage'
        static void Patch()
        {
            FastList<ushort>[] groupAnimals = (FastList<ushort>[]) Util.Get(Singleton<CitizenManager>.instance, "m_groupAnimals");
            Util.DebugPrint("In Patch");
            Util.DebugPrint("groupAnimals length", groupAnimals.Length.ToString());

            int j = 0;
            foreach(FastList<ushort> list in groupAnimals)
            {
                if (list != null)
                {
                    ItemClass.Service service = ItemClass.Service.None;
                    ItemClass.SubService sub = ItemClass.SubService.None;

                    if (j + 1 < ItemClass.ServiceCount)
                        service = (ItemClass.Service) (j + 1);
                    else
                        sub = (ItemClass.SubService) (j + 1 - ItemClass.ServiceCount);

                    int i = 0;

                    while(i < list.m_size)
                    {
                        var prefab = PrefabCollection<CitizenInfo>.GetPrefab(list[i]);
                        Util.DebugPrint("Prefab", prefab.GetType().Name.ToString(), prefab.name,
                            j.ToString(), i.ToString(), service.ToString(), sub.ToString());

                        if (IsSeagull(PrefabCollection<CitizenInfo>.GetPrefab(list[i])))
                        {
                            list.RemoveAt(i);
                            list.Trim();
                        }
                        else
                            i++;
                    }
                }
                j++;
            }

            int n = PrefabCollection<BuildingInfo>.PrefabCount();

            // Performance optimization.
            for (int i = 0; i < n; i++)
            {
                PrefabAI ai = PrefabCollection<BuildingInfo>.GetPrefab((uint) i)?.GetAI();

                if (ai is ParkAI p)
                    p.m_seagulls = false;
                else if (ai is HarborAI h)
                    h.m_animalCount = 0;
                else if (ai is CargoHarborAI c)
                    Util.Set(c, "m_animalCount", 0);
                else if (ai is LandfillSiteAI l)
                    l.m_animalCount = 0;
            }
        }

        static void Release()
        {
            CitizenManager cm = Singleton<CitizenManager>.instance;
            CitizenInstance[] buffer = cm.m_instances.m_buffer;
            int n = buffer.Length, count = 0;
            Util.DebugPrint("In Release");

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
