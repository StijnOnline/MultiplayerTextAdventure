﻿using Assets.Code;
using Assets.Code.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour {
    [SerializeField] private GameObject chooseMenu;
    [SerializeField] private GameObject hostOutput;
    [SerializeField] private GameObject toggleViewButton;
    [SerializeField] private GameObject clientLogin;
    [SerializeField] private GameObject clientConnect;
    [SerializeField] private GameObject clientLobby;
    [SerializeField] private LobbyWindow[] LobbyWindows;
    [SerializeField] private GameObject clientGame;
    private Menus lastToggleMenu;

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

    public void UpdateLobbyWindows(Lobby lobby) {
        foreach(var lobbyWindows in LobbyWindows) {
            lobbyWindows.UpdatePlayers(lobby);
        }
    }

    public void Host() {
        (new GameObject()).AddComponent<ServerManager>();
        SetMenu(Menus.hostOutput);
    }

    public void Client() {
        SetMenu(Menus.clientLogin);
    }

    public void Both() {
        (new GameObject()).AddComponent<ServerManager>();
        SetMenu(Menus.clientLogin);
    }

    public void ToggleView() {
        if(currentMenu == Menus.hostOutput) {            
            SetMenu(lastToggleMenu);
        }else {
            lastToggleMenu = currentMenu;
            SetMenu(Menus.hostOutput);
        }
    }

    public void StartGame() {
        ServerManager.Singleton.StartGame();
    }

    public void SetMenu(Menus menu) {
        currentMenu = menu;
        chooseMenu.SetActive(menu == Menus.chooseMenu);
        hostOutput.SetActive(menu == Menus.hostOutput);
        clientLogin.SetActive(menu == Menus.clientLogin);
        clientConnect.SetActive(menu == Menus.clientConnect);
        clientLobby.SetActive(menu == Menus.clientLobby);
        clientGame.SetActive(menu == Menus.clientGame);

        toggleViewButton.SetActive(menu == Menus.hostOutput || menu == Menus.clientLobby || menu == Menus.clientGame);        
    }
}
