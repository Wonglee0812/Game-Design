using UnityEngine;

public class WayPoint : MonoBehaviour
{
    public static Transform[] Points;
    private void Awake()
    {
        Points = new Transform[transform.childCount];
        for (int i = 0; i < Points.Length; i++)
        {
            Points[i] = transform.GetChild(i);
        }
    }
}
