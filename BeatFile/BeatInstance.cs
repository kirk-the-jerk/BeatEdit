using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BeatFile
{
    public class BeatInstance
    {
        public int Index;
        public double TriggerTime;
        public double Magnitude;
        public int EnergyLevel;
        public int FileOffset;

        public BeatInstance(int index, double triggerTime, double magnitude, int energyLevel, int fileOffset)
        {
            Index = index;
            TriggerTime = triggerTime;
            Magnitude = magnitude;
            EnergyLevel = energyLevel;
            FileOffset = fileOffset;
        }

        public BeatInstance(string stuff, int fileOffset)
        {
            try
            {
                Match m = Regex.Match(stuff, "_index\\\\\":[0-9]+");
                Index = System.Convert.ToInt32(Regex.Match(m.Value, "[0-9]+").Value);

                m = Regex.Match(stuff, "_magnitude\\\\\":[0-9]*(?:\\.[0-9]*)?");
                Magnitude = System.Convert.ToDouble(Regex.Match(m.Value, "[0-9]*\\.[0-9]*").Value);

                m = Regex.Match(stuff, "_triggerTime\\\\\":[0-9]*(?:\\.[0-9]*)?");
                TriggerTime = System.Convert.ToDouble(Regex.Match(m.Value, "[0-9]*\\.[0-9]*").Value);

                m = Regex.Match(stuff, "_energyLevel\\\\\":[0-9]");
                EnergyLevel = System.Convert.ToInt32(Regex.Match(m.Value, "[0-9]").Value);

                FileOffset = fileOffset;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
