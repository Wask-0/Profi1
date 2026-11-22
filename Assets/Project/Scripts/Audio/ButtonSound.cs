using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Labyrinthino.Audio
{
    /// <summary>
    /// Plays the click sound whenever the attached button is pressed. It does not rely on
    /// button listeners, so it keeps working when screens rebind their click handlers.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public sealed class ButtonSound : MonoBehaviour, IPointerClickHandler
    {
        private static AudioController _audio;

        /// <summary>
        /// Registers the audio controller used by every button sound.
        /// </summary>
        /// <param name="audio">Controller that plays the clicks.</param>
        public static void SetAudio(AudioController audio)
        {
            _audio = audio;
        }

        /// <inheritdoc />
        public void OnPointerClick(PointerEventData eventData)
        {
            Button button = GetComponent<Button>();
            if (_audio != null && button.IsInteractable())
            {
                _audio.Play(SoundId.Click);
            }
        }
    }
}
