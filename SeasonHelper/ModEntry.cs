using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Network;

namespace SeasonHelper
{
    using IngredientStats = IDictionary<int, SeasonData.TaskStats>;

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
            "IslandSecret",
            "fishingGame",
            "Temp",
            "WitchSwamp",
            "BugLand",
            "Sewer",
            "UndergroundMine",
            "Farm",
            "Desert"
        };
        private readonly ISet<int> polycultureCrops = new HashSet<int> { 24, 188, 190, 192, 248, 250, 252, 254, 256, 258, 260, 262, 264, 266, 268, 270, 272, 274, 276, 278, 280, 282, 284, 300, 304, 398, 400, 433 };


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
            data = new SeasonData();
            IngredientStats cookingData = parseCookingRecipes();
            IngredientStats craftingData = parseCraftingRecipes();
            IngredientStats bundleData = parseBundleData();
            parseCrops(cookingData, craftingData, bundleData);
            parseFishAndForage(cookingData, craftingData, bundleData);
        }

        private IngredientStats parseBundleData()
        {
            StardewValley.Locations.CommunityCenter communityCenter = Game1.getLocationFromName("CommunityCenter") as StardewValley.Locations.CommunityCenter;
            IDictionary<string, string> bundleData = Game1.content.Load<Dictionary<string, string>>("Data\\Bundles");
            IngredientStats ingredients = new Dictionary<int, SeasonData.TaskStats>();

            foreach (KeyValuePair<string, string> entry in bundleData)
            {
                int bundleKey = Convert.ToInt32(entry.Key.Split('/')[1]);
                string[] bundleValues = entry.Value.Split('/');
                string[] ingredientValues = bundleValues[2].Split(' ');

                int numPossibleIngredients = ingredientValues.Length / 3;
                int bundleNumberNeeded = bundleValues.Length > 4 ? Convert.ToInt32(bundleValues[4]) : numPossibleIngredients;
                bool[] bundleCompletionStats = communityCenter.bundles[bundleKey];

                int bundleNumberCompleted = 0;
                for (int i = 0; i < numPossibleIngredients; i++)
                {
                    if (bundleCompletionStats[i])
                    {
                        bundleNumberCompleted += 1;
                    }
                }
                bool bundleComplete = bundleNumberCompleted >= bundleNumberNeeded;

                for (int i = 0; i < ingredientValues.Length; i += 3)
                {
                    int objectIndex = Convert.ToInt32(ingredientValues[i]);
                    int count = Convert.ToInt32(ingredientValues[i + 1]);

                    if (!ingredients.ContainsKey(objectIndex))
                    {
                        ingredients.Add(objectIndex, new SeasonData.TaskStats(0, 0));
                    }

                    ingredients[objectIndex].needed += count;

                    if (bundleComplete || bundleCompletionStats[i / 3])
                    {
                        ingredients[objectIndex].done += count;
                    }
                }
            }

            return ingredients;
        }

        private IngredientStats parseCookingRecipes()
        {
            IDictionary<string, string> recipeData = Game1.content.Load<Dictionary<string, string>>("Data\\CookingRecipes");
            return parseRecipeHelper(recipeData, Game1.player.cookingRecipes);
        }

        private IngredientStats parseCraftingRecipes()
        {
            IDictionary<string, string> recipeData = Game1.content.Load<Dictionary<string, string>>("Data\\CraftingRecipes");
            return parseRecipeHelper(recipeData, Game1.player.craftingRecipes);
        }

        private IngredientStats parseRecipeHelper(IDictionary<string, string> recipeData, NetStringDictionary<int, NetInt> playerData)
        {
            IngredientStats ingredients = new Dictionary<int, SeasonData.TaskStats>();

            foreach (KeyValuePair<string, string> entry in recipeData)
            {
                playerData.TryGetValue(entry.Key, out int numMade);
                bool made = numMade > 0;
                string[] recipeValues = entry.Value.Split('/');
                string[] ingredientValues = recipeValues[0].Split(' ');

                for (int i = 0; i < ingredientValues.Length; i += 2)
                {
                    int objectIndex = Convert.ToInt32(ingredientValues[i]);
                    int count = Convert.ToInt32(ingredientValues[i + 1]);

                    if (!ingredients.ContainsKey(objectIndex))
                    {
                        ingredients.Add(objectIndex, new SeasonData.TaskStats(0, 0));
                    }

                    ingredients[objectIndex].needed += count;
                    if (made)
                    {
                        ingredients[objectIndex].done += count;
                    }
                }
            }

            return ingredients;
        }

        private void parseCrops(IngredientStats cookingData, IngredientStats craftingData, IngredientStats bundleData)
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

                    int shippedNeeded = polycultureCrops.Contains(objectIndex) ? 15 : 1;
                    Game1.player.basicShipped.TryGetValue(objectIndex, out int shipped);
                    shipped = Math.Min(shipped, shippedNeeded);
                    crop.addTaskStats("Shipped", shippedNeeded, shipped);

                    if (cookingData.ContainsKey(objectIndex))
                    {
                        crop.addTaskStats("Cooking", cookingData[objectIndex]);
                    }
                    if (craftingData.ContainsKey(objectIndex))
                    {
                        crop.addTaskStats("Crafting", craftingData[objectIndex]);
                    }
                    if (bundleData.ContainsKey(objectIndex))
                    {
                        crop.addTaskStats("Bundle", bundleData[objectIndex]);
                    }

                    this.data.addCrop(crop);
                }
            }
        }

        private void parseFishAndForage(IngredientStats cookingData, IngredientStats craftingData, IngredientStats bundleData)
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
                string[] seasonList = convertBoolArrayToSeasonList(entry.Value);
                SeasonData.SeasonObject fish = new SeasonData.SeasonObject(
                    entry.Key,
                    seasonList
                );

                if (cookingData.ContainsKey(entry.Key))
                {
                    fish.addTaskStats("Cooking", cookingData[entry.Key]);
                }
                if (craftingData.ContainsKey(entry.Key))
                {
                    fish.addTaskStats("Crafting", craftingData[entry.Key]);
                }
                if (bundleData.ContainsKey(entry.Key))
                {
                    fish.addTaskStats("Bundle", bundleData[entry.Key]);
                }

                if (fish.totalStats.needed == 0)
                {
                    fish.addTaskStats("Caught", 1, Game1.player.fishCaught.ContainsKey(entry.Key) ? 1 : 0);
                }

                this.data.addFish(fish);
            }

            foreach (KeyValuePair<int, bool[]> entry in forageData)
            {
                string[] seasonList = convertBoolArrayToSeasonList(entry.Value);
                SeasonData.SeasonObject forage = new SeasonData.SeasonObject(
                    entry.Key,
                    seasonList
                );

                int shippedNeeded = 1;
                Game1.player.basicShipped.TryGetValue(entry.Key, out int shipped);
                shipped = Math.Min(shipped, shippedNeeded);
                forage.addTaskStats("Shipped", shippedNeeded, shipped);

                if (cookingData.ContainsKey(entry.Key))
                {
                    forage.addTaskStats("Cooking", cookingData[entry.Key]);
                }
                if (craftingData.ContainsKey(entry.Key))
                {
                    forage.addTaskStats("Crafting", craftingData[entry.Key]);
                }
                if (bundleData.ContainsKey(entry.Key))
                {
                    forage.addTaskStats("Bundle", bundleData[entry.Key]);
                }

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
                populateData();
                Game1.activeClickableMenu = new SeasonMenu(Monitor, data);
            }
        }
    }
}