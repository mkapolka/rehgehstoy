using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[ExecuteInEditMode]
public class TextButton : MonoBehaviour {

  public string word;
  public string info;
  public Language.WordType type;
  public InputField targetText;
  public Text buttonText;
  public AudioSource source;

  public AudioClip[] sounds;

  public void Update() {
    if (this.buttonText != null && this.buttonText.text != this.word) {
      this.buttonText.text = this.word;
    }
  }

  public void Clicko() {
    this.source.PlayOneShot(this.sounds[(int)Mathf.Floor(Random.value * this.sounds.Length)]);

    string space = "";
    if ((this.type == Language.WordType.Feature || this.type == Language.WordType.NegativeFeature) && this.targetText.text.Trim() != "") {
      space = "\n\n";
    } else {
      space = " ";
    }

    this.targetText.text += space + this.word;
  }

  public void MouseOver() {
    CheatSheet sheet = GameObject.FindObjectOfType<CheatSheet>() as CheatSheet;
    sheet.dniText.text = this.word;
    sheet.infoText.text = this.info != "" ? this.info : "????";
  }
}
