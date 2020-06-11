using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using TMPro;
using System.IO;
using System.Net.Http;
using System;

public class Login : MonoBehaviour {
    [SerializeField] private TMP_InputField email;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private TextMeshProUGUI failed;

    public static string SessionID = null;
    public static string Username = null;
    private bool awaitingRequest = false;

    //old not async
    /*private void TryLogin() {
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
    }*/

    public void LoginButton() {
        GetHttpAsync();
    }

    private async void GetHttpAsync() {
        if(awaitingRequest) return;
        awaitingRequest = true;

        string url = "http://database.stijn.online/playerLogin.php?email=" + email.text + "&password=" + password.text;
        using(var client = new HttpClient()) {
            client.Timeout = TimeSpan.FromSeconds(10);
            try {
                var result = await client.GetAsync(url);
                if(result.IsSuccessStatusCode) {
                    string content = await result.Content.ReadAsStringAsync();
                    Debug.Log("SessionID: " + content);
                    if(content != "Incorrect Login Info") {
                        SessionID = content;
                        Username = email.text; //todo get actual username
                        Menu.Instance.SetMenu(Menu.Menus.clientConnect);
                    } else {
                        failed.text = "Incorrect Login Info";
                        StartCoroutine(resetFailedText());
                    }
                } else {
                    failed.text = "Request Failed";
                    StartCoroutine(resetFailedText());
                }
            } catch (Exception e){
                failed.text = "Request Timed Out";
                StartCoroutine(resetFailedText());
            }
        }
        awaitingRequest = false;
    }

    private IEnumerator resetFailedText() {
        yield return new WaitForSeconds(3f);
        failed.text = "";
    }
}
