using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class RematchButton : MonoBehaviour
{
    public Button rematchButton;

    void Update()
    {
        // allows the rematch button only for the master client
        rematchButton.interactable = PhotonNetwork.IsMasterClient;
    }
}
