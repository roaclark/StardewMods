using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace SeasonHelper
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        // Ignores some crops like Qi Bean and forage crops
        private readonly ISet<int> ignoredCrops = new HashSet<int> { 495, 496, 497, 498, 885, 890 };
        private readonly ISet<string> ignoredLocations = new HashSet<string> {
            "IslandNorth",
            "IslandSouth",
            "IslandSouthEast",
            "IslandWest",
            "IslandSouthEastCave",
            "IslandNorthCave1",
            "IslandSecret"
        };

        private SeasonData data = new SeasonData();

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
        }

        private void populateData()
        {
            parseCrops();
            parseFishAndForage();
        }

        private void parseCrops()
        {
            IDictionary<int, string> cropData = Game1.content.Load<Dictionary<int, string>>("Data\\Crops");
            foreach (KeyValuePair<int, string> entry in cropData)
            {
                if (!ignoredCrops.Contains(entry.Key))
                {
                    string[] cropValues = entry.Value.Split('/');
                    string[] cropSeasons = cropValues[1].Split(' ');
                    int objectIndex = Convert.ToInt32(cropValues[3]);
                    SeasonData.SeasonObject crop = new SeasonData.SeasonObject(objectIndex, cropSeasons);

                    // TODO: Switch between 15 and 1 depending on shipped achievement
                    Game1.player.basicShipped.TryGetValue(objectIndex, out int shipped);
                    crop.addTaskStats("Shipped", 15, shipped);

                    this.data.addCrop(crop);
                }
            }
        }

        private void parseFishAndForage()
        {
            IDictionary<string, string> locationData = Game1.content.Load<Dictionary<string, string>>("Data\\Locations");

            IDictionary<int, bool[]> fishData = new Dictionary<int, bool[]>();
            IDictionary<int, bool[]> forageData = new Dictionary<int, bool[]>();

            foreach (KeyValuePair<string, string> entry in locationData)
            {
                if (!ignoredLocations.Contains(entry.Key))
                {
                    string[] locationValues = entry.Value.Split('/');

                    for (int seasonI = 0; seasonI < 4; seasonI++)
                    {
                        string[] forageValues = locationValues[seasonI].Split(' ');
                        if (forageValues.Length > 1)
                        {
                            for (int forageI = 0; forageI < forageValues.Length; forageI += 2)
                            {
                                int itemId = Convert.ToInt32(forageValues[forageI]);
                                if (!forageData.ContainsKey(itemId))
                                {
                                    forageData.Add(itemId, new bool[] { false, false, false, false });
                                }
                                forageData[itemId][seasonI] = true;
                            }
                        }
                    }

                    for (int seasonI = 0; seasonI < 4; seasonI++)
                    {
                        string[] fishValues = locationValues[seasonI + 4].Split(' ');
                        if (fishValues.Length > 1)
                        {
                            for (int fishI = 0; fishI < fishValues.Length; fishI += 2)
                            {
                                int itemId = Convert.ToInt32(fishValues[fishI]);
                                if (!fishData.ContainsKey(itemId))
                                {
                                    fishData.Add(itemId, new bool[] { false, false, false, false });
                                }
                                fishData[itemId][seasonI] = true;
                            }
                        }
                    }

                }
            }

            foreach (KeyValuePair<int, bool[]> entry in fishData)
            {
                SeasonData.SeasonObject fish = new SeasonData.SeasonObject(
                    entry.Key,
                    convertBoolArrayToSeasonList(entry.Value)
                );
                this.data.addFish(fish);
            }

            foreach (KeyValuePair<int, bool[]> entry in forageData)
            {
                SeasonData.SeasonObject forage = new SeasonData.SeasonObject(
                    entry.Key,
                    convertBoolArrayToSeasonList(entry.Value)
                );

                Game1.player.basicShipped.TryGetValue(entry.Key, out int shipped);
                forage.addTaskStats("Shipped", 1, shipped);

                this.data.addForage(forage);
            }
        }

        private string[] convertBoolArrayToSeasonList(bool[] arr)
        {
            List<string> seasons = new List<string>();
            if (arr[0])
            {
                seasons.Add("spring");
            }
            if (arr[1])
            {
                seasons.Add("summer");
            }
            if (arr[2])
            {
                seasons.Add("fall");
            }
            if (arr[3])
            {
                seasons.Add("winter");
            }
            return seasons.ToArray();
        }


        /*********
        ** Private methods
        *********/
        /// <summary>Raised after the player presses a button on the keyboard, controller, or mouse.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event data.</param>
        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (Game1.activeClickableMenu != null) return;

            // TODO check for e.Button == Config.KeyBinding
            if (Context.IsPlayerFree && e.Button == SButton.V)
            {
                data = new SeasonData();
                populateData();
                Game1.activeClickableMenu = new SeasonMenu(data);
            }
        }
    }
}