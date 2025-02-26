using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;


using System.Collections.Generic;
namespace GameLab
{

    public class SoundManager(OptionsState state)
    {
        private readonly Dictionary<string, SoundEffect> _sounds = [];
        private readonly Dictionary<string, SoundEffectInstance> _instances = [];
        private readonly Dictionary<string, float> _volumes = [];
        private readonly OptionsState _state = state;
        public float GlobalVolume { get; set; } = 0.5f;
        public string CurrentlyPlaying { get; private set; }

        public void LoadContent(ContentManager content)
        {
            // Example sounds
            AddSound(content, "banana-death", "sound/banana-death", true);
            AddSound(content, "player-step", "sound/step-sound", true);
            AddSound(content, "player-kick", "sound/kick-sound", true);
            AddSound(content, "player-hit", "sound/player-hit", true);
            AddSound(content, "player-dodge", "sound/player-dodge", true);
            AddSound(content, "mushroom-attack", "sound/mushroom-attack", true);
            AddSound(content, "banana-attack", "sound/banana-attack", true);
            AddSound(content, "main-loop", "sound/main_theme_loop", false);
            AddSound(content, "menu-loop", "sound/menu-loop", false);
            AddSound(content, "game-over", "sound/game-over", false);
            AddSound(content, "player-slide", "sound/player-slide", true);
        }

        private void AddSound(ContentManager content, string name, string path, bool isEffect)
        {
            SoundEffect sound = content.Load<SoundEffect>(path);
            _sounds[name] = sound;
            _instances[name] = sound.CreateInstance();
            _volumes[name] = isEffect ? _state.GetEffectsVolume() / 100f : _state.GetMusicVolume() / 100f;
        }

        public void PlaySound(string name, bool loop = false)
        {
            if (_instances.TryGetValue(name, out SoundEffectInstance instance))
            {
                if (instance.State != SoundState.Playing)
                {
                    instance.Volume = _volumes[name];
                    instance.IsLooped = loop;
                    instance.Play();
                    CurrentlyPlaying = name;
                }
            }
        }

        public void StopSound(string name)
        {
            if (_instances.TryGetValue(name, out SoundEffectInstance instance))
            {
                instance.Stop();
                if (CurrentlyPlaying == name)
                {
                    CurrentlyPlaying = null;
                }
            }
        }

        public void SetVolume(string name, float volume)
        {
            if (_instances.TryGetValue(name, out SoundEffectInstance instance))
            {
                instance.Volume = volume;
                _volumes[name] = volume;
            }
        }

        public void StopAllSounds()
        {
            foreach (SoundEffectInstance instance in _instances.Values)
            {
                instance.Stop();
            }
        }

        public void Dispose()
        {
            foreach (SoundEffectInstance instance in _instances.Values)
            {
                instance.Dispose();
            }
            _instances.Clear();
            _sounds.Clear();
            _volumes.Clear();
        }
        public void PauseSound(string name)
        {
            if (_instances.TryGetValue(name, out SoundEffectInstance instance))
            {
                if (instance.State == SoundState.Playing)
                {
                    instance.Pause();
                }
            }
        }

        public void ResumeSound(string name)
        {
            if (_instances.TryGetValue(name, out SoundEffectInstance instance))
            {
                if (instance.State == SoundState.Paused)
                {
                    instance.Resume();
                }
            }
        }
    }
}