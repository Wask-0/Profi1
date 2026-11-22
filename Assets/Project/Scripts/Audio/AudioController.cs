using Labyrinthino.Data;
using UnityEngine;

namespace Labyrinthino.Audio
{
    /// <summary>
    /// Sound effects of the game. The numeric value is the index in the clip array.
    /// </summary>
    public enum SoundId
    {
        /// <summary>Button press.</summary>
        Click = 0,

        /// <summary>Tap on a board cell.</summary>
        Tap = 1,

        /// <summary>Correct answer or solved level.</summary>
        Correct = 2,

        /// <summary>Wrong answer or forbidden move.</summary>
        Error = 3,

        /// <summary>Victory jingle.</summary>
        Victory = 4,

        /// <summary>Defeat jingle.</summary>
        Defeat = 5,

        /// <summary>Step along a path.</summary>
        Step = 6
    }

    /// <summary>
    /// Plays background music and sound effects with independent volumes.
    /// </summary>
    public sealed class AudioController : MonoBehaviour
    {
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _soundSource;
        [SerializeField] private AudioClip _musicClip;
        [SerializeField] private AudioClip[] _soundClips;

        /// <summary>
        /// Applies saved volumes and starts the music loop.
        /// </summary>
        /// <param name="data">Save data that stores the volumes.</param>
        public void Initialise(SaveData data)
        {
            SetMusicVolume(data.MusicVolume);
            SetSoundVolume(data.SoundVolume);
            _musicSource.clip = _musicClip;
            _musicSource.loop = true;
            _musicSource.Play();
        }

        /// <summary>
        /// Changes the music volume.
        /// </summary>
        /// <param name="volume">Volume from 0 to 1.</param>
        public void SetMusicVolume(float volume)
        {
            _musicSource.volume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// Changes the sound effects volume.
        /// </summary>
        /// <param name="volume">Volume from 0 to 1.</param>
        public void SetSoundVolume(float volume)
        {
            _soundSource.volume = Mathf.Clamp01(volume);
        }

        /// <summary>
        /// Plays a sound effect once.
        /// </summary>
        /// <param name="sound">Effect to play.</param>
        public void Play(SoundId sound)
        {
            int index = (int)sound;
            if (index < _soundClips.Length && _soundClips[index] != null)
            {
                _soundSource.PlayOneShot(_soundClips[index]);
            }
        }
    }
}
