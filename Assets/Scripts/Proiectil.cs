using System.Collections;
using UnityEngine;


namespace GK {
	public class Proiectil : MonoBehaviour {

		IEnumerator Start() {
			yield return new WaitForSeconds(5.0f);
			Destroy(gameObject);
		}

	}
}
