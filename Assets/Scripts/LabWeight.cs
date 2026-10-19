using UnityEngine;

public class LabWeight : MonoBehaviour
{
    [Tooltip("Yahan weight gram mein likho (jaise 50, 100, 250)")]
    public float massInGrams = 100f;

    public float MassInKg => massInGrams / 1000f;
}