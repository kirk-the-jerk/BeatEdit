using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace BeatFile
{
    public class BeatFile
    {
        private string _fileName;
        private string _fileText;
        private List<BeatInstance> _beatList;
        private string _fileTrack;
        private string _fileArtist;

        public string Track { get => _fileTrack; }
        public string Artist { get => _fileArtist; }

        public void Read(string filename)
        {
            _fileName = filename;
            _fileText = "";

            try
            {
                _fileText = File.ReadAllText(_fileName);
            }
            catch (Exception ex)
            {
                throw new Exception("error reading file", ex);
            }
        }

        public void Parse()
        {
            string current = "header";

            try
            {
                // get song info
                Regex reg = new Regex("original.*?,");
                MatchCollection matches = reg.Matches(_fileText);
                if (matches.Count == 3)
                {
                    string s = matches[1].Value.Substring(matches[1].Value.IndexOf(':') + 2);
                    _fileTrack = s.Substring(0, s.Length-2);

                    s = matches[2].Value.Substring(matches[2].Value.IndexOf(':') + 2);
                    _fileArtist = s.Substring(0, s.Length - 2);
                }

                // get beats
                reg = new Regex("_index.{1,100}?_magnitude.{1,400}?_energyLevel\\\\\":[0-9]");

                matches = reg.Matches(_fileText, _fileText.IndexOf(@"_beats"));
//                Console.WriteLine("{0} matches", matches.Count);

                _beatList = new List<BeatInstance>();
                foreach (Match m in matches)
                {
                    current = m.Value;
                    BeatInstance bi = new BeatInstance(m.Value, m.Index);
                    if (bi.Index != _beatList.Count)
                        throw new Exception("Nonsequential index");

                    _beatList.Add(bi);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("error parsing file @" + current ?? "", ex);
            }
        }

        public List<BeatInstance> GetAllBeats()
        {
            return _beatList;
        }

        public BeatInstance GetBeatByIndex(int index)
        {
            return _beatList[index];
        }

        public string GetWavFilename()
        {
            if (_fileName.IndexOf(".txt") >= 0)
                return _fileName.Replace(".trackdata.txt", ".wav");
            else
                return "";
        }

        public void Update()
        {
            try
            {
                StringBuilder sb = new StringBuilder(_fileText);

                foreach (BeatInstance bi in _beatList)
                {
                    Regex reg = new Regex("_energyLevel\\\\\":[0-9]");
                    Match m = reg.Match(_fileText, bi.FileOffset);

                    int offset = m.Index + m.Length - 1;

                    sb[offset] = bi.EnergyLevel.ToString()[0];

//                    Console.Write("{0} --> ", _fileText[offset]);
//                    Console.WriteLine("{0}", sb[offset]);
                }

                _fileText = sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("error updating file", ex);
            }
        }

        public BeatAnalysis Analyze()
        {
            try
            {
                BeatAnalysis ba = new BeatAnalysis();
                ba.Analyze(_beatList);

                return ba;
            }
            catch (Exception ex)
            {
                throw new Exception("error analyzing file", ex);
            }
        }

        public void Backup()
        {
            // TODO:  fix so you can back up files that are not in current folder
            if (_fileName.IndexOf('\\') >= 0)
                throw new Exception("sorry can only back up files that are in current folder");

            const string dir = "BEAT-backup";
            try
            {
                System.IO.Directory.CreateDirectory(dir);
                File.Copy(_fileName, String.Format("{0}\\[BACKUP-{1}]{2}", dir, DateTime.Now.Ticks, _fileName));
            }
            catch (Exception ex)
            {
                throw new Exception("error backing up file", ex);
            }
        }

        public void Write()
        {
            try
            {
                File.WriteAllText(_fileName, _fileText);
            }
            catch (Exception ex)
            {
                throw new Exception("error writing file", ex);
            }
        }
    }
}
