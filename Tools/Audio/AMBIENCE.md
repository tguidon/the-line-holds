# Countryside ambience

`generate_ambience.py` creates the original 48-second stereo afternoon bed used by `BattleAudio`: filtered wind, leaf rustle, sparse panned bird phrases and quiet loop boundaries. It uses no recordings or third-party samples. Run it from any directory with Python 3; the seed is 1944.

Unity imports the WAV as Vorbis in memory. The dedicated ambience gain fades, ducks during combat/pause, and can be muted independently from weapon sounds with the speaker control. This retains the project's CLI-only gain layer; no AudioMixer or package was added.
