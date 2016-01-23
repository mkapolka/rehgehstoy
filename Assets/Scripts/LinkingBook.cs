using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;


public class LinkingBook : MonoBehaviour {

  public string bookId;
	public TerrainoTron terrain;
  public GameObject player;
  public Canvas canvas;
  public CanvasGroup canvasGroup;
  public AudioSource linkSound;
  public TextDisplay textSource;
  public CanvasGroup fadeOut;

  public AudioSource bookSfx;
  public AudioSource ageSfx;

  public void Start() {
    this.SetPlayerLocked(true);
  }

  public void UploadBook() {
    string bookText = this.textSource.inputField.text;

    ShelfBook shelfBook = GameObject.Find(this.bookId).GetComponent<ShelfBook>();
    bool shouldUpload = shelfBook.bookText != bookText;
    shelfBook.bookText = bookText;
    
    if (this.bookId != "Personal Book" && bookText.Trim() != "" && shouldUpload) {
      Debug.Log("Uploading " + this.bookId + "...");
      using(var client = new System.Net.WebClient()) {
        string result = Regex.Replace(bookText, @"[^\x00-\x7F]", c => 
            string.Format(@"\u{0:x4}", (int)c.Value[0]));
        byte[] payload = new ASCIIEncoding().GetBytes("\"" + result + "\"");
        client.UploadDataAsync(new System.Uri("http://rehgehstoy.firebaseio.com/books/" + this.bookId + ".json"), "PUT", payload);
      }
    }
  }

  public void TouchLinkPanel() {
    this.LockCursor(true);
    StartCoroutine(Fadey(true));
  }

  public void SetPlayerLocked(bool locked) {
    if (locked) {
      player.transform.position = new Vector3(-1000, -1000, -500);
      player.GetComponent<FirstPersonDrifter>().gravity = 0;
      player.GetComponent<FirstPersonDrifter>().moveDirection = Vector3.zero;
    } else {
      player.GetComponent<FirstPersonDrifter>().gravity = 10f;
      player.GetComponent<FirstPersonDrifter>().moveDirection = Vector3.zero;
    }
  }

  public void LinkOut() {
    StartCoroutine(Fadey(false));
  }

  public IEnumerator Fadey(bool goingIntoAge) {
    linkSound.PlayOneShot(linkSound.clip);
    float timer = 1.0f;
    canvasGroup.interactable = false;

    while (timer > 0.0f) {
      timer -= Time.deltaTime;
      fadeOut.alpha = (1 - timer);
      yield return null;
    }

    if (goingIntoAge) {
      this.bookSfx.Stop();
      this.ageSfx.Play();
      SetPlayerLocked(false);
      Vector3 position = terrain.GetLinkInPosition();
      if (position.y < 1.0f) {
        position.y += 100.0f;
      }
      player.transform.position = position + new Vector3(0, 1.0f, 0);
      canvasGroup.alpha = 0.0f;
    } else {
      this.bookSfx.Play();
      this.ageSfx.Stop();
      SetPlayerLocked(true);
      this.LockCursor(false);
      canvasGroup.alpha = 1.0f;
      canvasGroup.interactable = true;
    }

    timer = 1.0f;
    while (timer > 0.0f) {
      timer -= Time.deltaTime;
      fadeOut.alpha = timer;
      yield return null;
    }
  }

  public void GenerateAge() {
    Age.TerrainChunk chunk = Language.GenerateTerrain(this.textSource.GetAgeText());
    this.terrain.ApplyChunk(chunk);
  }

  public void Update() {
    if (Input.GetButtonDown("Link Out") || Input.GetKeyDown(KeyCode.Escape)) {
      if (this.canvasGroup.alpha == 0) {
        StartCoroutine(Fadey(false));
      }
    }

    if (Input.GetKeyDown(KeyCode.Escape)) {
      this.LockCursor(false);
    }

    if( Input.GetMouseButtonDown(0) && this.canvasGroup.alpha == 0)
    {
      LockCursor(true);
    }
  }

  public void LockCursor(bool lockCursor)
  {
    Screen.lockCursor = lockCursor;
  }
}
