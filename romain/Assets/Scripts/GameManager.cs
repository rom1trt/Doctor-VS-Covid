using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class GameManager : MonoBehaviourPun, IPunObservable
{
    public static GameManager instance; // singleton

    public static List<CitizenController> citizenPool = new List<CitizenController>(); // list of all citizens in the scene
    public static bool started = false; // is match started?
    public static bool ended = false; // is match ended?

    // characters relative variables
    public GameObject doctor;
    public Transform doctorSpawn;
    public AudioClip healAudio;
    public GameObject virus;
    public Transform virusSpawn;
    public AudioClip infectAudio;

    [Range(1, 99)]
    public int vaccinationPercentageWin = 70; // the percentage of vaccination for the doctor to win
    public Text vaccinatedText; // vaccinated text state
    public Text infectedText; // infected text state
    public GameObject endDisplay; // end display
    public Text endDisplayText; // end display title
    public GameObject playerSelectionMenu; // player selection menu

    // global characters availability values for character selection
    public bool doctorAvailable = true;
    public bool virusAvailable = true;

    void Awake()
    {
        if(instance == null)
            instance = this;
    }

    void Start()
    {
        citizenPool = FindObjectsOfType<CitizenController>().ToList(); // retrieve all citizens
    }

    void Update()
    {
        // calculate percentages and display them
        int infectedCount = citizenPool.Count(x => x.state == CitizenState.Infected);
        int vaccinatedCount = citizenPool.Count(x => x.state == CitizenState.Vaccinated);
        vaccinatedText.text = ((float)vaccinatedCount / citizenPool.Count * 100f).ToString("F0") + "% VACCINATED";
        infectedText.text = ((float)infectedCount / citizenPool.Count * 100f).ToString("F0") + "% INFECTED";

        // check if doctor has won
        if (((float)vaccinatedCount / citizenPool.Count * 100f) > vaccinationPercentageWin)
        {
            ended = true;
            endDisplay.SetActive(true);
            endDisplayText.color = new Color(0.133f, 0.54f, 0.133f, 1f);
            endDisplayText.text = "DOCTOR WON !";
        }

        // check if virus has won
        if (((float)infectedCount / citizenPool.Count * 100f) > 100 - vaccinationPercentageWin)
        {
            ended = true;
            endDisplay.SetActive(true);
            endDisplayText.color = Color.red;
            endDisplayText.text = "VIRUS WON !";
        }

        // check if draw
        if (((float)infectedCount / citizenPool.Count * 100f) == 100 - vaccinationPercentageWin && ((float)vaccinatedCount / citizenPool.Count * 100f) == vaccinationPercentageWin)
        {
            ended = true;
            endDisplay.SetActive(true);
            endDisplayText.color = Color.yellow;
            endDisplayText.text = "DRAW";
        }
    }

    // shows the character selection menu
    public static void ShowSelectionMenu()
    {
        instance.playerSelectionMenu.SetActive(true);
    }

    // starts the match
    public static void StartMatch()
    {
        started = true;
        instance.playerSelectionMenu.SetActive(false);
    }

    // function to handle the rpc
    public void PlaySound(int id, Vector3 position)
    {
        photonView.RPC("PlaySoundRPC", RpcTarget.All, id, position);
    }

    // play sound for all players
    [PunRPC]
    public void PlaySoundRPC(int id, Vector3 position)
    {
        switch (id)
        {
            case 0:
                AudioSource.PlayClipAtPoint(healAudio, position, 0.5f);
                break;
            case 1:
                AudioSource.PlayClipAtPoint(infectAudio, position, 0.5f);
                break;
        }
    }

    // function to handle the rpc
    public void SetSelectionState(int id, bool state)
    {
        photonView.RPC("SetSelectionStateRPC", RpcTarget.AllBuffered, id, state);
    }

    // set selection state for all players
    [PunRPC]
    public void SetSelectionStateRPC(int id, bool state)
    {
        switch (id)
        {
            case 0:
                instance.doctorAvailable = state;
                break;
            case 1:
                instance.virusAvailable = state;
                break;
        }
    }

    // function to handle the rpc
    public void Rematch()
    {
        photonView.RPC("RematchRPC", RpcTarget.AllBuffered);
    }

    // reload the current scene fo all players
    [PunRPC]
    public void RematchRPC()
    {
        Destroy(FindObjectOfType<DoctorController>().gameObject);
        Destroy(FindObjectOfType<VirusController>().gameObject);
        instance.doctorAvailable = true;
        instance.virusAvailable = true;
        started = false;
        ended = false;
        endDisplay.SetActive(false);
        FindObjectOfType<CharacterSelection>().selection = SelectedPlayer.None;

        // reset citizens positions and states
        foreach (CitizenController citizen in citizenPool)
        {
            citizen.SetState(CitizenState.None);
            citizen.ResetPosition();
        }

        ShowSelectionMenu();
    }

    // load the menu scene
    public void Menu()
    {
        SceneManager.LoadScene("Menu");
        PhotonNetwork.Disconnect();
    }

    // this is just to allow for IPunObservable
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
