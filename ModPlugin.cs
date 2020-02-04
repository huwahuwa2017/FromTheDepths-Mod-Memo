//Reference : Assembly-CSharp-firstpass.dll, Core.dll, Modding.dll, Newtonsoft.Json.dll

using BrilliantSkies.Core.Constants;
using BrilliantSkies.Core.SteamworksIntegration;
using BrilliantSkies.Modding;
using BrilliantSkies.Ui.Displayer;
using BrilliantSkies.Ui.Displayer.Types;
using Newtonsoft.Json.Linq;
using Steamworks;
using System;
using System.IO;
using BrilliantSkies.Core.Timing;

namespace AdvancedMimicUi
{
    public class ModPlugin : GamePlugin
    {
        private bool useSteam;

        private string ModPath;

        private int RequestCount;

        private CallResult<SteamUGCRequestUGCDetailsResult_t> SteamCall;



        public string name
        {
            //Mod Name
            get { return "AdvancedMimicUi"; }
        }

        public System.Version version
        {
            //Mod Version
            get { return new System.Version(0, 0, 0); }
        }

        public ulong PublishedFileId
        {
            //Steam Workshop ID
            get { return 1513554113; }
        }



        public void OnLoad()
        {
            useSteam = SteamInterface.UseSteam && PublishedFileId != 0;

            ModPath = Path.Combine(Get.ProfilePaths.RootModDir().ToString(), name);
            UpdateJSON(Path.Combine(ModPath, "plugin.json"));
            ModProblemOverwrite($"{name}  v{version}  Active!", ModPath, string.Empty, false);

            GameEvents.StartEvent += OnStart;
            GameEvents.Twice_Second += SteamUGCRequest;
        }

        public void OnStart()
        {
            GameEvents.StartEvent -= OnStart;
        }

        public void OnSave()
        {
        }

        private void UpdateJSON(string pluginPath)
        {
            if (File.Exists(pluginPath))
            {
                JObject jObject = JObject.Parse(File.ReadAllText(pluginPath));

                string ModVersion = version.ToString();
                string FTDGameVersion = Get.Game.VersionString;

                bool F0 = jObject["name"].ToString() != name;
                bool F1 = jObject["version"].ToString() != ModVersion;
                bool F2 = jObject["gameversion"].ToString() != FTDGameVersion;

                if (F0 || F1 || F2)
                {
                    if (F0) jObject["name"] = name;
                    if (F1) jObject["version"] = ModVersion;
                    if (F2) jObject["gameversion"] = FTDGameVersion;

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
            if (useSteam && ++RequestCount <= 4)
            {
                Console.WriteLine("SteamUGCRequest : " + RequestCount);

                SteamAPICall_t UGCDetails = SteamUGC.RequestUGCDetails(new PublishedFileId_t(PublishedFileId), 0);
                SteamCall = new CallResult<SteamUGCRequestUGCDetailsResult_t>(Callback);
                SteamCall.Set(UGCDetails);
            }
            else
            {
                GameEvents.Twice_Second -= SteamUGCRequest;
            }
        }

        public void Callback(SteamUGCRequestUGCDetailsResult_t param, bool bIOFailure)
        {
            GameEvents.Twice_Second -= SteamUGCRequest;

            string Description = param.m_details.m_rgchDescription;

            if (!string.IsNullOrEmpty(Description))
            {
                using (StringReader Reader = new StringReader(Description))
                {
                    string InputLine;
                    System.Version LatestVersion = null;

                    while ((InputLine = Reader.ReadLine()) != null)
                    {
                        if (InputLine.StartsWith("Latest mod version "))
                        {
                            LatestVersion = System.Version.Parse(InputLine.Remove(0, 18));
                            break;
                        }
                    }

                    if (LatestVersion != null && version.CompareTo(LatestVersion) == -1)
                    {
                        ModProblemOverwrite(name, ModPath + "UpdateText", "New version released! v" + LatestVersion, false);
                    }
                }
            }
        }
    }
}