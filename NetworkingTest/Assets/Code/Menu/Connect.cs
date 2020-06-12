using Assets.Code.Client;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Connect : MonoBehaviour {
    public void ConnectToHost(TMP_InputField inputField) {
        (new GameObject()).AddComponent<ClientManager>().clientBehaviour.Connect(inputField.text);
        Menu.Singleton.SetMenu(Menu.Menus.clientLobby);
    }

    public void LocalHost() {
        (new GameObject()).AddComponent<ClientManager>().clientBehaviour.ConnectLocalHost();
        Menu.Singleton.SetMenu(Menu.Menus.clientLobby);
    }
}
