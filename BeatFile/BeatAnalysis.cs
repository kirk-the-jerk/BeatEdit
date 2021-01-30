using System;
using System.Collections.Generic;
using System.Text;

namespace BeatFile
{
    public class Histogram<TVal> : SortedDictionary<TVal, uint>
    {
        public void IncrementCount(TVal binToIncrement)
        {
            if (ContainsKey(binToIncrement))
            {
                this[binToIncrement]++;
            }
            else
            {
                Add(binToIncrement, 1);
            }
        }
    }

    public class BeatAnalysis
    {
        public Histogram<int> EnergyHist = null;
        public Histogram<int> MagHist = null;
        public Histogram<int> BpmHist = null;

        public void Analyze(List<BeatInstance> beatList)
        {
            try
            {
                double t = 0.0;
                EnergyHist = new Histogram<int>();
                MagHist = new Histogram<int>();
                BpmHist = new Histogram<int>();

                foreach (BeatInstance bi in beatList)
                {
                    EnergyHist.IncrementCount(bi.EnergyLevel);
                    MagHist.IncrementCount(System.Convert.ToInt32(bi.Magnitude));
                    double bpm = 60.0 / (bi.TriggerTime - t);
                    BpmHist.IncrementCount(System.Convert.ToInt32(bpm / 5.0) * 5);

                    t = bi.TriggerTime;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("error analyzing", ex);
            }
        }

        public void Show()
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("Engergy Levels");
                Console.WriteLine("--------------");

                foreach (KeyValuePair<int, uint> hi in (EnergyHist as SortedDictionary<int, uint>))
                    Console.WriteLine("{0} x {1}", hi.Key, hi.Value);

                Console.WriteLine();
                Console.WriteLine("Magnitudes");
                Console.WriteLine("----------");

                foreach (KeyValuePair<int, uint> hi in (MagHist as SortedDictionary<int, uint>))
                    Console.WriteLine("{0} x {1}", hi.Key, hi.Value);

                Console.WriteLine();
                Console.WriteLine("BPM");
                Console.WriteLine("---");

                foreach (KeyValuePair<int, uint> hi in (BpmHist as SortedDictionary<int, uint>))
                    Console.WriteLine("{0} x {1}", hi.Key, hi.Value);
            }
            catch (Exception ex)
            {
                throw new Exception("error showing analysis", ex);
            }
        }
    }
}
