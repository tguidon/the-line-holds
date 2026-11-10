"""Original seamless Normandy afternoon soundscape. No recordings or third-party samples."""
from pathlib import Path
import math, random, wave, array
root=Path(__file__).resolve().parents[2]
rate=32000;duration=48;n=rate*duration
rng=random.Random(1944);left=array.array('f',[0])*n;right=array.array('f',[0])*n
# Soft, slowly changing wind through hedges, with high-frequency leaf rustle.
slow=fast=0.
for i in range(n):
 t=i/rate;noise=rng.uniform(-1,1);slow+=.008*(noise-slow);fast+=.12*(noise-fast)
 breeze=.55+.22*math.sin(2*math.pi*t/24)+.12*math.sin(2*math.pi*t/16)
 value=(slow*.38+fast*.032)*breeze
 left[i]=value;right[i]=value*(.92+.08*math.sin(2*math.pi*t/48))
# Sparse paired birds, varied phrases with rests; panned across the hedgerows.
for phrase in range(17):
 start=rng.uniform(1,duration-2);pan=rng.uniform(.1,.9)
 for syllable in range(rng.randint(2,4)):
  at=start+syllable*rng.uniform(.16,.24);length=rng.uniform(.08,.16);freq=rng.uniform(1800,3100);phase=0
  for j in range(int(length*rate)):
   k=int(at*rate)+j
   if k>=n:break
   t=j/rate;u=t/length;phase+=2*math.pi*(freq+650*math.sin(u*math.pi)*math.sin(u*5))/rate
   v=math.sin(phase)*math.sin(math.pi*u)**2*.022
   left[k]+=v*(1-pan);right[k]+=v*pan
# Periodic construction above plus a 0.5s equal-power join makes the loop quiet at its seam.
path=root/'TheLineHolds/Assets/TheLineHolds/Audio/Countryside.wav';data=array.array('h')
for i in range(n):
 fade=min(1,i/(rate*.5),(n-1-i)/(rate*.5))
 for channel in (left,right):data.append(int(max(-1,min(1,channel[i]*fade))*32767))
with wave.open(str(path),'wb') as w:w.setnchannels(2);w.setsampwidth(2);w.setframerate(rate);w.writeframes(data.tobytes())
print(path)
