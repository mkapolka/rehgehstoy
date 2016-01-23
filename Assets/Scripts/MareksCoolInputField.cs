using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MareksCoolInputField : InputField {

	public int GetCharacterIndexFromPositionPublic(Vector2 pos) {
    Vector2 itp = this.GetComponent<RectTransform>().InverseTransformPoint(pos);
    return this.GetCharacterIndexFromPosition(itp);
  }
}
