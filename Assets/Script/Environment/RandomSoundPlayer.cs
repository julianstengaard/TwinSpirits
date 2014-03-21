﻿using UnityEngine;
using System.Collections.Generic;

public class RandomSoundPlayer : MonoBehaviour {

	public RandomSoundStruct[] Sounds;
	public Dictionary<string, AudioClip[]> _sounds = new Dictionary<string, AudioClip[]>();
	private Dictionary<string, int> _lastPlayed = new Dictionary<string, int>();

	private AudioSource _source;
	private bool ready = false;

	void Start () {
		_source = GetComponent<AudioSource>();

		if(_source == null) print("No audio source");

		foreach(var s in Sounds) {
			_sounds.Add(s.Name, s.Sounds);
		}

	}

	public float PlayRandomSound(string name) {
		var soundPos = Random.Range(0, _sounds[name].Length);

		if(!_lastPlayed.ContainsKey(name)) _lastPlayed.Add(name, 0);

		soundPos = _lastPlayed[name] == soundPos ? soundPos == 0 ? 1 : 0 : soundPos;

		var sound = _sounds[name][soundPos];
		_lastPlayed[name] = soundPos;
		_source.PlayOneShot(sound);
		return sound.length;
	}
}
