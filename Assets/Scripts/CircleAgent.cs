using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class CircleAgent : Agent
{
    Rigidbody2D rBody;
    void Start () {
        rBody = GetComponent<Rigidbody2D>();
    }

    public Transform plane;
    public override void OnEpisodeBegin()
    {
        // If the Agent fell, zero its momentum
        if (this.transform.localPosition.y < 2)
        {
            this.rBody.angularVelocity = 0;
            this.rBody.velocity = Vector2.zero;
            this.transform.localPosition = new Vector3( 0, 5f, 0);
            
            plane.rotation = Quaternion.identity;
            plane.Rotate(Vector3.forward, Random.Range(-60f, 60f));
        }

        lastTryTime = Time.time;
        // Move the target to a new spot
        // plane.localRotation = Quaternion.AngleAxis(Ra);
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(plane.localRotation);

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.y);
    }
    
    public float forceMultiplier = 10;
    public float attemptTime = 5;
    public float lastTryTime = 0;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        //controlSignal.y = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);

        // Rewards
       // float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // Reached target
        if (transform.localPosition.y > 0)
        {
            AddReward(transform.localPosition.y / 100);
           // EndEpisode();
        }

        // Fell off platform
        else if (this.transform.localPosition.y < -2)
        {
            SetReward(- 1.0f);
            EndEpisode();
        }
        
        if (Time.time - lastTryTime > attemptTime)
        {
            EndEpisode();
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
}
