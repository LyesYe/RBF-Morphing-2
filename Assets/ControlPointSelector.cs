using UnityEngine;
using System.Collections.Generic;

public class ControlPointSelector : MonoBehaviour
{
	public Camera mainCamera;
	public MeshFilter sourceMeshFilter;
	public MeshFilter destinationMeshFilter;
	public GameObject controlPointMarkerPrefab;
	public float markerSize = 0.1f;

	private Mesh sourceMesh;
	private Mesh destinationMesh;
	private List<Vector3> sourceControlPoints = new List<Vector3>();
	private List<Vector3> destinationControlPoints = new List<Vector3>();
	private bool selectingSourcePoints = true;

	void Start()
	{
		if (mainCamera == null)
		{
			mainCamera = Camera.main;
		}

		if (sourceMeshFilter != null)
		{
			sourceMesh = sourceMeshFilter.mesh;
		}

		if (destinationMeshFilter != null)
		{
			destinationMesh = destinationMeshFilter.mesh;
		}
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			SelectControlPoint();
		}
	}

	void SelectControlPoint()
	{
		Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit))
		{
			if ( hit.collider.gameObject == sourceMeshFilter.gameObject)
			{
				Vector3 hitPoint = hit.point;
				Vector3 closestVertex = FindClosestVertex(hitPoint, sourceMesh , sourceMeshFilter.gameObject.transform);
				sourceControlPoints.Add(closestVertex);
				SpawnControlPointMarker(sourceMeshFilter.gameObject.transform.TransformPoint(closestVertex));

				Debug.Log("Source Control Points Count: " + sourceControlPoints.Count);


			}
			else if ( hit.collider.gameObject == destinationMeshFilter.gameObject)
			{
				Vector3 hitPoint = hit.point;
				Vector3 closestVertex = FindClosestVertex(hitPoint, destinationMesh , destinationMeshFilter.gameObject.transform);
				destinationControlPoints.Add(closestVertex);
				SpawnControlPointMarker(destinationMeshFilter.gameObject.transform.TransformPoint(closestVertex));

				Debug.Log("Destination Control Points Count: " + destinationControlPoints.Count);


			}
		}
	}

	Vector3 FindClosestVertex(Vector3 point, Mesh mesh, Transform meshTransform)
	{
		Vector3[] vertices = mesh.vertices;
		Vector3 closestVertex = meshTransform.TransformPoint(vertices[0]);
		float minDistance = Vector3.Distance(point, closestVertex);

		foreach (Vector3 vertex in vertices)
		{
			Vector3 worldVertex = meshTransform.TransformPoint(vertex);
			float distance = Vector3.Distance(point, worldVertex);
			if (distance < minDistance)
			{
				minDistance = distance;
				closestVertex = vertex;
			}
		}
		return closestVertex;
	}


	void SpawnControlPointMarker(Vector3 position)
	{
		GameObject marker = Instantiate(controlPointMarkerPrefab, position, Quaternion.identity);
		marker.transform.localScale = Vector3.one * markerSize;
	}

	public Vector3[] GetSourceControlPoints()
	{
		return sourceControlPoints.ToArray();
	}

	public Vector3[] GetDestinationControlPoints()
	{
		return destinationControlPoints.ToArray();
	}

}
