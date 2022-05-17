using System;
using UnityEngine;
using System.Collections;

public class BulletScript : MonoBehaviour {

	[Tooltip("Furthest distance bullet will look for target")]
	public float maxDistance = 1000000;
	[Tooltip("Prefab of wall damange hit. The object needs 'LevelPart' tag to create decal on it.")]
	public GameObject decalHitWall;
	[Tooltip("Decal will need to be sligtly infront of the wall so it doesnt cause rendeing problems so for best feel put from 0.01-0.1.")]
	public float floatInfrontOfWall;
	[Tooltip("Blood prefab particle this bullet will create upoon hitting enemy")]
	public GameObject bloodEffect;
	[Tooltip("Put Weapon layer and Player layer to ignore bullet raycast.")]
	private LayerMask _ignoreLayer;

	private void Awake()
	{
		_ignoreLayer = 1 << LayerMask.NameToLayer("Player");
	}

	RaycastHit _hitInfo;

	/*
	* Uppon bullet creation with this script attatched,
	* bullet creates a raycast which searches for corresponding tags.
	* If raycast finds somethig it will create a decal of corresponding tag.
	*/
	void Update () {

		if(Physics.Raycast(transform.position, transform.forward,out _hitInfo, maxDistance, ~_ignoreLayer)){
			if(decalHitWall){
				if(_hitInfo.transform.CompareTag("Untagged")){
					Instantiate(decalHitWall, _hitInfo.point + _hitInfo.normal * floatInfrontOfWall, Quaternion.LookRotation(_hitInfo.normal));
					Destroy(gameObject);
				}
				if(_hitInfo.transform.CompareTag("Dummie")){
					Instantiate(bloodEffect, _hitInfo.point, Quaternion.LookRotation(_hitInfo.normal));
					Destroy(gameObject);
				}
			}		
			Destroy(gameObject);
		}
		Destroy(gameObject, 0.1f);
	}

}
