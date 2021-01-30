using System;
using System.IO;
using System.Collections.Generic;
using BeatFile;
using NAudio;

namespace BeatEdit
{
    class BeatEdit
    {
        private string _fileName = null;
        private BeatFile.BeatFile _beatFile = null;

        private int _indexStart=-1;
        private int _indexEnd=-1;
        private int _indexDirection=0;
        private bool _indexSet = false;

        private bool _doEdit = false;
        private int _editDest = -1;
        private int _editSrc = -1;
        private int _editLimit = 9999;
        private int _editMult = 1;

        private bool _doViewBeats = false;
        private bool _doViewAnalysis = false;
        private bool _doViewEdits = false;

        private bool _doWrite = false;
        private bool _doBackup = false;
        private bool _doPlay = false;
        private bool _playQuiet = false;

        private bool _run = true;

        public BeatEdit(string[] args)
        {
            string current = "";

            try
            {
                if (args.Length == 0)                                       // if no params, assume help
                    args = new string[] { "-h" };

                foreach (string s in args)                                  // check for help first
                {
                    if (s == "-h" || s == "-?")
                    {
                        Console.WriteLine("BEATEDIT v0.1 - reddit.com/u/glonq");
                        Console.WriteLine("Syntax:  BEATEDIT filename.txt -param -param -param");
                        Console.WriteLine("...where 'param' is:");
                        Console.WriteLine("   -h        help");
                        Console.WriteLine("   -h*       show all help");
                        Console.WriteLine("   -h!       show examples");
                        Console.WriteLine("   -hX       help and examples for command X");
                        Console.WriteLine("   -eX       edit beat; set EnergyLevel to X");
                        Console.WriteLine("   -eX,Y     edit beat; set EnergyLevel to X but only if current value is Y");
                        Console.WriteLine("   -iXXX     edit one beat at index XXX");
                        Console.WriteLine("   -iXXX,YYY edit all beats between index XXX and YYY (inclusive)");
                        Console.WriteLine("   -mN       only edit beats that are a multiple of N");
                        Console.WriteLine("   -lN       limit; stop after N beats are changed (ignores unchanged beats)");
                        Console.WriteLine("   -v        view everything");
                        Console.WriteLine("   -v[a|b|e] view analysis; view beats; view edits");
                        Console.WriteLine("   -w -wb    write; write plus backup");
                        Console.WriteLine("   -p -pq    play track; play quiet (don't display beats)");
                        Console.WriteLine("   -f        find all files and show their info");

                        _run = false;
                        return;
                    }
                    else if (s == "-f")
                    {
                        string[] files = Directory.GetFiles(".", "*.trackdata.txt");

                        _beatFile = new BeatFile.BeatFile();
                        foreach (string fn in files)
                        {
                            Console.WriteLine("{0}", fn.Substring(2));
                            _beatFile.Read(fn);
                            _beatFile.Parse();
                            Console.WriteLine("{0} - {1}", _beatFile.Artist, _beatFile.Track);
                            Console.WriteLine();
                        }

                        _run = false;
                        return;
                    }
                }

                _fileName = args[0];

                foreach (string s in args)                                  // check for help first
                {
                    current = s;
                    if (s[0] == '-')
                        switch (s[1])
                        {
                            case 'h': ParseHelpParam(s); _run = false; return;
                            case 'e': ParseEditParam(s); break;
                            case 'i': ParseIndexParam(s); break;
                            case 'm': ParseMultParam(s); break;
                            case 'l': ParseLimitParam(s); break;
                            case 'v': ParseViewParam(s); break;
                            case 'w': ParseWriteParam(s); break;
                            case 'p': ParsePlayParam(s); break;
                        }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("error parsing parameter: " + current, ex);
            }
        }

        private void ParsePlayParam(string s)
        {
            // -p or -pq
            
            _doPlay = true;

            if (s.Length > 2 && s[2] == 'q')
                _playQuiet = true;
        }

        private void ParseViewParam(string s)
        {
            // -v or -v[b][e][a]

            if (s.Length <= 2)                  //-v
            {
                _doViewBeats = true;
                _doViewAnalysis = true;
                _doViewEdits = true;
            }
            else
            {
                foreach (char c in s.Substring(2))
                    if (c == 'b')
                        _doViewBeats = true;
                    else if (c == 'a')
                        _doViewAnalysis = true;
                    else if (c == 'e')
                        _doViewEdits = true;
            }
        }

        private void ParseWriteParam(string s)
        {
            // -w or -wb

            _doWrite = true;

            if (s.Length > 2 && s[2] == 'b')
            {
                _doBackup = true;

                if (_fileName.IndexOf('\\') >= 0)
                    throw new Exception("sorry can only back up files that are in current folder");
            }
        }

        private void ParseLimitParam(string s)
        {
            // -lNNN

            if (s.Length > 2)
                _editLimit = Convert.ToInt32(s.Substring(2));
        }

        private void ParseMultParam(string s)
        {
            // -mNNN

            if (s.Length > 2)
                _editMult = Convert.ToInt32(s.Substring(2));
        }

        private void ParseIndexParam(string s)
        {
            // -iNNN or -iXXX,YYY

            int i = s.IndexOf(',');
            if (i > 0)
            {
                _indexStart = Convert.ToInt32(s.Substring(2, i-2));
                _indexEnd = Convert.ToInt32(s.Substring(i + 1));
                _indexDirection = (_indexStart <= _indexEnd) ? 1 : -1;
            }
            else
            {
                _indexStart = _indexEnd = Convert.ToInt32(s.Substring(2));
                _indexDirection = 0;
            }

            _indexSet = true;
        }

        private void ParseEditParam(string s)
        {
            // -eX or -eX,Y

            int i = s.IndexOf(',');
            if (i > 0)
            {
                _editDest = Convert.ToInt32(s.Substring(2, i-2));
                _editSrc = Convert.ToInt32(s.Substring(i + 1));
            }
            else
            {
                _editDest = Convert.ToInt32(s.Substring(2));
                _editSrc = -1;
            }

            _doEdit = true;
        }

        private void ParseHelpParam(string s)
        {
            // -hX

            Console.WriteLine();
            if (s.Length > 2)
            {
                switch(s[2])
                {
                    case '!':
                        Console.WriteLine("Examples:");
                        Console.WriteLine("beatview filename.txt -v");
                        Console.WriteLine("   view details (beats and analysis)");
                        Console.WriteLine();
                        Console.WriteLine("beatview filename.txt -e2 -i0");
                        Console.WriteLine("   set the first (index=0) beat to engery level 2");
                        Console.WriteLine();
                        Console.WriteLine("beatview filename.txt -e2,0 -i9999");
                        Console.WriteLine("   sets the last (becuase index is 'rounded down') beat to engery level 2,");
                        Console.WriteLine("   *but only if it's currently 0*");
                        Console.WriteLine();
                        Console.WriteLine("beatview filename.txt -e3 -i0,9999");
                        Console.WriteLine("   sets every beat to engery level 3,");
                        Console.WriteLine();
                        Console.WriteLine("beatview filename.txt -e3 -i0,9999 -m2");
                        Console.WriteLine("   sets every *second* beat to engery level 3");
                        Console.WriteLine();
                        Console.WriteLine("beatview filename.txt -e1 -i9999,0 -l4");
                        Console.WriteLine("   sets the last 4 beats to engery level 1");
                        Console.WriteLine("   (because -i9999,0 means to start at the end and work backwards)");
                        Console.WriteLine("   (and -l4 means to stop after a limit of 4 beats have been changed");
                        Console.WriteLine();
                        Console.WriteLine("Remember");
                        Console.WriteLine("   use -ve if you want to see details about which beats got changed");
                        Console.WriteLine("   use -w or -wb to write your edits back to the original file");
                        Console.WriteLine("     (otherwise edits are just 'for testing' and discarded)");
                        Console.WriteLine("   use -p to play the track ");
                        break;

                    case 'e':
                        Console.WriteLine("The -e parameter says what type of edit to do");
                        Console.WriteLine("   -eX says that you want to set beat(s) to engergy level X");
                        Console.WriteLine("   -eX,Y only changes beats to X if their current energy level is Y");
                        Console.WriteLine("   remember, you also need to specify *what beats to edit* using -i");
                        Console.WriteLine("   Energy levels are:");
                        Console.WriteLine("     0 - any event, or no event?");
                        Console.WriteLine("     1 - same?");
                        Console.WriteLine("     2 - any punch; no duck or squat or punch");
                        Console.WriteLine("     3 - any event");
                        Console.WriteLine("     4+- no event");
                        Console.WriteLine("   if you aren't sure what your edit will do, use -ve to tell you");
                        break;

                    case 'i':
                        Console.WriteLine("The -i parameter gives the index (number) for what beats to change");
                        Console.WriteLine("   -iXXX edits (only) beat XXX");
                        Console.WriteLine("   -iXXX,YYY edits all the beats between (inclusive) XXX and YYY");
                        Console.WriteLine("   if Y > X, then we edit the beats backwards; counting down from Y to X");
                        Console.WriteLine("   if X or Y are too high (more than the # of beats), then we round it down");
                        Console.WriteLine("   ...so if you want to hit the end of the song, just use 9999");
                        Console.WriteLine("   indexes start at zero, so use -i0 to edit the first beat");
                        Console.WriteLine("   remember, you also need to specify *what energy level to set* using -e");
                        Console.WriteLine("   if you aren't sure what your edit will do, use -ve to tell you");
                        break;

                    case 'l':
                        Console.WriteLine("The -l parameter sets the limit for how many beats to change");
                        Console.WriteLine("   -lX means to stop changing when more than X beats have changed");
                        break;

                    case 'm':
                        Console.WriteLine("The -m parameter lets you change only *some* beats in a range");
                        Console.WriteLine("   -lX means to only change a beat if it's a multipe of X");
                        Console.WriteLine("   -l1 changes all beats; -l2 changes half; -l3 changes a third...");
                        break;

                    case 'v':
                        Console.WriteLine("The -v parameter views details");
                        Console.WriteLine("   -vb views all the beats in the track");
                        Console.WriteLine("   -va views an analysis of the track's energy levels, magnitudes, and BPM");
                        Console.WriteLine("   -ve shows every beat that your edits will change");
                        Console.WriteLine("   -v  shows everything");
                        Console.WriteLine("   remember, you can combine options, like -vba or -vae");
                        Console.WriteLine("   btw for -vb, the length of the number bar indicates its magnitude");
                        break;

                    case 'w':
                        Console.WriteLine("The -w parameter  writes your changes back into the original file");
                        Console.WriteLine("   -wb also saves a timestamped backup into a folder named BEAT-backup");
                        Console.WriteLine("   remember *if you don't put -w then your edits won't actually be written!*");
                        break;

                    case 'f':
                        Console.WriteLine("The -f parameter scans all files and shows their filename + track & artist info");
                        break;

                    case 'p':
                        Console.WriteLine("The -p parameter plays the track using your default audio output");
                        Console.WriteLine("   use SPACE to toggle pause/play, and ESC to stop");
                        Console.WriteLine("   -pq plays 'quiet' - doesn't show beats on the screen");
                        Console.WriteLine("   btw, the length of the number bar indicates its magnitude");
                        break;

                    case '*':
                        ParseHelpParam("-he");
                        ParseHelpParam("-hi");
                        ParseHelpParam("-hm");
                        ParseHelpParam("-hl");
                        ParseHelpParam("-hw");
                        ParseHelpParam("-hv");
                        ParseHelpParam("-hp");
                        ParseHelpParam("-hf");
                        ParseHelpParam("-h!");
                        break;

                    default:
                        Console.WriteLine("sorry no help for that command");
                        break;
                }
            }
        }

        private void ViewOneBeat(BeatInstance bi)
        {
            Console.Write("{0,3}: {1:F3}s ", bi.Index, bi.TriggerTime);
            int mag = Convert.ToInt32(bi.Magnitude * 10);
            for (int i = 0; i <= mag; i++)
                Console.Write("{0}", bi.EnergyLevel);

            Console.WriteLine();
        }

        private void DoViewBeats()
        {
            try
            {
                foreach (BeatInstance bi in _beatFile.GetAllBeats())
                    ViewOneBeat(bi);
            }
            catch (Exception ex)
            {
                throw new Exception("error viewing file", ex);
            }
        }

        private void DoEditBeats()
        {
            Console.WriteLine();

            try
            {
                List<BeatInstance> bl = _beatFile.GetAllBeats();

                // check params

                if (!_indexSet)
                    throw new Exception("no index set. use -i param");

                if (bl.Count <= 0)
                    throw new Exception("no beats found");

                // ensure indexes within ranges

                if (_indexDirection == 0)
                {
                    if (_indexStart < 0 || _indexStart >= bl.Count)
                        throw new Exception("index out of range");
                }
                else // _indexStart == -1 or +1
                {
                    _indexStart = Math.Min(bl.Count - 1, Math.Max(_indexStart, 0));
                    _indexEnd = Math.Min(bl.Count - 1, Math.Max(_indexEnd, 0));

                    if (_indexStart == _indexEnd)
                        _indexDirection = 0;
                }

                // display params

                Console.WriteLine("edit beats between index {0} to {1}", _indexStart, _indexEnd);
                if (_editSrc >= 0)
                    Console.Write("...replace {0} with {1}", _editSrc, _editDest);
                else
                    Console.Write("...set to {0}", _editDest);

                if (_editLimit < bl.Count)
                    Console.Write(", limit {0} changes", _editLimit);

                if (_editMult > 1)
                    Console.Write(", only multiples of {0}", _editMult);

                Console.WriteLine();

                // do edit

                int changed = 0;
                int index = _indexStart;

                bool done = false;
                while (!done)
                {
                    if (index == _indexEnd)
                        done = true;

                    bool edit = true;

                    if (index % _editMult != 0)
                        edit = false;
                    if (_editSrc >= 0 && bl[index].EnergyLevel != _editSrc)
                        edit = false;

                    if (edit)
                    {
                        if (_doViewEdits)
                            Console.WriteLine("changing beat {0} from {1} to {2}", index, bl[index].EnergyLevel, _editDest);

                        bl[index].EnergyLevel = _editDest;
                        changed++;
                    }

                    index += _indexDirection;

                    if (changed >= _editLimit)
                        done = true;
                }

                Console.WriteLine("changed {0} beats", changed);
            }
            catch (Exception ex)
            {
                throw new Exception("error editing beats", ex);
            }
        }

        public void DoViewAnalysis()
        {
            try
            {
                BeatAnalysis ba = new BeatAnalysis();
                ba.Analyze(_beatFile.GetAllBeats());
                ba.Show();
            }
            catch (Exception ex)
            {
                throw new Exception("error analyzing file", ex);
            }
        }

        public void DoPlay()
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("playing track. press space to play/pause; ESC to stop");
                System.Threading.Thread.Sleep(1000);

                NAudio.Wave.WaveOutEvent wavout = new NAudio.Wave.WaveOutEvent();
                NAudio.Wave.WaveFileReader wavreader = new NAudio.Wave.WaveFileReader(_beatFile.GetWavFilename());

                wavout.Init(wavreader);
                wavout.Play();

                int index = 0;
                List<BeatInstance> bl = _beatFile.GetAllBeats();

                while (wavout.PlaybackState != NAudio.Wave.PlaybackState.Stopped)
                {
                    if (System.Console.KeyAvailable)
                    {
                        switch (System.Console.ReadKey(true).Key)
                        {
                            case ConsoleKey.Spacebar :
                                if (wavout.PlaybackState == NAudio.Wave.PlaybackState.Paused)
                                    wavout.Play();
                                else if (wavout.PlaybackState == NAudio.Wave.PlaybackState.Playing)
                                    wavout.Pause();
                                break;

                            case ConsoleKey.Escape :
                                Console.WriteLine("stopped playback");
                                wavout.Stop();
                                break;

                            case ConsoleKey.OemPeriod:
                                Console.WriteLine("{0}", wavreader.CurrentTime.TotalSeconds);
                                break;
                        }
                    }

                    while (index < bl.Count && bl[index].TriggerTime <= wavreader.CurrentTime.TotalSeconds)
                    {
                        if (!_playQuiet)
                            ViewOneBeat(bl[index]);

                        index++;
                    }

                    System.Threading.Thread.Sleep(10);
                }

                wavout.Dispose();
                wavreader.Dispose();

            }
            catch (Exception ex)
            {
                throw new Exception("error playing track", ex);
            }
        }

