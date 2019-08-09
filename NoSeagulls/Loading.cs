using ICities;

namespace NoSeagulls
{
    public class Loading : LoadingExtensionBase
    {
        ILoading ld;
        public override void OnCreated(ILoading loading) => ld = loading;

        public override void OnLevelLoaded(LoadMode mode)
        {
            if (mode == LoadMode.LoadGame || mode == LoadMode.NewGame)
                PatchSeagulls.Deploy(ld.managers.threading);
        }
    }
}
