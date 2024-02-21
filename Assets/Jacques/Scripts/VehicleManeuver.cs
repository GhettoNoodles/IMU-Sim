using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Maneuver", menuName = "Vehicle Maneuvers", order = 0)]
public class VehicleManeuver : ScriptableObject
{
    public string title;
    [Header("IMU TimeStamp (hh:mm:ss)")]
    public string ImuStartTime;
    public string ImuEndTime;

    [Header("Video TimeStamp (hh:mm:ss)")] 
    public string VidStartTime;
    public string VidEndTime;
}