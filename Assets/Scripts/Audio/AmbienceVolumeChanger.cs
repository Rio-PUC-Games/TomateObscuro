﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbienceVolumeChanger : MonoBehaviour {
	[System.Serializable] public class ChangerOptions
	{
		[Range(0,1)] public float NewVolume;
		public string AudioSourceName;
		[HideInInspector] public AudioSource SoundToChange;
	}

	public ChangerOptions[] Sounds;

	// Use this for initialization
	void Start () {
		foreach (ChangerOptions sound in Sounds) {
			sound.SoundToChange = GameObject.Find (sound.AudioSourceName).GetComponent<AudioSource>();
			sound.SoundToChange.volume = sound.NewVolume;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
