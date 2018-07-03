using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour {
	public static WeaponManager instance;

	public WeaponData[] weapons;

	void Awake () {
		instance = this;
	}

	void Start () {
		
	}
		
	[System.Serializable]
	public class WeaponData {
		public GameObject prefab;
		public Sprite sprite;
		public string name;
	}
}
