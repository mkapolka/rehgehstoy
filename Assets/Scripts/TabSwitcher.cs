using UnityEngine;
using System.Collections;

public class TabSwitcher : MonoBehaviour {

  public CanvasGroup[] tabs;

	public void ShowTab(int index) {
    foreach (CanvasGroup group in tabs) {
      group.alpha = 0;
      group.interactable = false;
      group.blocksRaycasts = false;
    }
    tabs[index].alpha = 1.0f;
    tabs[index].interactable = true;
    tabs[index].blocksRaycasts = true;
  }
}
