using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;


public class FlappyAgent : Agent
{
    private FlappyScript flappy;

    public override void Initialize()
    {
        flappy = GetComponentInChildren<FlappyScript>();
    }

    public override void OnEpisodeBegin()
    {
        ResetFlappy();
    }

    // Collect observations about the environment
    public override void CollectObservations(VectorSensor sensor)
    {
        // Functionality to be added later
    }

    // Perform actions based on decisions made by the model
    public override void OnActionReceived(ActionBuffers actions)
    {
        bool doJump = actions.DiscreteActions[0] == 1;

        if (doJump)
        {
            flappy.Jump();
        }



        CheckEpisodeEndConditions();
    }

    // Optional: Manual control for testing the agent
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Functionality to be added later
    }

    private void ResetFlappy()
    {
        #TODO: should request the flappy script to reset the game state
    }

    private void CheckEpisodeEndConditions()
    {
        // Placeholder: End the episode if the bird hits the ground, a pipe, or goes out of bounds
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Pipeblank"))
        {
            AddReward(1.0f); // Reward for passing a pipe
        }
        else if (col.CompareTag("Pipe") || col.CompareTag("Floor"))
        {
            AddReward(-1.0f); // Penalty for hitting obstacles
            EndEpisode(); // End the episode
        }
    }
}
