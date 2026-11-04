using System.Collections.Generic;
using UnityEngine;
namespace TheLineHolds
{
    public sealed class BattleAudio : MonoBehaviour
    {
        public AudioClip placement, firing, impact, escape, wave, victory, defeat;
        AudioSource source;
        float nextFire, nextImpact;
        void Awake() { source = gameObject.AddComponent<AudioSource>(); source.spatialBlend = 0; source.volume = .35f; }
        public void Play(string cue)
        {
            AudioClip clip = null;
            switch(cue)
            {
                case "place": clip = placement; break;
                case "fire": if(Time.unscaledTime < nextFire) return; nextFire=Time.unscaledTime+.06f; clip=firing; break;
                case "impact": if(Time.unscaledTime < nextImpact) return; nextImpact=Time.unscaledTime+.1f; clip=impact; break;
                case "escape": clip=escape; break;
                case "wave": clip=wave; break;
                case "victory": clip=victory; break;
                case "defeat": clip=defeat; break;
            }
            if(clip) source.PlayOneShot(clip);
        }
        public void SetPaused(bool paused) { if(paused) source.Pause(); else source.UnPause(); }
        public void ResetAudio() { source.Stop(); nextFire=nextImpact=0; }
    }
}
