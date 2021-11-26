//Reference : Assembly-CSharp-firstpass.dll, Core.dll, Modding.dll, Newtonsoft.Json.dll, Steamworks.dll, Ui.dll

using BrilliantSkies.Core.Constants;
using BrilliantSkies.Core.Timing;
using BrilliantSkies.Modding;
using BrilliantSkies.Ui.Displayer;
using BrilliantSkies.Ui.Displayer.Types;
using Newtonsoft.Json.Linq;
using Steamworks;
using System;
using System.IO;

namespace TestMod
{
    public class ModPlugin : GamePlugin
    {
        //Mod name
        public static string ModName { get; } = "TestMod";

        //Mod version
        public static System.Version ModVersion { get; } = new System.Version(0, 0, 0);

        //Game version
        public static System.Version GameVersion { get; } = new System.Version(3, 3, 2);

        //Steam workshop ID
        public static ulong WorkshopID { get; } = 0;



        private static string _myModDirPath;

        public static string MyModDirPath
        {
            get
            {
                if (string.IsNullOrEmpty(_myModDirPath))
                {
                    _myModDirPath = Path.Combine(Get.ProfilePaths.RootModDir().ToString(), ModName);
                }

                return _myModDirPath;
            }
        }



        private int _requestCount;

        private CallResult<SteamUGCRequestUGCDetailsResult_t> _steamCall;

        public string name { get; } = ModName;

        public System.Version version { get; } = ModVersion;

        public void OnLoad()
        {
            GameEvents.StartEvent.RegWithEvent(OnStart);
            GameEvents.Twice_Second.RegWithEvent(SteamUGCRequest);
        }

        public void OnStart()
        {
            GameEvents.StartEvent.UnregWithEvent(OnStart);

            UpdateJSON(Path.Combine(MyModDirPath, "plugin.json"));
            ModProblemOverwrite($"{ModName}  v{ModVersion}  Active!", MyModDirPath, string.Empty, false);
        }

        public void OnSave()
        {
        }

        private void AssemblyLoad(params string[] names)
        {
            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            foreach (string name in names)
            {
                Assembly.LoadFile(Path.Combine(folderPath, name));
            }
        }

        private void UpdateJSON(string pluginPath)
        {
            if (File.Exists(pluginPath))
            {
                JObject jObject = JObject.Parse(File.ReadAllText(pluginPath));

                string modVersionText = ModVersion.ToString();
                string gameVersionText = GameVersion.ToString();

                bool f0 = jObject["name"].ToString() != ModName;
                bool f1 = jObject["version"].ToString() != modVersionText;
                bool f2 = jObject["gameversion"].ToString() != gameVersionText;

                if (f0 || f1 || f2)
                {
                    if (f0) jObject["name"] = ModName;
                    if (f1) jObject["version"] = modVersionText;
                    if (f2) jObject["gameversion"] = gameVersionText;

                    File.WriteAllText(pluginPath, jObject.ToString());
                }
            }
        }

        public void ModProblemOverwrite(string InitModName, string InitModPath, string InitDescription, bool InitIsError)
        {
            ModProblems.AllModProblems.Remove(InitModPath);
            ModProblems.AddModProblem(InitModName, InitModPath, InitDescription, InitIsError);

            foreach (IGui_GuiSystem guiSystem in GuiDisplayer.GetSingleton().ActiveGuis)
            {
                guiSystem.OnActivateGui();
            }
        }

        private void SteamUGCRequest(ITimeStep t)
        {
            if (WorkshopID != 0 && ++_requestCount <= 5)
            {
                Console.WriteLine("SteamUGCRequest : " + _requestCount);

                SteamAPICall_t ugcDetails = SteamUGC.RequestUGCDetails(new PublishedFileId_t(WorkshopID), 0);
                _steamCall = new CallResult<SteamUGCRequestUGCDetailsResult_t>(Callback);
                _steamCall.Set(ugcDetails);
            }
            else
            {
                GameEvents.Twice_Second.UnregWithEvent(SteamUGCRequest);
            }
        }

        public void Callback(SteamUGCRequestUGCDetailsResult_t param, bool bIOFailure)
        {
            GameEvents.Twice_Second.UnregWithEvent(SteamUGCRequest);

            string description = param.m_details.m_rgchDescription;

            if (!string.IsNullOrEmpty(description))
            {
                using (StringReader reader = new StringReader(description))
                {
                    string inputLine;
                    System.Version latestVersion = null;

                    while ((inputLine = reader.ReadLine()) != null)
                    {
                        if (inputLine.StartsWith("Mod latest version "))
                        {
                            latestVersion = System.Version.Parse(inputLine.Remove(0, 18));
                            break;
                        }
                    }

                    if (latestVersion != null && ModVersion.CompareTo(latestVersion) == -1)
                    {
                        ModProblemOverwrite(ModName, MyModDirPath + "UpdateText", "New version released! v" + latestVersion, false);
                    }
                }
            }
        }
    }
}
