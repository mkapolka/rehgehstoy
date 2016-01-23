using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text.RegularExpressions;

public class TextDisplay : MonoBehaviour {

	public Text displayText;
  public MareksCoolInputField inputField;

  public bool primed = false;

  public void SetPrimed(bool p) {
    this.primed = p;
  }

  public void Update() {
    int idx = this.inputField.GetCharacterIndexFromPositionPublic(Input.mousePosition);
    if (this.IndexOnWord(idx) && this.primed) {
      string word = GetWordAtIndex(idx).Trim();
      TextButtonManager manager = GameObject.FindObjectOfType<TextButtonManager>();
      foreach (TextButton button in manager.GetComponentsInChildren(typeof(TextButton), true)) {
        if (button.word == word) {
          button.MouseOver();
        }
      }

      int[] indices = GetWordIndices(idx);
      if (!(indices[0] == 0 && indices[1] == 0) && Input.GetMouseButtonDown(0)) {
        this.inputField.text = this.EraseWord(this.inputField.text, indices[0], indices[1]);
        this.GetComponent<AudioSource>().Play();
      }
    }
  }

  public string EraseWord(string text, int indexStart, int indexEnd) {
    return text.Substring(0, indexStart) + "<color=#1001>" + text.Substring(indexStart, indexEnd - indexStart) + "</color>" + text.Substring(indexEnd, text.Length - indexEnd);
  }

  public string StripRichText(string text) {
    Regex rgx = new Regex("<color=#1001>.*?</color>|<.*?>");
    text = rgx.Replace(text, "");
    return text;
  }
  
  public void UpdateSuggestedButtons() {
    string ageText = this.GetAgeText();
    Language.WordType[] types = Language.PossibleNextWordTypes(ageText);
    GameObject.FindObjectOfType<TextButtonManager>().ShowSuggestedButtons(types);
  }

  public string GetAgeText() {
    return this.StripRichText(this.inputField.text);
  }

  public bool IndexOnWord(int index) {
    return index > 0 && index < this.inputField.text.Length
      && this.inputField.text[index] != ' ' && this.inputField.text[index] != '\n'
      && this.inputField.text[index] != '>' && this.inputField.text[index] != '<';
  }

  public int[] GetWordIndices(int index) {
    string text = this.inputField.text;

    if (index >= text.Length) {
      return new int[]{0, 0};
    }

    // backwards
    int i = index;
    bool done = false;
    while (i > 0 && !done) {
      char next = text[i];
      if (next == '<') { // Hit a tag start moving backwards, we were in a tag, invalid.
        return new int[]{0, 0};
      }
      if (next == ' ' || next == '\n' || next == '>') {
        i += 1;
        done = true;
      } else {
        i -= 1;
      }
    }

    int j = index;
    done = false;
    while (j < text.Length && !done) {
      char next = text[j];
      if (next == '>') { // Hit a tag end moving forwards, we were in a tag, invalid.
        return new int[]{0, 0};
      }
      if (next == ' ' || next == '\n' || next == '<') {
        done = true;
      } else {
        j += 1;
      }
    }
    
    return new int[]{i, j};
  }

  public string GetWordAtIndex(int index) {
    int[] indices = this.GetWordIndices(index);
    return this.inputField.text.Substring(indices[0], indices[1] - indices[0]);
  }
}
