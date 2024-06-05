using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using TMPro;

public class EnvironmentController : MonoBehaviour
{   
    public SimpleMultiAgentGroup blueAgentGroup;
    public SimpleMultiAgentGroup pinkAgentGroup;
    public List<ColorAgent> agents;
    //steps
    [Header("Max Environment Steps")] public int MaxStep = 10000;
    private int resetTimer;

    //grid
    [SerializeField] Transform grid;
    private List<GameObject> children;

    //wallcolors
    [SerializeField] Material winMaterial;
    [SerializeField] Material loseMaterial;
    [SerializeField] MeshRenderer[] wallMeshRenderer;

    private int numberOfRemainingAgents;

    int blueReward;
    int pinkReward;

    //Text
    public TextMeshProUGUI scoreText;
    void Start()
    {
        blueAgentGroup = new SimpleMultiAgentGroup();
        pinkAgentGroup = new SimpleMultiAgentGroup();
        numberOfRemainingAgents = agents.Count;
        foreach (var agent in agents)
        {
            Debug.Log(agent.gameObject.tag);
            if (agent.gameObject.tag == "BlueAgent")
            {
                blueAgentGroup.RegisterAgent(agent);
            }
            else
            {
                pinkAgentGroup.RegisterAgent(agent);
            }
        }

        //get Grid
        children = new List<GameObject>();

        // Use a foreach loop to iterate through each child transform
        foreach (Transform childTransform in grid.transform)
        {
            // Add the child GameObject to the list
            children.Add(childTransform.gameObject);
        }
    }

    void FixedUpdate()
    {
        resetTimer += 1;
        if (resetTimer >= MaxStep && MaxStep > 0)
        {
            blueAgentGroup.GroupEpisodeInterrupted();
            pinkAgentGroup.GroupEpisodeInterrupted();
            foreach (MeshRenderer scrs in wallMeshRenderer)
            {
                scrs.material = loseMaterial;
            }
            ResetEnvironment();
            scoreText.text = "End of Max Step";
        }
        //hurry up penalty
        blueAgentGroup.AddGroupReward(-1f / MaxStep);
        pinkAgentGroup.AddGroupReward(-1f / MaxStep);
    }
    public void ResetEnvironment()
    {
        resetTimer = 0;
        blueReward = 0;
        pinkReward = 0;
        numberOfRemainingAgents = agents.Count;
        //reset grid
        foreach (var agent in agents)
        {
            agent.gameObject.SetActive(true);
            if (agent.gameObject.tag == "BlueAgent")
            {
                blueAgentGroup.RegisterAgent(agent);
            }
            else
            {
                pinkAgentGroup.RegisterAgent(agent);
            }
        }
        foreach (GameObject child in children)
        {
            child.GetComponent<ColorChange>().Restart();
        }
    }

    public void ColoredTile(bool areYouBlue, bool recoloredTile)
    {
        if(areYouBlue == true)
        {
            blueReward++;
            if(recoloredTile == true)
            {
                pinkReward--;
                pinkAgentGroup.AddGroupReward(-0.1f);
            }
            blueAgentGroup.AddGroupReward(0.1f);
        }
        else if(areYouBlue == false)
        {
            pinkReward++;
            if (recoloredTile == true)
            {
                blueReward--;
                blueAgentGroup.AddGroupReward(-0.1f);
            }
            pinkAgentGroup.AddGroupReward(0.1f);
        }
        
        //winning condition
        if (blueReward >= (children.Count * 0.70f))
        {
            blueAgentGroup.AddGroupReward(1f);
            pinkAgentGroup.AddGroupReward(-1f);
            foreach (MeshRenderer scrs in wallMeshRenderer)
            {
                scrs.material = winMaterial;
            }
            blueAgentGroup.EndGroupEpisode();
            pinkAgentGroup.EndGroupEpisode();
            ResetEnvironment();
            scoreText.text = "BLUE WINS!";
        }
        else if (pinkReward >= (children.Count * 0.70f))
        {
            pinkAgentGroup.AddGroupReward(1f);
            blueAgentGroup.AddGroupReward(-1f);
            foreach (MeshRenderer scrs in wallMeshRenderer)
            {
                scrs.material = winMaterial;
            }
            blueAgentGroup.EndGroupEpisode();
            pinkAgentGroup.EndGroupEpisode();
            ResetEnvironment();
            scoreText.text = "PINK WINS!";
        }
    }

    public void AgentDead(bool areYouBlue)
    {   
        numberOfRemainingAgents--;
        if(areYouBlue == true)
        {
            blueAgentGroup.AddGroupReward(-1f);
            //pinkAgentGroup.AddGroupReward(0.2f);
        }
        else if (areYouBlue == false)
        {
            pinkAgentGroup.AddGroupReward(-1f);
            //blueAgentGroup.AddGroupReward(0.2f);
        }

        if (numberOfRemainingAgents == 0)
        {
            //Debug.Log("DEAD");
            foreach (MeshRenderer scrs in wallMeshRenderer)
            {
                scrs.material = loseMaterial;
            }
            blueAgentGroup.EndGroupEpisode();
            pinkAgentGroup.EndGroupEpisode();
            ResetEnvironment();
            scoreText.text = "All Agents Died";
        }
    }
}
