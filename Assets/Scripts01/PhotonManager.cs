using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public TMP_InputField userNameText;
    public TMP_InputField roomNameText;
    public TMP_InputField maxPlayer;


    public GameObject PlayerNamePanel;
    public GameObject LobbyPanel;
    public GameObject RoomCreatedPanel;
    public GameObject ConnectingPanel;
    public GameObject RoomListPanel;


    private Dictionary<string, RoomInfo> roomListData;

    public GameObject roomListPrefab;
    public Transform roomListParent;

    private Dictionary<string, GameObject> roomListGameobject;
    private Dictionary<int, GameObject> playerListGameobject;

    [Header("Inside Room Panel")]
    public GameObject InsideRoomPanel;
    public GameObject playerListItemPrefab;
    public GameObject playerListItemParent;
    public GameObject PlayButton;

    #region UnityMethod
    // Start is called before the first frame update
    void Start()
    {
        ActivateMyPanel(PlayerNamePanel.name);
        roomListData = new Dictionary<string, RoomInfo>();
        roomListGameobject = new Dictionary<string, GameObject>();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Network state:" + PhotonNetwork.NetworkClientState);
    }


    #endregion  


    #region UiMethod

    public void OnLoginClick()
    {
        string name = userNameText.text;
        if (!string.IsNullOrEmpty(name))
        {
            PhotonNetwork.LocalPlayer.NickName = name;
            PhotonNetwork.ConnectUsingSettings();
            ActivateMyPanel(ConnectingPanel.name);
        }
        else
        {
            Debug.Log("Empty name ");
        }
    }



    #endregion

    public void OnclickRoomCreate()
    {
        string roomName = roomNameText.text;
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = roomName + Random.Range(0, 1000);
        }

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)int.Parse(maxPlayer.text);
        PhotonNetwork.CreateRoom(roomName, roomOptions);
        
    }

    #region PHOTON_CALLBACKS

    public void OnCancelClick()
    {
        ActivateMyPanel(LobbyPanel.name);
    }

    public void RoomListBtnClicked()
    {
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
        }
        ActivateMyPanel(RoomListPanel.name);
    }

    public void BackFromRoomList()
    {
        if(PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        ActivateMyPanel(LobbyPanel.name);
    }
    public void BackFromplayerList()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
        ActivateMyPanel(LobbyPanel.name);
    }


    public override void OnConnected()
    {
        Debug.Log("Connected to internet!");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + "is connected to photon...");
        ActivateMyPanel(LobbyPanel.name);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log(PhotonNetwork.CurrentRoom.Name + "Is created !");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName + "Room Joined !");
        ActivateMyPanel(InsideRoomPanel.name);

        if(playerListGameobject == null)
        {
            playerListGameobject = new Dictionary<int, GameObject>();
        }

        if(PhotonNetwork.IsMasterClient)
        {
            PlayButton.SetActive(true);
        }
        else
        {
            PlayButton.SetActive(false);
        }

        foreach(Player p in PhotonNetwork.PlayerList)
        {
            GameObject playerListItem = Instantiate(playerListItemPrefab);
            playerListItem.transform.SetParent(playerListItemParent.transform);
            playerListItem.transform.localScale = Vector3.one;

            playerListItem.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text=p.NickName;// show player name
            if (p.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber) 
            {
                playerListItem.transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                playerListItem.transform.GetChild(0).gameObject.SetActive(false);
            }

            playerListGameobject.Add(p.ActorNumber, playerListItem);
        }

    }


    public override void OnPlayerEnteredRoom(Player newPlayer) //remote player
    {
        GameObject playerListItem = Instantiate(playerListItemPrefab);
        playerListItem.transform.SetParent(playerListItemParent.transform);
        playerListItem.transform.localScale = Vector3.one;

        playerListItem.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = newPlayer.NickName;// show player name
        if (newPlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
        {
            playerListItem.transform.GetChild(0).gameObject.SetActive(true);
        }
        else
        {
            playerListItem.transform.GetChild(0).gameObject.SetActive(false);
        }

        playerListGameobject.Add(newPlayer.ActorNumber, playerListItem);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) //  player left
    {
        Destroy(playerListGameobject[otherPlayer.ActorNumber]);
        playerListGameobject.Remove(otherPlayer.ActorNumber);

        if (PhotonNetwork.IsMasterClient)
        {
            PlayButton.SetActive(true);
        }
        else
        {
            PlayButton.SetActive(false);
        }
    }

    public void OnClickPlayButton() 
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameScene"); /////
        } 
    }

    public override void OnLeftRoom()
    {
        ActivateMyPanel(LobbyPanel.name);
        foreach(GameObject obj in playerListGameobject.Values)
        {
            Destroy(obj);
        }
    }
    

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //Clear List

        ClearRoomList();
        foreach (RoomInfo rooms in roomList) /// chng
        {
            Debug.Log("Room Name : " + rooms.Name);
            if (!rooms.IsOpen || !rooms.IsVisible || rooms.RemovedFromList)
            {
                if (roomListData.ContainsKey(rooms.Name))
                {
                    roomListData.Remove(rooms.Name);
                }
            }
            else
            {
                if (roomListData.ContainsKey(rooms.Name))
                {
                    //update list
                    roomListData[rooms.Name] = rooms;
                }
                else
                {
                    //add
                    roomListData.Add(rooms.Name, rooms);
                }
            }
        }

        // Generate list
        foreach (RoomInfo roomItem in roomListData.Values)
        {
            GameObject roomListItemObject = Instantiate(roomListPrefab);
            roomListItemObject.transform.SetParent(roomListParent);
            roomListItemObject.transform.localScale = Vector3.one;
            // room name player Number Button room Join

            roomListItemObject.transform.GetChild(0).gameObject.GetComponent<TMP_Text>().text = roomItem.Name;
            roomListItemObject.transform.GetChild(1).gameObject.GetComponent<TMP_Text>().text = roomItem.PlayerCount + "/" + roomItem.MaxPlayers;
            roomListItemObject.transform.GetChild(2).gameObject.GetComponent<Button>().onClick.AddListener(() => RoomJoinFromList(roomItem.Name));
            roomListGameobject.Add(roomItem.Name, roomListItemObject);
        }
    }

    public override void OnLeftLobby()
    {
        ClearRoomList();
        roomListData.Clear();  


    }
    #endregion




    #region Public_Methods

    public void RoomJoinFromList(string roomName)
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
        PhotonNetwork.JoinRoom(roomName);
    }
    public void ClearRoomList()

    {
        if (roomListGameobject.Count > 0)
        {
            foreach (var v in roomListGameobject.Values)
            {
                Destroy(v);
            }
            roomListGameobject.Clear();
        }
    }

    public void ActivateMyPanel(string panelName)
    {
        LobbyPanel.SetActive(panelName.Equals(LobbyPanel.name));
        PlayerNamePanel.SetActive(panelName.Equals(PlayerNamePanel.name));
        RoomCreatedPanel.SetActive(panelName.Equals(RoomCreatedPanel.name));
        ConnectingPanel.SetActive(panelName.Equals(ConnectingPanel.name));
        RoomListPanel.SetActive(panelName.Equals(RoomListPanel.name)); //
        InsideRoomPanel.SetActive(panelName.Equals(InsideRoomPanel.name));

    }

    #endregion





    // change
   
}

