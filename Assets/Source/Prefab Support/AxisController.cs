using UnityEngine;

public class AxisController : MonoBehaviour
{
    // Define the possible directions for the manipulator
    public enum Axis { X, Y, Z };
    
    // Public variable that must be set in the Inspector
    public Axis axisDirection;
}