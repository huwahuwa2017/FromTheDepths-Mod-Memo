//Reference : Assembly-CSharp-firstpass.dll, Core.dll, Modding.dll, Newtonsoft.Json.dll, Ui.dll

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

namespace TestMod
{
    public class ModPlugin : GamePlugin
    {
        private bool UseSteam;

        private string ModPath;

        private int RequestCount;

        private CallResult<SteamUGCRequestUGCDetailsResult_t> SteamCall;



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

        public ulong PublishedFileId
        {
            //Steam Workshop ID
            get { return 0; }
        }



        public void OnLoad()
        {
            UseSteam = SteamInterface.UseSteam && PublishedFileId != 0;

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

                bool F0 = jObject["name"].ToString() != name;
                bool F1 = jObject["version"].ToString() != ModVersion;

                if (F0 || F1)
                {
                    if (F0) jObject["name"] = name;
                    if (F1) jObject["version"] = ModVersion;

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
            if (UseSteam && ++RequestCount <= 4)
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
                        if (InputLine.StartsWith("Mod latest version "))
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