        public void Run()
        {
            if (!_run || _fileName == null)
                return;

            _beatFile = new BeatFile.BeatFile();
            _beatFile.Read(_fileName);
            _beatFile.Parse();
            Console.WriteLine("{0} - {1}", _beatFile.Artist, _beatFile.Track);
            Console.WriteLine("found {0} beats", _beatFile.GetAllBeats().Count);

            if (_doEdit)
                DoEditBeats();

            if (_doViewBeats)
                DoViewBeats();

            if (_doViewAnalysis)
                DoViewAnalysis();

            if (_doBackup)
            {
                Console.WriteLine();
                Console.WriteLine("backing up...");
                _beatFile.Backup();
            }

            if (_doWrite)
            {
                Console.WriteLine();
                Console.WriteLine("writing...");
                _beatFile.Update();
                _beatFile.Write();
            }

            if (_doPlay)
                DoPlay();

            Console.WriteLine();
            Console.WriteLine(":)");
        }
    }

    class BeatEditProgram
    {
        static void Main(string[] args)
        {
//            args = new string[] { "f2201bb27a2c23d2fcad24718d1b30ba.trackdata.txt", "-e0,8", "-i999,0", "-l999", "-p"};
            args = new string[] { "f2201bb27a2c23d2fcad24718d1b30ba.trackdata.txt", "-h" };

            try
            {
                BeatEdit be = new BeatEdit(args);
                be.Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine("!!!");
                Console.WriteLine("{0}", ex);
                Console.WriteLine("{0}", ex.InnerException);
                Console.WriteLine(":(");
            }
        }
    }
}
