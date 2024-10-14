using UnityEngine;
using System.Collections.Generic;

public class DrawCircularCut : MonoBehaviour
{
    private List<Vector3> cutPoints; // Stores points along the circular path
    private LineRenderer cutRender;
    private Camera cam;
    private bool isCutting;

    void Start()
    {
        cam = FindObjectOfType<Camera>();
        cutRender = GetComponent<LineRenderer>();
        cutRender.startWidth = 0.05f;
        cutRender.endWidth = 0.05f;
        cutPoints = new List<Vector3>();
    }

    void Update()
    {
        Vector3 mouse = Input.mousePosition;
        mouse.z = -cam.transform.position.z;

        if (Input.GetMouseButtonDown(0))
        {
            isCutting = true;
            cutPoints.Clear(); // Reset the list of points
            AddPointToCut(cam.ScreenToWorldPoint(mouse)); // Add the starting point
        }

        if (Input.GetMouseButton(0) && isCutting)
        {
            AddPointToCut(cam.ScreenToWorldPoint(mouse)); // Add points as the mouse moves
            UpdateCutRenderer(); // Update visual for the cut line
        }

        if (Input.GetMouseButtonUp(0) && isCutting)
        {
            isCutting = false;
            PerformCircularCut(); // Perform the cut after letting go
        }
    }

    private void AddPointToCut(Vector3 point)
    {
        if (cutPoints.Count == 0 || Vector3.Distance(cutPoints[cutPoints.Count - 1], point) > 0.1f)
        {
            cutPoints.Add(point);
        }
    }

    private void UpdateCutRenderer()
    {
        cutRender.positionCount = cutPoints.Count;
        cutRender.SetPositions(cutPoints.ToArray());
        cutRender.startColor = Color.red;
        cutRender.endColor = Color.red;
    }

    private void PerformCircularCut()
    {
        if (cutPoints.Count < 2) return; // Not enough points to make a cut

        // Loop through pairs of consecutive points and cut between them
        for (int i = 0; i < cutPoints.Count - 1; i++)
        {
            Vector3 pointA = cutPoints[i];
            Vector3 pointB = cutPoints[i + 1];

            Vector3 pointInPlane = (pointA + pointB) / 2;
            Vector3 cutPlaneNormal = Vector3.Cross((pointA - pointB), (pointA - cam.transform.position)).normalized;
            Quaternion orientation = Quaternion.FromToRotation(Vector3.up, cutPlaneNormal);

            var all = Physics.OverlapBox(pointInPlane, new Vector3(100, 0.01f, 100), orientation);

            foreach (var hit in all)
            {
                MeshFilter filter = hit.gameObject.GetComponentInChildren<MeshFilter>();
                if (filter != null)
                {
                    Cutter.Cut(hit.gameObject, pointInPlane, cutPlaneNormal);
                }
            }
        }
    }
}
