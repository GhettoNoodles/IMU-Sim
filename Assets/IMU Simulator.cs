using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.UI;

public class IMUSimulator : MonoBehaviour
{
    [SerializeField] private string _path;
    [SerializeField] private int targetSamplesPerSecond;
    [SerializeField] private Vector3 offset;
    private List<Sample> CurrentSecondSamples = new List<Sample>();
    [SerializeField] private List<Sample> LoadedAngles = new List<Sample>();

    private int coCounter;
    public Transform cube;

    // Start is called before the first frame update
    void Start()
    {
        string testWord;
        string line;
        StreamReader reader = new StreamReader(_path);

        while ((line = reader.ReadLine()) != null)
        {
            Sample newSample = new Sample();
            string[] bits = line.Split('	');

            //second mark
            string[] timeBits = bits[0].Split(':');
            //int index = bits[0].LastIndexOf(":");
            bits[0] = timeBits[^2];
            newSample.timeStampSec = int.Parse(bits[0]);

            // angles

            if (!float.TryParse(bits[8], NumberStyles.Any, CultureInfo.InvariantCulture, out newSample.angles.x))
            {
                Debug.Log("x-angle wrong format");
            }

            if (!float.TryParse(bits[9], NumberStyles.Any, CultureInfo.InvariantCulture, out newSample.angles.y))
            {
                Debug.Log("y-angle wrong format");
            }

            if (!float.TryParse(bits[10], NumberStyles.Any, CultureInfo.InvariantCulture, out newSample.angles.z))
            {
                Debug.Log("z-angle wrong format");
            }

            /* Debug.Log("Second:" + newSample.timeStampSec);
             Debug.Log("X:" + newSample.xAngle);
             Debug.Log("Y:" + newSample.yAngle);
             Debug.Log("Z:" + newSample.zAngle);*/
            if (CurrentSecondSamples.Count == 0 || CurrentSecondSamples.Last().timeStampSec == newSample.timeStampSec)
            {
                CurrentSecondSamples.Add(newSample);
                Debug.Log("Added new Count: " + CurrentSecondSamples.Count);
            }
            else
            {
                //average
                Debug.Log("Done");
                CalculateAverageForSecond();
                CurrentSecondSamples.Clear();
                CurrentSecondSamples.Add(newSample);
                Debug.Log("New Count: " + CurrentSecondSamples.Count);
            }
        }

        CalculateAverageForSecond();
    }

    public void CalculateAverageForSecond()
    {
        if (CurrentSecondSamples.Count < targetSamplesPerSecond)
        {
            LoadedAngles.AddRange(CurrentSecondSamples);
            for (int i = 0; i < (targetSamplesPerSecond-CurrentSecondSamples.Count); i++)
            {
                LoadedAngles.Add(CurrentSecondSamples[CurrentSecondSamples.Count-1]);
            }
            CurrentSecondSamples.Clear();
        }
        else
        {
            double temp = CurrentSecondSamples.Count / targetSamplesPerSecond;
            int divisions = (int)Math.Round(temp);
            for (int j = 0; j < targetSamplesPerSecond; j++)
            {
                Vector3 summedAngles = new Vector3();
                for (int i = 0; i < divisions; i++)
                {
                    summedAngles += CurrentSecondSamples[0].angles;
                    CurrentSecondSamples.RemoveAt(0);
                }

                summedAngles = summedAngles / divisions;
                Sample newtemp = new Sample();
                newtemp.angles = summedAngles;
                LoadedAngles.Add(newtemp);
            }

            CurrentSecondSamples.Clear();
        }
        Debug.Log(LoadedAngles.Count);
    }

    public void Simulate()
    {
        coCounter = 0;
    
        StartCoroutine(SimulateRoutine());
        /*for (int i = 0; i < LoadedAngles.Count; i++)
        {
            Debug.Log(LoadedAngles[i].angles);            
        }*/
    }

    IEnumerator SimulateRoutine()
    {
        float waitTime = 1f/targetSamplesPerSecond;
        while (coCounter < LoadedAngles.Count)
        {
            Vector3 newAngle = LoadedAngles[coCounter].angles + offset;
            cube.rotation = Quaternion.Euler(newAngle);

            Debug.Log(
                "Rotated to: " + LoadedAngles[coCounter].angles + " counter: " + coCounter + " Wait Time: " + waitTime);
            coCounter++;
            yield return new WaitForSeconds(waitTime);
        }
        Debug.Log("DONE");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Simulate();
        }
    }
}


public class Sample
{
    public int timeStampSec;
    public Vector3 angles;
    public float AsX;
    public float AsY;
    public float AsZ;
}