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

	public WeaponData GetEquipedWeapon () {
		string weaponName = PlayerPrefs.GetString ("weapon", "pistol");
		foreach (var weapon in weapons) {
			if (weapon.name == weaponName) {
				return weapon;
			}
		}

		print ("Could not find weapon with name: " + weaponName);
		return weapons [0];
	}
		
	[System.Serializable]
	public class WeaponData {
		public GameObject prefab;
		public Sprite sprite;
		public string name;
	}
}
