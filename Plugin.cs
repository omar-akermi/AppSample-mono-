using MelonLoader;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using ScheduleOne.UI.Phone;
using ScheduleOne.DevUtilities;
using ScheduleOne.UI.Phone.ProductManagerApp;
using ScheduleOne.Product;
using HarmonyLib;
using ScheduleOneEnhanced.API.Quests;
using ScheduleOne.Quests;
using System.Linq;
using SilkRoad.Quests;
[assembly: HarmonyDontPatchAll]

[assembly: MelonInfo(typeof(SilkRoad.Plugin), "SilkRoad", "1.0.0", "Akermi")]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace SilkRoad
{
    public class Plugin : MelonMod
    {
        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("🚚 Silk Road mod loaded!");
        }

        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            if (sceneName != "Main") return;

            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("com.silkroad.npc");
            harmony.PatchAll(typeof(QuestManagerPatch).Assembly);


            GameObject.DontDestroyOnLoad(npcGO);

                if (SilkRoadAppUI.Instance != null)
                {
                    SilkRoadAppUI.Instance.HandleQuestCompleteFromPlugin();
                }
                else
                {
                    MelonLogger.Warning("⚠️ SilkRoadAppUI.Instance was null — could not update UI.");
                }
            };
        }



            foreach (Transform child in parentCanvas)
            {
                if (child.name.Contains("ProductManagerApp") && child.GetComponent<ProductManagerApp>() == null)
                {
                    GameObject.Destroy(child.gameObject);
                    MelonLogger.Msg("🗑️ Removed dangling ProductManagerApp clone.");
                }
            }

            Transform originalApp = parentCanvas.Find("ProductManagerApp");
            if (originalApp == null)
            {
                MelonLogger.Error("❌ ProductManagerApp not found. Cannot clone.");
                yield break;
            }

            GameObject clonedApp = GameObject.Instantiate(originalApp.gameObject, parentCanvas);
            clonedApp.name = "SilkRoadApp";
            clonedApp.SetActive(false);

            var oldApp = clonedApp.GetComponent<ProductManagerApp>();
            if (oldApp != null)
                GameObject.Destroy(oldApp);

            Transform container = clonedApp.transform.Find("Container");
            if (container != null)
            {
                foreach (Transform child in container)
                    GameObject.Destroy(child.gameObject);
            }

            var silkApp = clonedApp.AddComponent<SilkRoadApp>();
            silkApp.appContainer = container.GetComponent<RectTransform>();

            MelonLogger.Msg("✅ SilkRoad app attached to AppsCanvas.");

            SetupSilkRoadIcon(clonedApp);
        }

        private void SetupSilkRoadIcon(GameObject appGO)
        {
            string iconPath = "SilkRoadIcon.png";
            Sprite iconSprite = LoadImage(iconPath);

            var iconGrid = GameObject.Find("Player_Local/CameraContainer/Camera/OverlayCamera/GameplayMenu/Phone/phone/HomeScreen/AppIcons");
            if (iconGrid == null)
            {
                MelonLogger.Error("❌ AppIcons grid not found.");
                return;
            }

            foreach (Transform icon in iconGrid.transform)
            {
                var label = icon.Find("Label")?.GetComponent<Text>()?.text;
                if (icon.name == "SilkRoadIcon" || label == "Silkroad")
                    GameObject.Destroy(icon.gameObject);
            }

            bool foundFirstProducts = false;
            foreach (Transform icon in iconGrid.transform)
            {
                var label = icon.Find("Label")?.GetComponent<Text>()?.text;
                if (label == "Products")
                {
                    if (!foundFirstProducts)
                    {
                        foundFirstProducts = true;
                        continue;
                    }
                    GameObject.Destroy(icon.gameObject);
                    MelonLogger.Msg("🗑️ Removed extra Products icon.");
                }
            }

            Transform existingIcon = iconGrid.transform.GetChild(6);
            GameObject newIcon = GameObject.Instantiate(existingIcon.gameObject, iconGrid.transform);
            newIcon.name = "SilkRoadIcon";

            newIcon.transform.Find("Label").GetComponent<Text>().text = "Silkroad";
            if (iconSprite != null)
            {
                var img = newIcon.transform.Find("Mask/Image").GetComponent<Image>();
                img.sprite = iconSprite;
            }

            var btn = newIcon.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                GameObject.Destroy(btn.gameObject);
                MelonLogger.Msg("🗑️ Removed original broken SilkRoad icon (self)");
                appGO.SetActive(true);
            });

            MelonLogger.Msg("✅ SilkRoad icon created.");
        }

        public static Sprite LoadImage(string fileName)
        {
            string fullPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), fileName);
            if (!File.Exists(fullPath))
            {
                MelonLogger.Error($"❌ Icon file not found: {fullPath}");
                return null;
            }

            try
            {
                byte[] data = File.ReadAllBytes(fullPath);
                Texture2D tex = new Texture2D(2, 2);
                if (tex.LoadImage(data))
                {
                    return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                }
            }
            catch (System.Exception ex)
            {
                MelonLogger.Error("❌ Failed to load sprite: " + ex);
            }

            return null;
        }
        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
            // Reset quest load flag on scene change
            QuestsLoaderPatch.Reset();
        }

    }
}