using ICities;

namespace NoSeagulls
{
    public class Loading : LoadingExtensionBase
    {
        private ILoading ld;

        public override void OnCreated(ILoading loading)
        {
            ld = loading;
            PatchSeagulls.Deploy();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
                PatchSeagulls.ReleaseInstances(ld.managers.threading);
        }

        public override void OnReleased()
        {
            PatchSeagulls.Revert();
        }
    }
}
