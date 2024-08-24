using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using static MusicManager;

public class MusicManager : MonoBehaviour
{
    public enum Melody { None, MainMenu, IntroOutro, InTheStreets, InTheBar };

    public event EventHandler<OnFadeEventArgs> OnFadeOut;
    public class OnFadeEventArgs : EventArgs {
        public Melody musicFaded;
    }

    [SerializeField] private float durationFade;
    [SerializeField] private AudioMixerGroup masterMixer;
    [SerializeField] private AudioClip[] musicThemes;

    private AudioSource audioSource;
    private Melody nextMelody = Melody.None;
    public Melody CurrentMelody { get; private set; } = Melody.None;
    private bool isFadingOut;
    private bool isFadingIn;
    private float timeStartFade = float.NegativeInfinity;
    private Dictionary<Melody, AudioClip> gameMusics = new Dictionary<Melody, AudioClip>();

    public static MusicManager Instance;

    private void Awake() {
        Instance = this;
        audioSource = GetComponent<AudioSource>();
        gameMusics.Clear();
        gameMusics.Add(Melody.MainMenu, musicThemes[0]);
        gameMusics.Add(Melody.IntroOutro, musicThemes[1]);
        gameMusics.Add(Melody.InTheStreets, musicThemes[2]);
        gameMusics.Add(Melody.InTheBar, musicThemes[3]);
    }

    public void Play(Melody melody) {
        if (isFadingIn || isFadingOut) {
            Debug.Log("Could not play melody, still fading stuff");
        } else {
            nextMelody = melody;
            if (CurrentMelody == Melody.None) {
                StartTrack();
            } else {
                isFadingOut = true;
            }
            timeStartFade = Time.timeSinceLevelLoad;
        }
    }

    public void Stop() {
        if (!isFadingIn && !isFadingOut) {
            nextMelody = Melody.None;
            isFadingOut = true;
            timeStartFade = Time.timeSinceLevelLoad;
        }

    }

    private void Update() {
        if (isFadingOut) {
            if (Time.timeSinceLevelLoad - timeStartFade < durationFade) {
                float progress = (Time.timeSinceLevelLoad - timeStartFade) / durationFade;
                SetMasterVolume(1 - progress);
            } else {
                isFadingOut = false;
                OnFadeOut?.Invoke(this, new OnFadeEventArgs() {
                    musicFaded = CurrentMelody
                });
                StartTrack();
            }
        } else if (isFadingIn) {
            if (Time.timeSinceLevelLoad - timeStartFade < durationFade) {
                float progress = (Time.timeSinceLevelLoad - timeStartFade) / durationFade;
                SetMasterVolume(progress);
            } else { 
                isFadingIn = false;
            }
        }
    }

    private void SetMasterVolume(float volume) {
        volume = Mathf.Clamp01(volume);
        masterMixer.audioMixer.SetFloat("Volume-Master", 20.0f * Mathf.Log10(volume));
    }

    private void StartTrack() {
        if (nextMelody != Melody.None) { // swap music while volume at 0
            audioSource.clip = gameMusics[nextMelody];
            audioSource.Play();
            CurrentMelody = nextMelody;
            nextMelody = Melody.None;
            isFadingIn = true;
            timeStartFade = Time.timeSinceLevelLoad;
        }
    }
}
