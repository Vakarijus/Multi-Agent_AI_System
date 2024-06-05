using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ColorAgent : Agent
{
    //[SerializeField] private Transform targetTransform;
    [SerializeField] int agentPosZ = 6;
    [SerializeField] int agentPosX = -4;
    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float rotationSpeed = 1f;

    //wallcolors
    [SerializeField] Material winMaterial;
    [SerializeField] Material loseMaterial;
    [SerializeField] MeshRenderer[] wallMeshRenderer;

    //TileColors
    [SerializeField] Material normalColor;
    [SerializeField] Material paintedColor;

    private Rigidbody rb;

    //cooperative agents
    public EnvironmentController groupManager;
    //different movement for differet agent
    public bool useArrowKeys = false;

    bool areYouBlue;

    //Team agents
    public GameObject friend;

    //Enemy agents
    public GameObject enemy1;
    public GameObject enemy2;

    void Start()
    {
        //cooperative agents
        //groupManager = FindObjectOfType<EnvironmentController>();

        rb = GetComponent<Rigidbody>();
        if (this.gameObject.tag == "BlueAgent")
        {
            areYouBlue = true;
        }
        else
        {
            areYouBlue = false;
        }
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(agentPosX, 0.870000005f, agentPosZ);
        transform.localRotation = Quaternion.Euler(0, -90f, 0);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        
        sensor.AddObservation(friend.transform.localPosition);
        sensor.AddObservation(enemy1.transform.localPosition);
        sensor.AddObservation(enemy2.transform.localPosition);
        
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveRotate = actions.ContinuousActions[0];
        float moveForward = actions.ContinuousActions[1];

        rb.MovePosition(transform.position + transform.forward * moveForward * moveSpeed * Time.deltaTime);
        transform.Rotate(0f, moveRotate * rotationSpeed, 0f, Space.Self);

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        float moveX = 0f;
        float moveZ = 0f;

        if (useArrowKeys)
        {
            moveX = Input.GetKey(KeyCode.RightArrow) ? 1.0f : Input.GetKey(KeyCode.LeftArrow) ? -1.0f : 0f;
            moveZ = Input.GetKey(KeyCode.UpArrow) ? 1.0f : Input.GetKey(KeyCode.DownArrow) ? -1.0f : 0f;
        }
        else
        {
            moveX = Input.GetKey(KeyCode.D) ? 1.0f : Input.GetKey(KeyCode.A) ? -1.0f : 0f;
            moveZ = Input.GetKey(KeyCode.W) ? 1.0f : Input.GetKey(KeyCode.S) ? -1.0f : 0f;
        }

        actionsOut.ContinuousActions.Array[0] = moveX;
        actionsOut.ContinuousActions.Array[1] = moveZ;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Wall")
        {
            AddReward(-1f);
            this.gameObject.SetActive(false);
            groupManager.AgentDead(areYouBlue);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        bool recoloredTile;

        if (areYouBlue == true)
        {
            if (other.gameObject.tag == "Tile")
            {
                recoloredTile = false;
                other.gameObject.GetComponent<Renderer>().material = paintedColor;
                other.gameObject.tag = "BlueColoredTile";
                groupManager.ColoredTile(areYouBlue, recoloredTile);
            }
            else if (other.gameObject.tag == "PinkColoredTile")
            {
                recoloredTile = true;
                other.gameObject.GetComponent<Renderer>().material = paintedColor;
                other.gameObject.tag = "BlueColoredTile";
                groupManager.ColoredTile(areYouBlue, recoloredTile);
            }
        }

        else if (areYouBlue == false)
        {
            if (other.gameObject.tag == "Tile")
            {
                recoloredTile = false;
                other.gameObject.GetComponent<Renderer>().material = paintedColor;
                other.gameObject.tag = "PinkColoredTile";
                groupManager.ColoredTile(areYouBlue, recoloredTile);
            }
            else if (other.gameObject.tag == "BlueColoredTile")
            {
                recoloredTile = true;
                other.gameObject.GetComponent<Renderer>().material = paintedColor;
                other.gameObject.tag = "PinkColoredTile";
                groupManager.ColoredTile(areYouBlue, recoloredTile);
            }
        }
    }
}
