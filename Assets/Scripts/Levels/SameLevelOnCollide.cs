﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SameLevelOnCollide : MonoBehaviour {

	void OnTriggerEnter2D (Collider2D coll)
	{
		if(coll.gameObject.layer == LevelManager.Instance.PlayerLayer)
		{
			Debug.Log("Resetting Level");
			LevelManager.Instance.PlayerDying.PlayDelayed(LevelManager.Instance.PlayerDyingDelay); //toca som de player morrendo
			LevelManager.Instance.ResetLevel();
		}
	}

}
