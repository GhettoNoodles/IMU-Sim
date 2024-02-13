using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.Video;


public class IMUSimulator : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private GameObject canv;
    public VideoPlayer vid;
    
    [SerializeField] private string _path;
    [SerializeField] private int targetSamplesPerSecond;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float intensity;
    [SerializeField] private float speed;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private List<VehicleManeuver> maneuvers = new List<VehicleManeuver>();
    private List<Sample> CurrentSecondSamples = new List<Sample>();
    private List<Sample> processedSamples = new List<Sample>();
    private List<string> _maneuvers= new List<string>();
    private string selectedManeuver;
    private VehicleManeuver activeManeuver = null;
    private const float g = -9.81f;
    private int coCounter;

    public Transform cube;

    // Start is called before the first frame update
    public void SetupAndSaveData()
    {
        int j;
        foreach (var maneuver in maneuvers) //Validate TimeStamps
        {
            var testIMU = maneuver.ImuStartTime.Split(':').Concat(maneuver.ImuEndTime.Split(':')).ToArray();
            var testVid = maneuver.VidStartTime.Split(':').Concat(maneuver.VidStartTime.Split(':')).ToArray();
            for (int i = 0; i < testIMU.Length; i++)
            {
                if (!int.TryParse(testIMU[i], out j))
                {
                    Debug.LogError("IMU Time Format Invalid for " + maneuver.title);
                }
            }for (int i = 0; i < testVid.Length; i++)
            {
                if (!int.TryParse(testVid[i], out j))
                {
                    Debug.LogError("Video Time Format Invalid for " + maneuver.title);
                }
            }
        }

        StreamReader reader = new StreamReader(_path);

        while (reader.ReadLine() is { } line)
        {
            Sample newSample = new Sample();
            string[] bits = line.Split('	');

            //timestamp 
            var indexDiv = bits[0].IndexOf(' ');
            var indexDiv2 = bits[0].LastIndexOf(':');
            newSample.timeStamp = bits[0].Substring(indexDiv + 1, indexDiv2 - indexDiv - 1);
            var timeBits = bits[0].Split(':');
            newSample.timeStampSec = int.Parse(timeBits[^2]);

            if (activeManeuver == null)
            {
                foreach (var maneuver in maneuvers)
                {
                    if (newSample.timeStamp == maneuver.ImuStartTime)
                    {
                        activeManeuver = maneuver;
                        Debug.Log("Checking for next");
                    }
                }
            }

            if (activeManeuver != null)
            {
                // Load Data From textFile

                if (!float.TryParse(bits[8], NumberStyles.Any, CultureInfo.InvariantCulture, out newSample.angles.x))
                {
                    Debug.Log("x-angle wrong format");
                }

                if (!float.TryParse(bits[9], NumberStyles.Any, CultureInfo.InvariantCulture, out newSample.angles.z))
                {
                    Debug.Log("y-angle wrong format");
                }

                if (!float.TryParse(bits[10], NumberStyles.Any, CultureInfo.InvariantCulture, out newSample.angles.y))
                {
                    Debug.Log("z-angle wrong format");
                }

                if (!float.TryParse(bits[5], NumberStyles.Any, CultureInfo.InvariantCulture,
                        out newSample.acceleration.x))
                {
                    Debug.Log("x-acc wrong format");
                }

                if (!float.TryParse(bits[6], NumberStyles.Any, CultureInfo.InvariantCulture,
                        out newSample.acceleration.z))
                {
                    Debug.Log("y-acc wrong format");
                }

                if (!float.TryParse(bits[7], NumberStyles.Any, CultureInfo.InvariantCulture,
                        out newSample.acceleration.y))
                {
                    Debug.Log("z-acc wrong format");
                }
                newSample.acceleration.z -=1;
                if (CurrentSecondSamples.Count == 0 ||
                    CurrentSecondSamples.Last().timeStampSec == newSample.timeStampSec)
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
                
                if (newSample.timeStamp == activeManeuver.ImuEndTime)
                {
                    SaveManeuver();
                    Debug.Log("saved " + activeManeuver.title);
                    processedSamples.Clear();
                    CurrentSecondSamples.Clear();
                    activeManeuver = null;
                }
            }
        }
    }

    private void SaveManeuver()
    {
        var datapath = Application.persistentDataPath + "/" + activeManeuver.title + ".json";
        Debug.Log(datapath);
        Sample[] samples = new Sample[processedSamples.Count];
        for (int i = 0; i < processedSamples.Count; i++)
        {
            samples[i] = processedSamples[i];
        }

        var maneuverData = new ManeuverData
        {
            Title = activeManeuver.title,
            Samples = samples
        };


        var output = JsonConvert.SerializeObject(maneuverData);
        File.WriteAllText(datapath, output);
        _maneuvers.Add(activeManeuver.title);
        UpdateDropDown();
    }

    private void CalculateAverageForSecond() //averages data to accomodate for target Samples per second
    {
        if (CurrentSecondSamples.Count < targetSamplesPerSecond) //fill up data to target if there is not enough data
        {
            processedSamples.AddRange(CurrentSecondSamples);
            for (int i = 0; i < (targetSamplesPerSecond - CurrentSecondSamples.Count); i++)
            {
                processedSamples.Add(CurrentSecondSamples[^1]);
            }

            CurrentSecondSamples.Clear();
        }
        else //Average
        {
            double temp = CurrentSecondSamples.Count / targetSamplesPerSecond;
            var divisions = (int)Math.Round(temp);
            for (int j = 0; j < targetSamplesPerSecond; j++)
            {
                var summedAngles = new Vector3();
                var summedAcc = new Vector3();
                for (int i = 0; i < divisions; i++)
                {
                    summedAcc += CurrentSecondSamples[0].acceleration;
                    summedAngles += CurrentSecondSamples[0].angles;
                    CurrentSecondSamples.RemoveAt(0);
                }

                if (summedAcc.x >= -0.01 && summedAcc.x <= 0.01)
                {
                    summedAcc.x = 0;
                }

                if (summedAcc.y >= -0.01 && summedAcc.y <= 0.01)
                {
                    summedAcc.y = 0;
                }

                if (summedAcc.x >= -0.01 && summedAcc.x <= 0.01)
                {
                    summedAcc.z = 0;
                }

                summedAcc = summedAcc/ divisions;
                summedAngles = summedAngles/ divisions;
                var newtemp = new Sample
                {
                    acceleration =
                        new Vector3(summedAcc.x, summedAcc.y, -(summedAcc.z)) *
                        g, //IMU uses g as unit for acceleration.
                    angles = summedAngles + offset
                };
                processedSamples.Add(newtemp);
            }

            CurrentSecondSamples.Clear();
        }
    }

    public void Simulate()
    {
        canv.SetActive(false);
        coCounter = 0;
        Sample[] loadedManeuver = LoadManeuverData(selectedManeuver);
        vid.Play();
        StartCoroutine(SimulateRoutine(loadedManeuver));
    }

    private Sample[] LoadManeuverData(string maneuverTitle)
    {
        var dataPath = Application.persistentDataPath + "/" + maneuverTitle + ".json";
        string jsonData = File.ReadAllText(dataPath);
        var data = JsonConvert.DeserializeObject<ManeuverData>(jsonData);
        return data.Samples;
    }

    IEnumerator SimulateRoutine(Sample[] loadedSamples)
    {
        float waitTime = (1f / speed) / targetSamplesPerSecond;
        while (coCounter < loadedSamples.Length)
        {
            Debug.Log("ls: "+ loadedSamples.Length + " Cc: " + coCounter);
            Vector3 delta;
            if (coCounter == 0)
            {
                delta = loadedSamples[0].angles;
            }
            else
            {
                delta = (loadedSamples[coCounter - 1].angles - loadedSamples[coCounter].angles) * intensity;
            }

            Vector3 newAngle = cube.rotation.eulerAngles + (delta);
            // cube.rotation = Quaternion.Euler(newAngle);
            rb.AddRelativeForce(loadedSamples[coCounter].acceleration * intensity, ForceMode.Acceleration);
            Debug.Log(
                "Added Force in " + loadedSamples[coCounter].acceleration * intensity + " counter: " +
                coCounter / targetSamplesPerSecond + " Time " + loadedSamples[coCounter].timeStamp);
            coCounter++;
            yield return new WaitForSeconds(waitTime);
        }
        vid.Stop();
        canv.SetActive(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetupAndSaveData();
        }
    }
    //UI STuff
    private void UpdateDropDown()
    {
        if (_maneuvers.Count>0)
        {
            dropdown.ClearOptions();
            dropdown.AddOptions(_maneuvers);
            SetManeuver();
        }
    }

    public void SetManeuver()
    {
        selectedManeuver = dropdown.options[dropdown.value].text;
        Debug.Log(selectedManeuver);
    }

    public string GetManeuver()
    {
        return selectedManeuver;
    }
}




public class Sample
{
    public string timeStamp;
    public int timeStampSec;
    public Vector3 angles;
    public Vector3 acceleration;
}

public class ManeuverData
{
    [JsonProperty("Title")] public string Title;
    [JsonProperty("Samples")] public Sample[] Samples;
}