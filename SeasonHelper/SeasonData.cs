using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace SeasonHelper
{
    public class SeasonData
    {
        public class TaskStats
        {
            public int needed = 0;
            public int done = 0;

            public TaskStats(int needed, int done)
            {
                this.needed = needed;
                this.done = done;
            }
        }

        public class SeasonObject
        {
            public int objectIndex;
            public string[] seasons = new string[] { };
            public IDictionary<string, TaskStats> tasks = new Dictionary<string, TaskStats>();
            public TaskStats totalStats = new TaskStats(0, 0);

            public SeasonObject(int objectIndex, string[] seasons)
            {
                this.objectIndex = objectIndex;
                this.seasons = seasons;
            }

            public void addTaskStats(string name, int needed, int done)
            {
                tasks.Add(name, new TaskStats(needed, done));
                totalStats.needed += needed;
                totalStats.done += done;
            }

            public void addTaskStats(string name, TaskStats stats)
            {
                addTaskStats(name, stats.needed, stats.done);
            }

            public string prettyPrint()
            {
                List<string> lines = new List<String>();
                lines.Add("Item " + objectIndex.ToString());
                foreach (KeyValuePair<string, TaskStats> entry in tasks)
                {
                    lines.Add(entry.Key + ": " + entry.Value.done.ToString() + "/" + entry.Value.needed.ToString());
                }
                return String.Join("\n", lines);
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
