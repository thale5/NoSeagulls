using ICities;

namespace NoSeagulls
{
    public class Mod : IUserMod
    {
        public string Name
        {
            get
            {
                return "No Seagulls";
            }
        }

        public string Description
        {
            get { return "Removes seagulls from your city and prevents new ones from spawning"; }
        }
    }
}
