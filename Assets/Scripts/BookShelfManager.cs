using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BookShelfManager : MonoBehaviour {

  public Text errorText;

	public void Start() {
    foreach (ShelfBook book in GameObject.FindObjectsOfType<ShelfBook>()) {
      if (book.gameObject.name != "Personal Book") {
        book.GetComponent<Button>().interactable = false;
      }
    }
    StartCoroutine(PopulateBookshelf());
  }

  public IEnumerator PopulateBookshelf() {
    WWW curl = new WWW("https://rehgehstoy.firebaseio.com/books.json");

    yield return curl;

    Debug.Log(curl.text);
    if (!string.IsNullOrEmpty(curl.error)) {
      this.errorText.text = curl.error;
    } else {
      JSONObject jso = new JSONObject(curl.text);
      Dictionary<string, string> diccy = jso.ToDictionary();
      foreach (ShelfBook book in GameObject.FindObjectsOfType<ShelfBook>()) {
        if (diccy.ContainsKey(book.gameObject.name)) {
          book.bookText = diccy[book.gameObject.name];
        }
        book.GetComponent<Button>().interactable = true;
      }
    }
  }
}
