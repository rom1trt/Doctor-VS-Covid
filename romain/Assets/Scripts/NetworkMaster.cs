using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkMaster : MonoBehaviourPunCallbacks
{
    private bool inRoom = false;
    public static NetworkMaster instance; // singleton

    public SelectedPlayer selection = SelectedPlayer.None;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            instance = this;
        }
    }

    void Update()
    {
        if (inRoom)
        {
            // check if all characters have been selected and if match isn't started yet
            if (!GameManager.instance.doctorAvailable && !GameManager.instance.virusAvailable && !GameManager.started)
            {
                GameManager.StartMatch(); // starts the match

                // instantiate the correct selected character
                switch (FindObjectOfType<CharacterSelection>().selection)
                {
                    case SelectedPlayer.Doctor:
                        PhotonNetwork.Instantiate(GameManager.instance.doctor.name, GameManager.instance.doctorSpawn.position, Quaternion.identity);
                        break;
                    case SelectedPlayer.Virus:
                        PhotonNetwork.Instantiate(GameManager.instance.virus.name, GameManager.instance.virusSpawn.position, Quaternion.identity);
                        break;
                }
            }
        }
    }

    // connects to photon master server
    public static void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
    }


    public override void OnConnectedToMaster()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 2; // match can have max 2 players
        PhotonNetwork.JoinOrCreateRoom("Match", roomOptions, TypedLobby.Default); // create the match or join it if it already exists
    }


    public override void OnJoinedRoom()
    {
        inRoom = true; // we're in the room

        // if all characters have not been selected show the selection menu
        if (GameManager.instance.doctorAvailable || GameManager.instance.virusAvailable)
        {
            GameManager.ShowSelectionMenu();
        }
    }
}