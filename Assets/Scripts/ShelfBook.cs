using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShelfBook : MonoBehaviour {
  public TabSwitcher bookSwitcher;
  public TabSwitcher pageSwitcher;

	public string bookText;
  public InputField textField;
  public LinkingBook linkingBook;

  public void Click() {
    this.bookSwitcher.ShowTab(0);
    this.pageSwitcher.ShowTab(0);
    this.linkingBook.bookId = this.gameObject.name;
    this.textField.text = this.bookText;
    this.linkingBook.GenerateAge();
  }
}
