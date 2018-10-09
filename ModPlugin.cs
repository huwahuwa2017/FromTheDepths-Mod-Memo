using BrilliantSkies.Core.Constants;
using BrilliantSkies.Modding;
using System;
using System.IO;

public class ModPlugin : GamePlugin
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
        ModProblem.AddModProblem(name, Path.Combine(Get.ProfilePaths.RootModDir().ToString(), name), "Mod Version " + version, false);
    }

    public void OnSave()
    {
    }
}
