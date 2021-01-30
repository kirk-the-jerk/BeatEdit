using System;
using System.IO;
using System.Collections.Generic;
using BeatFile;

namespace BeatView
{
    class BeatViewProgram
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 1)
                {
                    Console.WriteLine("missing filename");
                    return;
                }

                string filename = args[0];
                if (Path.GetExtension(filename) == "")
                    filename += ".txt";

                if (!File.Exists(filename))
                {
                    Console.WriteLine("{0} not found", filename);
                    return;
                }

                Console.WriteLine("reading {0}", filename);

                BeatFile.BeatFile bf = new BeatFile.BeatFile();
                bf.Read(filename);
                bf.Parse();

                List<BeatInstance> beatlist = bf.GetAllBeats();

                ShowList(beatlist);

                bf.GetBeatByIndex(477).EnergyLevel = 0;

                if (false)
                {
                    bf.Update();
                    bf.Backup();
                    bf.Write();
                }

                BeatAnalysis ba = bf.Analyze();
                ba.Show();

                Console.WriteLine(":)");
            }
            catch (Exception ex)
            {
                Console.WriteLine("!!!");
                Console.WriteLine("{0}", ex);
                Console.WriteLine("{0}", ex.InnerException);
                Console.WriteLine(":(");
            }
        }

        static void ShowList(List<BeatInstance> beatlist)
        {
            foreach (BeatInstance bi in beatlist)
            {
                Console.WriteLine("{0,3}: time={1} mag={2} energy={3} ", bi.Index, bi.TriggerTime, bi.Magnitude, bi.EnergyLevel);
            }
        }
    }
}
