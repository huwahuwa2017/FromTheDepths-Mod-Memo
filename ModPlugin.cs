//Reference : Core.dll, Modding.dll, Newtonsoft.Json.dll

using BrilliantSkies.Core.Constants;
using BrilliantSkies.Modding;
using Newtonsoft.Json.Linq;
using System.IO;
using static BrilliantSkies.Core.Timing.GameEvents;

namespace TestMod
{
    public class ModPlugin : GamePlugin
    {
        public string name
        {
            //Mod Name
            get { return "TestMod"; }
        }

        public System.Version version
        {
            //Mod Version
            get { return new System.Version(0, 0, 0); }
        }



        public void OnLoad()
        {
            string ModPath = Path.Combine(Get.ProfilePaths.RootModDir().ToString(), name);
            UpdateJSON(Path.Combine(ModPath, "plugin.json"));
            ModProblemOverwrite(name + "  v" + version, ModPath, "Active!", false);

            StartEvent += OnStart;
        }

        public void OnStart()
        {
            StartEvent -= OnStart;
        }

        public void OnSave()
        {

        }

        private void UpdateJSON(string FilePath)
        {
            string ModVersion = version.ToString();
            string FTDVersion = Get.Game.VersionString;

            JObject jObject = JObject.Parse(File.ReadAllText(FilePath));

            bool F0 = jObject["name"].ToString() != name;
            bool F1 = jObject["version"].ToString() != ModVersion;
            bool F2 = jObject["gameversion"].ToString() != FTDVersion;

            if (F0 || F1 || F2)
            {
                if (F0) jObject["name"] = name;
                if (F1) jObject["version"] = ModVersion;
                if (F2) jObject["gameversion"] = FTDVersion;

                File.WriteAllText(FilePath, jObject.ToString());
            }
        }

        public void ModProblemOverwrite(string InitModName, string InitModPath, string InitDescription, bool InitIsError)
        {
            ModProblem.AllModProblems.Remove(InitModPath);
            ModProblem.AddModProblem(InitModName, InitModPath, InitDescription, InitIsError);
        }
    }
}
