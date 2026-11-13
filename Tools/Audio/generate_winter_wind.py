"""Original low winter wind for the forest. No field recordings or external samples."""
from array import array
import math
from pathlib import Path
import random
import wave

rate, seconds = 24000, 24
rng = random.Random(1224)
samples = array('h')
low = air = 0.
for i in range(rate*seconds):
    t = i/rate
    pulse = .58 + .24*math.sin(math.tau*t/12) + .16*math.sin(math.tau*t/8+.8)
    noise = rng.uniform(-1,1)
    low += .012*(noise-low)
    air += .14*(noise-air)
    breath = low*.61 + air*.048
    fade = min(1,t/.5,(seconds-t)/.5)
    samples.append(round(32767*breath*pulse*fade))

path = Path(__file__).resolve().parents[2]/'TheLineHolds/Assets/TheLineHolds/Audio/WinterWind.wav'
with wave.open(str(path),'wb') as output:
    output.setparams((1,2,rate,0,'NONE','not compressed'))
    output.writeframes(samples.tobytes())
