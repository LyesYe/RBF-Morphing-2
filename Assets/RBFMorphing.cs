using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBFMorphing : MonoBehaviour
{
	public ControlPointSelector cpSelector;
	public float IMScale = 1.0f;
	public float sigma = 1.0f; // New parameter for Gaussian RBF
	public MeshFilter sourceMeshFilter;
	public MeshFilter destinationMeshFilter;
	private Mesh sourceMesh;
	private Mesh destinationMesh;
	[Range(0f, 1f)]
	[SerializeField] private float _slider;

	public bool morphing = false;

	private Vector3[] sourceVertices;
	private Vector3[] targetVertices;
	private Vector3[] originalVertices;
	private Vector3[] controlPointsSource;
	private Vector3[] controlPointsDestination;
	private float[] wx, wy, wz;

	public void StartMorphing()
	{
		sourceMesh = sourceMeshFilter.mesh;
		destinationMesh = destinationMeshFilter.mesh;
		sourceVertices = sourceMesh.vertices;
		originalVertices = (Vector3[])sourceVertices.Clone();
		controlPointsSource = cpSelector.GetSourceControlPoints();
		controlPointsDestination = cpSelector.GetDestinationControlPoints();

		ConstructAndSolveLinearSystem(controlPointsSource, controlPointsDestination);
		ComputeTargetVertices();
		print(originalVertices[0]);
		print(targetVertices[9]);
		morphing = true;
	}


	void Update()
	{
		if (morphing) UpdateMeshVertices(_slider);
	}

	private void UpdateMeshVertices(float t)
	{
		Vector3[] vertices = new Vector3[sourceVertices.Length];

		for (int i = 0; i < sourceVertices.Length; i++)
		{
			vertices[i] = Vector3.Lerp(originalVertices[i], targetVertices[i], t);
		}

		sourceMesh.vertices = vertices;
		sourceMesh.RecalculateNormals();
	}
	private void ComputeTargetVertices()
	{
		int numVertices = sourceVertices.Length;
		targetVertices = new Vector3[numVertices];

		for (int i = 0; i < numVertices; i++)
		{
			targetVertices[i] = originalVertices[i] + FinalDisplacement(sourceVertices[i], controlPointsSource, wx, wy, wz);
			print(targetVertices[i]);
		}
	}

	private float GaussianRBF(float distance)
	{
		return Mathf.Exp(-Mathf.Pow(distance, 2) / (2 * Mathf.Pow(sigma, 2)));
	}

	public Vector3[] ComputeDisplacements(Vector3[] sourcePoints, Vector3[] destinationPoints)
	{
		if (sourcePoints.Length != destinationPoints.Length)
		{
			Debug.LogError("Source and destination points arrays must have the same length.");
			return null;
		}

		Vector3[] displacements = new Vector3[sourcePoints.Length];

		for (int i = 0; i < sourcePoints.Length; i++)
		{
			displacements[i] = destinationPoints[i] - sourcePoints[i];
		}

		return displacements;
	}


	public void ConstructAndSolveLinearSystem(Vector3[] sourcePoints, Vector3[] destinationPoints)
	{
		int N = sourcePoints.Length;
		float[,] A = new float[N, N];
		Vector3[] displacements = ComputeDisplacements(sourcePoints, destinationPoints);
		float[,] Dx = new float[N, 3];  // Matrix to hold x, y, z displacements separately

		// Construct matrix A
		for (int i = 0; i < N; i++)
		{
			for (int j = 0; j < N; j++)
			{
				float distance = Vector3.Distance(sourcePoints[i], sourcePoints[j]);
				A[i, j] = GaussianRBF(distance);
			}
		}

		// Construct matrix Dx
		for (int i = 0; i < N; i++)
		{
			Dx[i, 0] = displacements[i].x;  // x displacement
			Dx[i, 1] = displacements[i].y;  // y displacement
			Dx[i, 2] = displacements[i].z;  // z displacement
		}

		// Solve for weights w
		wx = SolveLinearSystem(A, Dx, 0);
		wy = SolveLinearSystem(A, Dx, 1);
		wz = SolveLinearSystem(A, Dx, 2);

		// Now wx, wy, and wz hold the weights for x, y, and z displacements respectively
	}

	private float[] SolveLinearSystem(float[,] A, float[,] D, int component)
	{
		int N = A.GetLength(0);
		float[] b = new float[N];
		for (int i = 0; i < N; i++)
		{
			b[i] = D[i, component];
		}
		return GaussianElimination(A, b);
	}

	private float[] GaussianElimination(float[,] A, float[] b)
	{
		int N = A.GetLength(0);
		float[] x = new float[N];

		for (int i = 0; i < N; i++)
		{
			// Find the pivot row
			int maxRow = i;
			for (int k = i + 1; k < N; k++)
			{
				if (Mathf.Abs(A[k, i]) > Mathf.Abs(A[maxRow, i]))
				{
					maxRow = k;
				}
			}

			// Swap the rows
			for (int k = i; k < N; k++)
			{
				float tmp = A[maxRow, k];
				A[maxRow, k] = A[i, k];
				A[i, k] = tmp;
			}
			float tmpB = b[maxRow];
			b[maxRow] = b[i];
			b[i] = tmpB;

			// Eliminate column below pivot
			for (int k = i + 1; k < N; k++)
			{
				float factor = A[k, i] / A[i, i];
				b[k] -= factor * b[i];
				for (int j = i; j < N; j++)
				{
					A[k, j] -= factor * A[i, j];
				}
			}
		}

		// Back substitution
		for (int i = N - 1; i >= 0; i--)
		{
			float sum = 0;
			for (int j = i + 1; j < N; j++)
			{
				sum += A[i, j] * x[j];
			}
			x[i] = (b[i] - sum) / A[i, i];
		}

		return x;
	}

	public Vector3 FinalDisplacement(Vector3 point, Vector3[] controlPoints, float[] wx, float[] wy, float[] wz)
	{
		int N = controlPoints.Length;
		Vector3 displacement = Vector3.zero;

		for (int i = 0; i < N; i++)
		{
			float distance = Vector3.Distance(point, controlPoints[i]);
			float phi = GaussianRBF(distance);
			print(wx[0]);
			displacement.x += wx[i] * phi;
			displacement.y += wy[i] * phi;
			displacement.z += wz[i] * phi;
		}
		print("The FINAL DISPLACEMENT IS " + displacement);
		return displacement;
	}
}
