using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using TMPro;
using System.IO;

public class Login : MonoBehaviour
{
    [SerializeField] private TMP_InputField email;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private TextMeshProUGUI failed;

    public static string SessionID = null;
    public static string Username = null;

    public void TryLogin() {
        HttpWebRequest myReq =(HttpWebRequest)WebRequest.Create("http://database.stijn.online/playerLogin.php?email=" + email.text + "&password=" + password.text);
        HttpWebResponse response = (HttpWebResponse)myReq.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());

        string responseText = reader.ReadToEnd();
        Debug.Log("SessionID: " + responseText);
        if(responseText == "Incorrect Login Info") {
            failed.text = "Incorrect Login Info";
            StartCoroutine(resetFailedText());
        } else {
            SessionID = responseText;
            Username = email.text; //change to actual username
            Menu.Instance.SetMenu(Menu.Menus.clientConnect);
        }
    }

    private IEnumerator resetFailedText() {
        yield return new WaitForSeconds(1f);
        failed.text = "";
    }
}
