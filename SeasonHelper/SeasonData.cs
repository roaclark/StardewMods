using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SeasonHelper
{
    public class SeasonData
    {
        public class SeasonObject
        {
            public int objectIndex;
            public string[] seasons = new string[] { };

            public int shippedDone = 0;
            public int shippedNeeded = 0;
            public int caughtDone = 0;
            public int caughtNeeded = 0;
            public int cookingDone = 0;
            public int cookingNeeded = 0;
            public int craftingDone = 0;
            public int craftingNeeded = 0;
            public int bundleDone = 0;
            public int bundleNeeded = 0;

            public SeasonObject(int objectIndex, string[] seasons)
            {
                this.objectIndex = objectIndex;
                this.seasons = seasons;
            }
        }

        private IDictionary<string, List<SeasonObject>> seasonCrops = new Dictionary<string, List<SeasonObject>>();
        private IDictionary<string, List<SeasonObject>> seasonFish= new Dictionary<string, List<SeasonObject>>();
        private IDictionary<string, List<SeasonObject>> seasonForage= new Dictionary<string, List<SeasonObject>>();

        public SeasonData()
        {
            seasonCrops.Add("spring", new List<SeasonObject>());
            seasonCrops.Add("summer", new List<SeasonObject>());
            seasonCrops.Add("fall", new List<SeasonObject>());
            seasonCrops.Add("winter", new List<SeasonObject>());

            seasonFish.Add("spring", new List<SeasonObject>());
            seasonFish.Add("summer", new List<SeasonObject>());
            seasonFish.Add("fall", new List<SeasonObject>());
            seasonFish.Add("winter", new List<SeasonObject>());

            seasonForage.Add("spring", new List<SeasonObject>());
            seasonForage.Add("summer", new List<SeasonObject>());
            seasonForage.Add("fall", new List<SeasonObject>());
            seasonForage.Add("winter", new List<SeasonObject>());
        }

        public void addCrop(SeasonObject crop)
        {
            foreach (string season in crop.seasons)
            {
                seasonCrops[season].Add(crop);
            }
        }

        public void addFish(SeasonObject fish)
        {
            foreach (string season in fish.seasons)
            {
                seasonFish[season].Add(fish);
            }
        }

        public void addForage(SeasonObject forage)
        {
            foreach (string season in forage.seasons)
            {
                seasonForage[season].Add(forage);
            }
        }

        public List<SeasonObject> getCrops(string season)
        {
            return seasonCrops[season];
        }

        public List<SeasonObject> getFish(string season)
        {
            return seasonFish[season];
        }

        public List<SeasonObject> getForage(string season)
        {
            return seasonForage[season];
        }
    }
}
