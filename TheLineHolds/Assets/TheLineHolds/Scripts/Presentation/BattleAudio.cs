using UnityEngine;
namespace TheLineHolds
{
    public sealed class BattleAudio : MonoBehaviour
    {
        public AudioClip placement, escape, wave, victory, defeat, countryside;
        [Range(0,1)] public float ambienceGain=.7f;
        public bool Muted { get; private set; }
        AudioSource ambience;
        public void ToggleMute()
        {
            Muted = !Muted;
            ambience.mute = ui.mute = Muted;
            foreach (var voice in voices) voice.mute = Muted;
        }
        public AudioClip[] machineGun, cannon, mortarLaunch, softHit, armorHit, shellImpact, mortarImpact;
        [Range(0, 1)] public float sfxGain = .65f;
        const int VoiceLimit = 24;
        readonly AudioSource[] voices = new AudioSource[VoiceLimit];
        readonly float[] levels = new float[VoiceLimit];
        readonly int[] priorities = new int[VoiceLimit];
        readonly float[] nextCue = new float[8];
        readonly int[] sequence = new int[8];
        AudioSource ui;
        Camera view;
        bool paused;
        public int ActiveWorldVoices { get { int count = 0; foreach (var voice in voices) if (voice && voice.isPlaying) count++; return count; } }
        void Awake()
        {
            view = Camera.main;
            ambience=gameObject.AddComponent<AudioSource>();ambience.playOnAwake=false;ambience.loop=true;ambience.spatialBlend=0;ambience.priority=220;
            ui = gameObject.AddComponent<AudioSource>(); ui.playOnAwake = false; ui.spatialBlend = 0;
            for (int i = 0; i < VoiceLimit; i++)
            {
                var child = new GameObject("Combat audio voice " + i); child.transform.SetParent(transform, false);
                voices[i] = child.AddComponent<AudioSource>(); voices[i].playOnAwake = false;
                voices[i].spatialBlend = 0; voices[i].dopplerLevel = 0;
            }
        }
        void LateUpdate()
        {
            if(countryside && ambience.clip!=countryside){ambience.clip=countryside;ambience.Play();}
            var game=GetComponent<BattleController>();float target=ambienceGain*(paused?.35f:game && game.Match!=null && game.Match.Phase==MatchPhase.Combat?.65f:1);
            ambience.volume=Mathf.MoveTowards(ambience.volume,target,Time.unscaledDeltaTime*.45f);
            // Headless gain layer: no AudioMixer asset or UI authoring is required.
            ui.volume = .35f * sfxGain;
            for (int i = 0; i < VoiceLimit; i++) voices[i].volume = levels[i] * sfxGain;
        }
        public void Play(string cue)
        {
            AudioClip clip = cue == "place" ? placement : cue == "escape" ? escape : cue == "wave" ? wave : cue == "victory" ? victory : cue == "defeat" ? defeat : null;
            if (clip) { ui.volume = .35f * sfxGain; ui.PlayOneShot(clip); }
        }
        public void VehicleDestroyed(Vector3 p) => World(mortarImpact,7,p,.3f,3,.08f);
        public void Fire(DefenseDefinition definition, Vector3 position)
        {
            var style = WeaponStyles.For(definition);
            if (style == WeaponStyle.MachineGun) World(machineGun, 0, position, .23f, 1, .025f);
            else if (style == WeaponStyle.Cannon) World(cannon, 1, position, .42f, 3, .015f);
            else World(mortarLaunch, 2, position, .34f, 2, .015f);
        }
        public void Impact(ShotState shot)
        {
            var style = WeaponStyles.For(shot.Definition);
            bool armored = !shot.Target.Resolved && shot.Target.Definition.armored;
            if (style == WeaponStyle.Mortar) World(mortarImpact, 6, shot.Destination, .4f, 3, .045f);
            else if (armored) World(armorHit, 4, shot.Destination, style == WeaponStyle.Cannon ? .35f : .17f, 2, .045f);
            else if (style == WeaponStyle.Cannon) World(shellImpact, 5, shot.Destination, .34f, 2, .04f);
            else World(softHit, 3, shot.Destination, .12f, 0, .045f);
        }
        void World(AudioClip[] clips, int family, Vector3 p, float level, int priority, float spacing)
        {
            if (paused || clips == null || clips.Length == 0 || Time.unscaledTime < nextCue[family]) return;
            int slot = -1;
            for (int i = 0; i < VoiceLimit; i++) if (!voices[i].isPlaying) { slot = i; break; }
            if (slot < 0)
                for (int i = 0; i < VoiceLimit; i++) if (priorities[i] < priority && (slot < 0 || priorities[i] < priorities[slot])) slot = i;
            if (slot < 0) return;
            nextCue[family] = Time.unscaledTime + spacing;
            int take = sequence[family]++;
            var source = voices[slot]; source.Stop(); source.clip = clips[take % clips.Length];
            source.pitch = .97f + (take % 5) * .015f;
            // A diorama mix: camera-relative stereo without distance loss from the elevated camera.
            if (!view) view = Camera.main;
            source.panStereo = view ? Mathf.Clamp((view.WorldToViewportPoint(p).x - .5f) * 1.2f, -.65f, .65f) : 0;
            source.transform.position = p; priorities[slot] = priority; levels[slot] = level;
            source.volume = level * sfxGain; source.priority = 160 - priority * 30; source.Play();
        }
        public void SetPaused(bool value)
        {
            paused = value;
            if (paused) { foreach (var voice in voices) voice.Pause(); }
            else { foreach (var voice in voices) voice.UnPause(); }
        }
        public void ResetAudio()
        {
            ui.Stop(); foreach (var voice in voices) voice.Stop(); paused = false;
            System.Array.Clear(nextCue, 0, nextCue.Length);
        }
    }
}
