using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuMusic : MonoBehaviour
{
    public float volume = 0.18f;

    void Start()
    {
        AudioSource source = GetComponent<AudioSource>();
        source.clip = CreateMusicClip();
        source.loop = true;
        source.volume = volume;
        source.Play();
    }

    AudioClip CreateMusicClip()
    {
        int sampleRate = 44100;
        int durationSeconds = 8;
        int samples = sampleRate * durationSeconds;

        AudioClip clip = AudioClip.Create("MenuMusicGenerated", samples, 1, sampleRate, false);
        float[] data = new float[samples];

        float[] notes = { 261.63f, 329.63f, 392.00f, 523.25f, 392.00f, 329.63f };

        for (int i = 0; i < samples; i++)
        {
            float time = (float)i / sampleRate;
            int noteIndex = Mathf.FloorToInt(time * 2f) % notes.Length;
            float frequency = notes[noteIndex];

            float wave = Mathf.Sin(2f * Mathf.PI * frequency * time);
            float harmony = Mathf.Sin(2f * Mathf.PI * (frequency / 2f) * time) * 0.35f;

            data[i] = (wave + harmony) * volume;
        }

        clip.SetData(data, 0);
        return clip;
    }
}
