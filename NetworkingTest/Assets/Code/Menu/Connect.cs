using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Connect : MonoBehaviour {
    public static ClientBehaviour clientBehaviour;
    public void ConnectToHost(TMP_InputField inputField) {
        clientBehaviour = (new GameObject()).AddComponent<ClientBehaviour>();
        clientBehaviour.Connect(inputField.text);
        Menu.Instance.SetMenu(Menu.Menus.clientLobby);
    }

    public void LocalHost() {
        clientBehaviour = (new GameObject()).AddComponent<ClientBehaviour>();
        clientBehaviour.ConnectLocalHost();
        Menu.Instance.SetMenu(Menu.Menus.clientLobby);
    }
}
