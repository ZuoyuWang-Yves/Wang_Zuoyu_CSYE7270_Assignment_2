using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameSfxAction
{
    CollectiblePickup,
    Flash,
    Explosion,
    HoleNotReady,
    BouncyHit,
    SlopeHit
}

public class ProceduralChordSfx : MonoBehaviour
{
    public static ProceduralChordSfx Instance;

    // [Header("General Audio")]
    // General Audio
    public float masterVolume = 0.7f;
    public float chordDuration = 0.28f;
    public int sampleRate = 44100;

    // [Header("Replay UI")]
    // End game replay UI
    public Slider replayProgressBar;
    public bool showProgressOnlyDuringReplay = true;

    private AudioSource oneShotSource;
    private Dictionary<GameSfxAction, AudioClip> clipCache = new Dictionary<GameSfxAction, AudioClip>();
    private List<RecordedEvent> recordedEvents = new List<RecordedEvent>();
    private float recordStartTime;
    private Coroutine replayRoutine;

    private struct RecordedEvent
    {
        public GameSfxAction action;
        public float timeOffset;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        oneShotSource = GetComponent<AudioSource>();
        if (oneShotSource == null)
        {
            oneShotSource = gameObject.AddComponent<AudioSource>();
        }

        oneShotSource.playOnAwake = false;
        recordStartTime = Time.time;

        if (replayProgressBar != null)
        {
            replayProgressBar.value = 0f;
            if (showProgressOnlyDuringReplay)
            {
                replayProgressBar.gameObject.SetActive(false);
            }
        }
    }

    public static void PlayAction(GameSfxAction action)
    {
        ProceduralChordSfx manager = EnsureInstance();
        manager.PlayAndRecord(action);
    }

    public static void ReplayRecordedSong()
    {
        ProceduralChordSfx manager = EnsureInstance();
        manager.ReplaySong();
    }

    private static ProceduralChordSfx EnsureInstance()
    {
        if (Instance != null)
        {
            return Instance;
        }

        GameObject obj = new GameObject("ProceduralChordSfx");
        return obj.AddComponent<ProceduralChordSfx>();
    }

    private void PlayAndRecord(GameSfxAction action)
    {
        AudioClip clip = GetOrCreateClip(action);
        oneShotSource.PlayOneShot(clip, masterVolume);

        RecordedEvent newEvent = new RecordedEvent
        {
            action = action,
            timeOffset = Time.time - recordStartTime
        };
        recordedEvents.Add(newEvent);
    }

    private AudioClip GetOrCreateClip(GameSfxAction action)
    {
        if (clipCache.TryGetValue(action, out AudioClip cached))
        {
            return cached;
        }

        int rootMidi;
        int[] intervals;
        GetChordDefinition(action, out rootMidi, out intervals);

        AudioClip generated = BuildChordClip(action.ToString(), rootMidi, intervals);
        clipCache[action] = generated;
        return generated;
    }

    private void GetChordDefinition(GameSfxAction action, out int rootMidi, out int[] intervals)
    {
        // Different actions use different chord colors.
        switch (action)
        {
            case GameSfxAction.CollectiblePickup:
                rootMidi = 72; // C5
                intervals = new[] { 0, 4, 7 }; // major
                break;
            case GameSfxAction.Flash:
                rootMidi = 74; // D5
                intervals = new[] { 0, 4, 9 }; // add6 flavor
                break;
            case GameSfxAction.Explosion:
                rootMidi = 55; // G3
                intervals = new[] { 0, 3, 6 }; // diminished
                break;
            case GameSfxAction.HoleNotReady:
                rootMidi = 57; // A3
                intervals = new[] { 0, 3, 7 }; // minor
                break;
            case GameSfxAction.BouncyHit:
                rootMidi = 67; // G4
                intervals = new[] { 0, 4, 7 }; // major
                break;
            case GameSfxAction.SlopeHit:
                rootMidi = 65; // F4
                intervals = new[] { 0, 4, 7 }; // major
                break;
            default:
                rootMidi = 60;
                intervals = new[] { 0, 4, 7 };
                break;
        }
    }

    private AudioClip BuildChordClip(string clipName, int rootMidi, int[] intervals)
    {
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(chordDuration * sampleRate));
        float[] samples = new float[sampleCount];

        float[] frequencies = new float[intervals.Length];
        for (int i = 0; i < intervals.Length; i++)
        {
            frequencies[i] = MidiToFrequency(rootMidi + intervals[i]);
        }

        for (int n = 0; n < sampleCount; n++)
        {
            float t = n / (float)sampleRate;
            float value = 0f;

            for (int i = 0; i < frequencies.Length; i++)
            {
                value += Mathf.Sin(2f * Mathf.PI * frequencies[i] * t);
            }

            value /= frequencies.Length;
            value *= Envelope01(t / chordDuration);
            samples[n] = value * 0.8f;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private float Envelope01(float normalizedTime)
    {
        float attack = 0.08f;
        float releaseStart = 0.45f;

        if (normalizedTime < attack)
        {
            return normalizedTime / attack;
        }

        if (normalizedTime > releaseStart)
        {
            float tail = 1f - ((normalizedTime - releaseStart) / (1f - releaseStart));
            return Mathf.Clamp01(tail);
        }

        return 1f;
    }

    private float MidiToFrequency(int midiNote)
    {
        return 440f * Mathf.Pow(2f, (midiNote - 69) / 12f);
    }

    private void ReplaySong()
    {
        if (recordedEvents.Count == 0)
        {
            return;
        }

        if (replayRoutine != null)
        {
            StopCoroutine(replayRoutine);
        }

        replayRoutine = StartCoroutine(ReplaySongRoutine());
    }

    private IEnumerator ReplaySongRoutine()
    {
        float previousTime = 0f;
        float totalDuration = recordedEvents[recordedEvents.Count - 1].timeOffset;

        if (replayProgressBar != null)
        {
            replayProgressBar.value = 0f;
            if (showProgressOnlyDuringReplay)
            {
                replayProgressBar.gameObject.SetActive(true);
            }
        }

        for (int i = 0; i < recordedEvents.Count; i++)
        {
            RecordedEvent ev = recordedEvents[i];
            float wait = Mathf.Max(0f, ev.timeOffset - previousTime);

            float elapsed = 0f;
            while (elapsed < wait)
            {
                elapsed += Time.deltaTime;

                if (replayProgressBar != null && totalDuration > 0f)
                {
                    float currentReplayTime = previousTime + Mathf.Min(elapsed, wait);
                    replayProgressBar.value = Mathf.Clamp01(currentReplayTime / totalDuration);
                }

                yield return null;
            }

            AudioClip clip = GetOrCreateClip(ev.action);
            oneShotSource.PlayOneShot(clip, masterVolume);
            previousTime = ev.timeOffset;
        }

        if (replayProgressBar != null)
        {
            replayProgressBar.value = 1f;
            if (showProgressOnlyDuringReplay)
            {
                yield return new WaitForSeconds(0.4f);
                replayProgressBar.gameObject.SetActive(false);
                replayProgressBar.value = 0f;
            }
        }

        replayRoutine = null;
    }
}
