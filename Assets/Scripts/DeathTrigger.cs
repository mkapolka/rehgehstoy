using UnityEngine;
using System.Collections;

public class DeathTrigger : MonoBehaviour {

	public void OnTriggerEnter(Collider other) {
    if (other.gameObject.GetComponent<FirstPersonDrifter>()) {
      GameObject.FindObjectOfType<LinkingBook>().LinkOut();
    }
  }
}
