using UnityEngine;

namespace SecondSpawn.AI
{
    [DisallowMultipleComponent]
    public sealed class PrototypeVoiceCue : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _volume = 0.12f;
        [SerializeField] private float _secondsPerCharacter = 0.025f;
        [SerializeField] private float _minSeconds = 0.18f;
        [SerializeField] private float _maxSeconds = 1.4f;

        private AudioSource _audioSource;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = gameObject.AddComponent<AudioSource>();
                _audioSource.playOnAwake = false;
                _audioSource.spatialBlend = 0.65f;
            }
        }

        public void PlayCue(string text)
        {
            if (_audioSource == null)
            {
                return;
            }

            var duration = Mathf.Clamp((text?.Length ?? 8) * _secondsPerCharacter, _minSeconds, _maxSeconds);
            var clip = BuildCue(duration);
            _audioSource.Stop();
            _audioSource.PlayOneShot(clip, _volume);
        }

        private static AudioClip BuildCue(float duration)
        {
            const int sampleRate = 22050;
            var sampleCount = Mathf.Max(1, Mathf.CeilToInt(sampleRate * duration));
            var samples = new float[sampleCount];

            for (var i = 0; i < sampleCount; i++)
            {
                var t = i / (float)sampleRate;
                var envelope = Mathf.Sin(Mathf.PI * i / sampleCount);
                var pitch = 260f + 60f * Mathf.Sin(t * 18f);
                samples[i] = Mathf.Sin(t * pitch * Mathf.PI * 2f) * envelope * 0.35f;
            }

            var clip = AudioClip.Create("PrototypeVoiceCue", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
