using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public enum SoundType { EatFood, Explosion, Gogogo, Grunt, Gunshot, Hit, HitAlt, HitKnife, MissJump, Tick}

    public Dictionary<SoundType, AudioSource> sounds = new Dictionary<SoundType, AudioSource>();

    public static SoundManager Instance;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        AudioSource[] sources = GetComponentsInChildren<AudioSource>();
        for (int i=0; i<sources.Length; i++) {
            sounds.Add((SoundType) i, sources[i]);
        }

    }

    public void Play(SoundType type) {
        if (type == SoundType.Hit || type == SoundType.HitAlt || type == SoundType.MissJump || type == SoundType.Gunshot || type == SoundType.HitKnife) {
            sounds[type].pitch = Random.Range(0.8f, 1.2f);
        }
        sounds[type].Play();
    }

    public void PlayMenuMove() {
        sounds[SoundType.MissJump].Play();
    }

    public void PlayMenuSelect() {
        sounds[SoundType.Hit].Play();
    }

}
