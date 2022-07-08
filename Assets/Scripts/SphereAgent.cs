using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;

public class SphereAgent : Agent
{
    [SerializeField] private Text score;
    [SerializeField] private Text bestScore;
    [SerializeField] private Text time;
    Rigidbody rBody;

    private float _score = 0;
    private float _bestScore = 0;
    
    private void SetBestScore()
    {
        if (_score > _bestScore)
        {
            _bestScore = _score;
            bestScore.text = _bestScore.ToString();
        }
    }

    private void SetTime(float sec)
    {
        sec = Mathf.RoundToInt(sec);
        this.time.text = sec.ToString();
    }

    private void SetScore(float val)
    {
        _score = val;
        score.text = _score.ToString();
    }
    
    private void AddScore(float val)
    {
        _score += val;
        score.text = _score.ToString();
    }
    
    void Start () {
        rBody = GetComponent<Rigidbody>();
        SetScore(0);
    }

    public Transform plane;
    public override void OnEpisodeBegin()
    {
        SetBestScore();
        SetScore(0);
        // If the Agent fell, zero its momentum
        // if (this.transform.localPosition.y < 0)
        // {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            plane.rotation = Quaternion.identity;
            this.transform.localPosition = new Vector3( 0, 2f, 0);

            var rb = plane.GetComponent<Rigidbody>();
            rb.angularVelocity = Vector3.zero;
            rb.velocity = Vector3.zero;
            plane.rotation = Quaternion.identity;
            plane.Rotate(Vector3.forward, Random.Range(-5f, 5f));
            plane.Rotate(Vector3.right, Random.Range(-5f, 5f));
        // }

        lastTryTime = Time.time;
        SetTime(attemptTime);
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
        // Target and Agent positions
        sensor.AddObservation(this.transform.localPosition);
        //sensor.AddObservation(plane.localRotation);

        // Agent velocity
        sensor.AddObservation(rBody.velocity.x);
        sensor.AddObservation(rBody.velocity.y);
        sensor.AddObservation(rBody.velocity.z);
        
        
        //sensor.AddObservation(attemptTime - (Time.time - lastTryTime));
        
        
    }
    
    public float forceMultiplier = 30;
    public float attemptTime = 10;
    public float lastTryTime = 0;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 3
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
       // controlSignal.y = actionBuffers.ContinuousActions[1];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        
        rBody.AddForce(controlSignal * forceMultiplier);

        // Rewards
       // float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

        // Reached target
        if (transform.localPosition.y > 0)
        {
            AddReward(transform.localPosition.y / 100);
            AddScore(transform.localPosition.y / 100);
           // EndEpisode();
        }

        // Fell off platform
        else if (this.transform.localPosition.y < -1.5f)
        {
            SetReward(- 1.0f);
            EndEpisode();
        }

        if ((Time.time - lastTryTime) > attemptTime)
        {
            EndEpisode();
        }
        else
        {
            SetTime(attemptTime - (Time.time - lastTryTime));
        }
    }
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = -Input.GetAxis("Horizontal");
        continuousActionsOut[1] = -Input.GetAxis("Vertical");
    }
}
