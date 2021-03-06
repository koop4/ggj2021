﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{

    public static Launcher Instance;

    // create room input reference
    [SerializeField] TMP_InputField roomNameInputField;

    // create player input reference
    [SerializeField] TMP_InputField userNameInputField;

    // error text is displayed on the error panel. 
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject playerListItemPrefab;
    [SerializeField] GameObject startGameButton;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Connecting 2 Master");
        PhotonNetwork.ConnectUsingSettings();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected 2 Master");
        PhotonNetwork.JoinLobby();
        // this allow to load scene 1 for all player when host start
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu("connect");
        Debug.Log("Lobby Joined");

        PhotonNetwork.NickName = (PhotonNetwork.NickName != "" && PhotonNetwork.NickName != null) ? PhotonNetwork.NickName : ("Player" + Random.Range(0, 1000).ToString("0000"));
    }

    //public override void OnPlayerPropertiesUpdate()
    //{
    //    MenuManager.Instance.OpenMenu("connect");
    //    Debug.Log("Lobby Joined");

    //    PhotonNetwork.NickName = (PhotonNetwork.NickName != "" && PhotonNetwork.NickName != null) ? PhotonNetwork.NickName : ("Player" + Random.Range(0, 1000).ToString("0000"));
    //}

    public void CreateRoom()
    {
        if(string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
        PhotonNetwork.CreateRoom(roomNameInputField.text);
        PhotonNetwork.NickName = (PhotonNetwork.NickName != "" && PhotonNetwork.NickName != null) ? PhotonNetwork.NickName : ("Player" + Random.Range(0, 1000).ToString("0000"));
        MenuManager.Instance.OpenMenu("loading"); 
    }

    public void SaveUsername()
    {
        //PhotonNetwork.CreateRoom(roomNameInputField.text);
        PhotonNetwork.NickName = userNameInputField.text;
        Debug.Log("PhotonNetwork.NickName " + PhotonNetwork.NickName);
        MenuManager.Instance.OpenMenu("connect");
    }

    public override void OnJoinedRoom()
    {
        MenuManager.Instance.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;
        Player[] players = PhotonNetwork.PlayerList;
        
        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(players[i]);
        }
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }


    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        MenuManager.Instance.OpenMenu("error");
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("loading");
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("loading");
    }


    public override void OnLeftRoom()
    {
        //TODO remove player from roomlist then call onroomlistupdate
        Debug.Log("Player Left");
        MenuManager.Instance.OpenMenu("connect");
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("Updating Room List");
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }
        for (int i = 0; i < roomList.Count; i++)
        {
            //photon removed list do not get referencially removed. 
            // so we check for the RemoveFromList bool and act accordingly skipping them
            if (roomList[i].RemovedFromList)
                continue;
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().Setup(roomList[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("New Player");
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().Setup(newPlayer);
    }

    public void StartGame()
    {
        Debug.Log("Start Game");
        PhotonNetwork.LoadLevel(1);
    }



}
