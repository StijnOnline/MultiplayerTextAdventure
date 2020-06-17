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

    public static string Username = null;
    private bool awaitingRequest = false;

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
                        Username = content;
                        Menu.Singleton.SetMenu(Menu.Menus.clientConnect);
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
