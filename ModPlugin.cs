//Reference : Core.dll, Modding.dll, Newtonsoft.Json.dll

using BrilliantSkies.Core.Constants;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Modding;
using Newtonsoft.Json.Linq;
using System.IO;

namespace ModName
{
    public class ModPlugin : GamePlugin
    {
        public string name
        {
            //Mod Name
            get { return "ModName"; }
        }

        public System.Version version
        {
            //Mod Version
            get { return new System.Version(0, 0, 0); }
        }



        public void OnLoad()
        {
            GameEvents.StartEvent += OnStart;

            string ModPath = Path.Combine(Get.ProfilePaths.RootModDir().ToString(), name);
            UpdateJSON(Path.Combine(ModPath, "plugin.json"));
            ModProblemOverwrite($"{name}  v{version}  Active!", ModPath, string.Empty, false);
        }

        public void OnStart()
        {
            GameEvents.StartEvent -= OnStart;
        }

        public void OnSave()
        {

        }

        private void UpdateJSON(string FilePath)
        {
            string ModVersion = version.ToString();
            string FTDGameVersion = Get.Game.VersionString;

            JObject jObject = JObject.Parse(File.ReadAllText(FilePath));

            bool F0 = jObject["name"].ToString() != name;
            bool F1 = jObject["version"].ToString() != ModVersion;
            bool F2 = jObject["gameversion"].ToString() != FTDGameVersion;

            if (F0 || F1 || F2)
            {
                if (F0) jObject["name"] = name;
                if (F1) jObject["version"] = ModVersion;
                if (F2) jObject["gameversion"] = FTDGameVersion;

                File.WriteAllText(FilePath, jObject.ToString());
            }
        }

        public void ModProblemOverwrite(string name, string path, string description, bool isError)
        {
            ModProblems.AllModProblems.Remove(path);
            ModProblems.AddModProblem(name, path, description, isError);
        }
    }
}
