using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace GK 
{
	public class Shoot : MonoBehaviour 
	{

		public GameObject proiectil; 
		public float MinDelay = 0.25f;
		public float InitialSpeed = 10.0f;
		public Transform SpawnLocation;

		float lastShot = -1000.0f;

		void Update() 
		{
            bool shooting = CrossPlatformInputManager.GetButton("Fire1");

			if (shooting) 
			{
				if (Time.time - lastShot >= MinDelay) 
				{
					lastShot = Time.time;

					GameObject projectileShoot = Instantiate(proiectil, SpawnLocation.position, SpawnLocation.rotation);

					projectileShoot.GetComponent<Rigidbody>().velocity = InitialSpeed * Camera.main.transform.forward;
				}
			} 
		}
	}
}
