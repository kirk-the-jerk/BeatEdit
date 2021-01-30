# BeatEdit

This is a simple, quick BoxVR custom track editor that I wrote for fun,
for the PC/Steam version of BoxVR/FitXR.

It can:
- Display the beats in a BoxVR custom track file.
- Edit one/all/some beats in a BoxVR custom track file.

WARNING
=======
This is a "version 0.1" product.  I have put reasonable effort into it being robust and
reliable, but have done *minimal* testing on it.
So please manually back up your BoxVR files before using this!
I tested against the newest PC (Steam) version of BoxVR; IDK if this works with
older/newer/different versions.

Installation instructions
=========================
- There is no installer.
- Copy all the files to your AppData\LocalLow\FITXR\BoxVR\Playlists\TrackData folder
- ...and then run BEATEDIT from there

How to use BEATEDIT
===================
Syntax:  BEATEDIT filename.txt -param -param -param
...where 'param' is:
   -h        help
   
   -h*       show all help
   
   -h!       show examples
   
   -hX       help and examples for command X
   
   -eX       edit beat; set EnergyLevel to X
   
   -eX,Y     edit beat; set EnergyLevel to X but only if current value is Y
   
   -iXXX     edit one beat at index XXX
   
   -iXXX,YYY edit all beats between index XXX and YYY (inclusive)
   
   -mN       only edit beats that are a multiple of N
   
   -lN       limit; stop after N beats are changed (ignores unchanged beats)
   
   -v        view everything
   
   -v[a|b|e] view analysis; view beats; view edits
   
   -w -wb    write; write plus backup
   
   -p -pq    play track; play quiet (don't display beats)
   
   -f        find all files and show their info
   

The -e parameter says what type of edit to do

   -eX says that you want to set beat(s) to engergy level X
   -eX,Y only changes beats to X if their current energy level is Y
   remember, you also need to specify *what beats to edit* using -i
   Energy levels are:
     0 - any event, or no event?
     1 - same?
     2 - any punch; no duck or squat or punch
     3 - any event
     4+- no event
   if you aren't sure what your edit will do, use -ve to tell you

The -i parameter gives the index (number) for what beats to change

   -iXXX edits (only) beat XXX
   -iXXX,YYY edits all the beats between (inclusive) XXX and YYY
   if Y > X, then we edit the beats backwards; counting down from Y to X
   if X or Y are too high (more than the # of beats), then we round it down
   ...so if you want to hit the end of the song, just use 9999
   indexes start at zero, so use -i0 to edit the first beat
   remember, you also need to specify *what energy level to set* using -e
   if you aren't sure what your edit will do, use -ve to tell you

The -m parameter lets you change only *some* beats in a range

   -lX means to only change a beat if it's a multipe of X
   -l1 changes all beats; -l2 changes half; -l3 changes a third...

The -l parameter sets the limit for how many beats to change

   -lX means to stop changing when more than X beats have changed

The -w parameter  writes your changes back into the original file

   -wb also saves a timestamped backup into a folder named BEAT-backup
   remember *if you don't put -w then your edits won't actually be written!*

The -v parameter views details

   -vb views all the beats in the track
   -va views an analysis of the track's energy levels, magnitudes, and BPM
   -ve shows every beat that your edits will change
   -v  shows everything
   remember, you can combine options, like -vba or -vae
   btw for -vb, the length of the number bar indicates its magnitude

The -p parameter plays the track using your default audio output

   use SPACE to toggle pause/play, and ESC to stop
   -pq plays 'quiet' - doesn't show beats on the screen
   btw, the length of the number bar indicates its magnitude

The -f parameter scans all files and shows their filename + track & artist info

Examples:

beatview filename.txt -v

   view details (beats and analysis)

beatview filename.txt -e2 -i0

   set the first (index=0) beat to engery level 2

beatview filename.txt -e2,0 -i9999

   sets the last (becuase index is 'rounded down') beat to engery level 2,
   *but only if it's currently 0*

beatview filename.txt -e3 -i0,9999

   sets every beat to engery level 3,

beatview filename.txt -e3 -i0,9999 -m2

   sets every *second* beat to engery level 3

beatview filename.txt -e1 -i9999,0 -l4

   sets the last 4 beats to engery level 1
   (because -i9999,0 means to start at the end and work backwards)
   (and -l4 means to stop after a limit of 4 beats have been changed

Remember

   use -ve if you want to see details about which beats got changed
   use -w or -wb to write your edits back to the original file
     (otherwise edits are just 'for testing' and discarded)
   use -p to play the track

Technical overview
==================
OMG please don't use this as a resumé for my software development capabilities!
I retired from day-to-day software development 15 years ago, and now mange dev teams.
I wrote this thing in C# because that's the language that I was most productive and fluent in.
...but I fully expect a seasoned .Net developer to openly cringe and weep at my code, especially
   because I took a few shortcuts that even I know are "wrong".

Anyways...
- BEATFILE is the class library that manages the reading, parsing, and writing of the beat files.
- It is shared between two console application projects:
- BEATVIEW is the app that I first developed to test BEATFILE.  It is not useful for public consumption.
- BEATEDIT is the [reasonably] friendly and useful editor that you should use.
- I'm considering creating a BEATGUI editor, but since I don't know XAML it'll have to be a WinForms app (eek!)

Disclaimer, legal
==================
- This product is not associated with or endorsed by the fine folks at FitXR.
- If this managles your files or causes your computer to explode, then I'm sorry.  But also not legally responsible.
  BACKUP YOUR FILES!
- I don't care what you do with this; just don't financially profit from it.
- Mucho gratitude to u/goodguy5000hd for doing the ground work on figuring out BoxVR custom track files
