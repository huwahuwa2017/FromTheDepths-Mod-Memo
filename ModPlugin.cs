using BrilliantSkies.Core;
using BrilliantSkies.Modding;
using System;

namespace TestMod
{
    public class ModPlugin : GamePlugin, GamePlugin_PostLoad
    {
        public string name
        {
            get { return "TestMod"; }
        }

        public Version version
        {
            get { return new Version(0, 0); }
        }

        public void OnLoad()
        {
            SafeLogging.Log("Loaded " + name);
        }

        public void OnSave()
        {
            SafeLogging.Log("Saved " + name);
        }

        public bool AfterAllPluginsLoaded()
        {
            return true;
        }
    }
}
