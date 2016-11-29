using System;
using System.Reflection;
using ColossalFramework;
using ICities;

namespace NoSeagulls
{
    public class PatchSeagulls
    {
        private static RedirectCallsState state1, state2, state3, state4, state5;
        private static bool deployed = false;

        public static void Deploy()
        {
            if (!deployed)
            {
                try
                {
                    MethodInfo patch = typeof(PatchSeagulls).GetMethod("Patch", BindingFlags.Instance | BindingFlags.NonPublic);
                    state1 = RedirectionHelper.RedirectCalls(typeof(ParkAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), patch);
                    state2 = RedirectionHelper.RedirectCalls(typeof(LandfillSiteAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), patch);
                    state3 = RedirectionHelper.RedirectCalls(typeof(HarborAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), patch);
                    state4 = RedirectionHelper.RedirectCalls(typeof(CargoHarborAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), patch);
                    state5 = RedirectionHelper.RedirectCalls(typeof(ParkAI).GetMethod("TargetAnimals", BindingFlags.Instance | BindingFlags.NonPublic), patch);
                    deployed = true;
                }
                catch (Exception e)
                {
                    Util.DebugPrint("Deploy:");
                    UnityEngine.Debug.LogException(e);
                }
            }
        }

        public static void Revert()
        {
            if (deployed)
            {
                try
                {
                    RedirectionHelper.RevertRedirect(typeof(ParkAI).GetMethod("TargetAnimals", BindingFlags.Instance | BindingFlags.NonPublic), state5);
                    RedirectionHelper.RevertRedirect(typeof(ParkAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), state1);
                    RedirectionHelper.RevertRedirect(typeof(LandfillSiteAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), state2);
                    RedirectionHelper.RevertRedirect(typeof(HarborAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), state3);
                    RedirectionHelper.RevertRedirect(typeof(CargoHarborAI).GetMethod("CountAnimals", BindingFlags.Instance | BindingFlags.NonPublic), state4);
                    deployed = false;
                }
                catch (Exception e)
                {
                    Util.DebugPrint("Revert:");
                    UnityEngine.Debug.LogException(e);
                }
            }
        }

        private int Patch(ushort buildingID, ref Building data)
        {
            return int.MaxValue;
        }

        // This method is called on the Main thread but we want the releases on the Simulation thread.
        public static void ReleaseInstances(IThreading threading)
        {
            try
            {
                threading.QueueSimulationThread(ReleaseAction);
            }
            catch (Exception e)
            {
                Util.DebugPrint("ReleaseInstances:");
                UnityEngine.Debug.LogException(e);
            }
        }

        static void ReleaseAction()
        {
            int birdAI = 0;

            try
            {
                CitizenManager instance = Singleton<CitizenManager>.instance;
                CitizenInstance[] buffer = instance.m_instances.m_buffer;
                int capacity = buffer.Length;

                for (int i = 1; i < capacity; i++)
                    if ((buffer[i].m_flags & CitizenInstance.Flags.Created) != CitizenInstance.Flags.None)
                    {
                        CitizenAI ai = buffer[i].Info.m_citizenAI;

                        if (ai != null && ai.IsAnimal() && ai is BirdAI)
                        {
                            instance.ReleaseCitizenInstance((ushort) i);
                            birdAI++;
                        }
                    }
            }
            catch (Exception e)
            {
                Util.DebugPrint("ReleaseAction:");
                UnityEngine.Debug.LogException(e);
            }

            if (birdAI > 0)
                Util.DebugPrint("Released", birdAI.ToString(), "seagulls");
        }
    }
}
