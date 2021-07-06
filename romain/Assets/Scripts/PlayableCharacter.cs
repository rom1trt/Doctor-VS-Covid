using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlaybleCharacter : MonoBehaviour
{
    public float speed = 6f; // speed of movement
    public float actionDelay = 1f; // action animation duration
    public float actionRange = 1f; // action range

    public string actionButtonText;
    public Color actionButtonColor = Color.white;

    //action button
    private Button actionButton;
    private CanvasGroup buttonGroup;
    private Text buttonText;
    private Image buttonImage;
    public Sprite buttonSprite;

    [HideInInspector]
    public bool isWalking = false; // is the player walking
    [HideInInspector]
    public bool canWalk = true; // can the player walk

    CameraController cameraController;
    Transform cam; // handles movement of the camera
    CharacterController controller; // component to move the player
    Animator anim; // animator controller
    PhotonView photonView;

    void Start()
    {
        cam = Camera.main.transform;
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        photonView = GetComponent<PhotonView>();
        cameraController = FindObjectOfType<CameraController>();

        // check if photon view is the actual client view
        if (photonView.IsMine)
        {
            // retrieve action button
            actionButton = GameObject.FindGameObjectWithTag("ActionButton").GetComponent<Button>();
            buttonGroup = actionButton.GetComponent<CanvasGroup>();
            buttonText = actionButton.transform.GetChild(0).GetComponent<Text>();
            buttonImage = actionButton.transform.GetChild(1).GetComponent<Image>();

            actionButton.targetGraphic.color = actionButtonColor;
            actionButton.onClick.RemoveAllListeners();
            actionButton.onClick.AddListener(() => Action()); // add event listener to the action button
            buttonText.text = actionButtonText;
            buttonImage.sprite = buttonSprite;
        }
    }

    void Update()
    {
        // if we're not the master client then do nothing
        if (!photonView.IsMine)
            return;

        cameraController.target = transform;

        anim.SetBool("walking", isWalking); // set animation walk parameter

        float horizontal = Input.GetAxis("Horizontal"); // getting horizontal input range from user
        float vertical = Input.GetAxis("Vertical");    // getting vertical input range from user

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized; // vector to store the direction of movement

        controller.Move(new Vector3(0, Physics.gravity.y, 0) * Time.deltaTime);

        if (direction.magnitude >= 0.1f && canWalk && !GameManager.ended) // if there was an input or can walk or game isn't ended
        {
            isWalking = true;

            // dealing with the rotation of the player
            float targetangle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0f, targetangle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetangle, 0f) * Vector3.forward;
            // move the player
            controller.Move(moveDirection.normalized * speed * Time.deltaTime);
        }
        else
            isWalking = false;

        // check if there is at least one citizen in range
        int rangeCount = 0;

        foreach (CitizenController citizen in GameManager.citizenPool)
            if (Vector3.Distance(transform.position, citizen.transform.position) < actionRange && citizen.state == CitizenState.None)
                rangeCount++;

        bool inRange = rangeCount > 0 && GameManager.ended == false;
        buttonGroup.alpha = inRange ? 1f : 0f;
        buttonGroup.interactable = inRange;
        buttonGroup.blocksRaycasts = inRange;
    }

    // action button callback
    public virtual void Action()
    {
        // if we're not the master client then do nothing
        if (!photonView.IsMine)
            return;

        StartCoroutine(ActionDelay());
    }

    // wait for animation
    IEnumerator ActionDelay()
    {
        canWalk = false;

        anim.SetTrigger("action");
        yield return new WaitForSeconds(actionDelay);

        canWalk = true;
    }

    // debug range
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, actionRange);
    }
}
