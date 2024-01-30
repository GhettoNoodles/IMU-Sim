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
    private List<Sample> CurrentSecondSamples = new List<Sample>();
    private List<Sample> LoadedAngles;

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
            int index = bits[0].LastIndexOf(":");
            bits[0] = bits[0].Substring(index - 2, 2);
            newSample.timeStampSec = int.Parse(bits[0]);

            // angles
            
            if (!float.TryParse(bits[8],NumberStyles.Any,CultureInfo.InvariantCulture, out newSample.xAngle))
            {
                Debug.Log("x-angle wrong format");
            }
            if (!float.TryParse(bits[9],NumberStyles.Any,CultureInfo.InvariantCulture,  out newSample.yAngle))
            {
                Debug.Log("y-angle wrong format");
            }if (!float.TryParse(bits[10],NumberStyles.Any,CultureInfo.InvariantCulture,  out newSample.zAngle))
            {
                Debug.Log("z-angle wrong format");
            }
            
           /* Debug.Log("Second:" + newSample.timeStampSec);
            Debug.Log("X:" + newSample.xAngle);
            Debug.Log("Y:" + newSample.yAngle);
            Debug.Log("Z:" + newSample.zAngle);*/
            if (CurrentSecondSamples.Count==0 || CurrentSecondSamples.Last().timeStampSec==newSample.timeStampSec)
            {
                CurrentSecondSamples.Add(newSample);
                Debug.Log("Added");
            }
            else
            {
                //average
                Debug.Log("Done");
            }
            
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void LoadTxt(string _path)
    {
    }
}

public class Sample
{
    public int timeStampSec;
    public float xAngle;
    public float yAngle;
    public float zAngle;
    public float AsX;
    public float AsY;
    public float AsZ;
}