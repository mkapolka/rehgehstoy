using UnityEngine;
using System;
using System.Collections;

public class TextButtonManager : MonoBehaviour {

  public void ShowSuggestedButtons(Language.WordType[] types) {
    foreach (TextButton button in this.GetComponentsInChildren(typeof(TextButton), true)) {
      button.gameObject.SetActive(Array.IndexOf(types, button.type) != -1);
    }
    foreach (TextButtonLabel label in this.GetComponentsInChildren(typeof(TextButtonLabel), true)) {
      label.gameObject.SetActive(Array.IndexOf(types, label.type) != -1);
    }
  }
}
