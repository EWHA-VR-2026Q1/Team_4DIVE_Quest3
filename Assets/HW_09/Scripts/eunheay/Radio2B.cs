using UnityEngine;

namespace HW09
{
    public class Radio2B : MonoBehaviour, Interactable2B
    {
        public AudioClip hackerClip;
        public AudioSource radioSource;

        private Manager2B manager;
        private bool used = false;

        void Start()
        {
            manager = FindObjectOfType<Manager2B>();
        }

        public void Execute()
        {
            if (used) return;
            used = true;

            radioSource.clip = hackerClip;
            radioSource.Play();

            manager.OnRadioActivated(hackerClip.length);
        }

        public void OnGrab()
        {
            Execute();
        }
    }
}
