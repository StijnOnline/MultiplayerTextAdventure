using Assets.Code;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour {
    [SerializeField] private GameObject chooseMenu;
    [SerializeField] private GameObject hostOutput;
    [SerializeField] private Commandline hostCommandline;
    [SerializeField] private GameObject clientLogin;
    [SerializeField] private GameObject clientConnect;
    [SerializeField] private GameObject clientLobby;
    public LobbyWindow LobbyWindow;
    [SerializeField] private GameObject clientGame;

    public enum Menus {
        chooseMenu,
        hostOutput,
        clientLogin,
        clientConnect,
        clientLobby,
        clientGame
    }
    private Menus currentMenu;
    public static Menu Instance;


    private void Awake() {
        if(Instance != null && Instance != this) {
            Destroy(gameObject);
        } else {
            Instance = this;
        }

        SetMenu(Menus.chooseMenu);
    }

    public void Host() {
        hostCommandline.StartHost();
        SetMenu(Menus.hostOutput);
    }

    public void Client() {
        SetMenu(Menus.clientLogin);
    }

    public void Both() {
        hostCommandline.StartHost();
        SetMenu(Menus.clientLogin);
    }

    public void SetMenu(Menus menu) {
        currentMenu = menu;
        chooseMenu.SetActive(menu == Menus.chooseMenu);
        hostOutput.SetActive(menu == Menus.hostOutput);
        clientLogin.SetActive(menu == Menus.clientLogin);
        clientConnect.SetActive(menu == Menus.clientConnect);
        clientLobby.SetActive(menu == Menus.clientLobby);
        clientGame.SetActive(menu == Menus.clientGame);
    }
}
