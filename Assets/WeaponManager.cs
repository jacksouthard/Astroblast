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

	public WeaponData GetDataFromIndex (int index) {
		return weapons [index];
	}
		
	[System.Serializable]
	public class WeaponData {
		public GameObject prefab;
		public Sprite sprite;
		public string name;
	}
}
