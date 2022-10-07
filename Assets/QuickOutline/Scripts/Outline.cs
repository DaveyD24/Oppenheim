//
//  Outline.cs
//  QuickOutline
//
//  Created by Chris Nolet on 3/30/18.
//  Copyright © 2018 Chris Nolet. All rights reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class Outline : MonoBehaviour
{
	static HashSet<Mesh> RegisteredMeshes = new HashSet<Mesh>();

	public EOutlineMode OutlineMode
	{
		get { return Mode; }
		set
		{
			Mode = value;
			bNeedsUpdate = true;
		}
	}

	public Color OutlineColour
	{
		get { return Colour; }
		set
		{
			Colour = value;
			bNeedsUpdate = true;
		}
	}

	public float OutlineWidth
	{
		get { return Width; }
		set
		{
			Width = value;
			bNeedsUpdate = true;
		}
	}

	[Serializable]
	class ListVector3
	{
		public List<Vector3> Data;
	}

	[SerializeField]
	EOutlineMode Mode;

	[SerializeField]
	Color Colour = Color.white;

	[SerializeField, Range(0f, 10f)]
	float Width = 2f;

	[Header("Optional")]

	[SerializeField, Tooltip("Pre-compute enabled: Per-vertex calculations are performed in the editor and serialized with the object. "
	+ "Pre-compute disabled: Per-vertex calculations are performed at runtime in Awake(). This may cause a pause for large meshes.")]
	bool bShouldPrecomputeOutline;

	[SerializeField, HideInInspector]
	List<Mesh> BakeKeys = new List<Mesh>();

	[SerializeField, HideInInspector]
	List<ListVector3> BakeValues = new List<ListVector3>();

	Renderer[] Renderers;
	Material OutlineMaskMaterial;
	Material OutlineFillMaterial;

	bool bNeedsUpdate;

	void Awake()
	{
		// Cache renderers
		Renderers = GetComponentsInChildren<Renderer>();

		// Instantiate outline materials
		OutlineMaskMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineMask"));
		OutlineFillMaterial = Instantiate(Resources.Load<Material>(@"Materials/OutlineFill"));

		OutlineMaskMaterial.name = "OutlineMask (Instance)";
		OutlineFillMaterial.name = "OutlineFill (Instance)";

		// Retrieve or generate smooth normals
		LoadSmoothNormals();

		// Apply material properties immediately
		bNeedsUpdate = true;
	}

	void OnEnable()
	{
		foreach (Renderer Renderer in Renderers)
		{
			// Append outline shaders.
			List<Material> Materials = Renderer.sharedMaterials.ToList();

			Materials.Add(OutlineMaskMaterial);
			Materials.Add(OutlineFillMaterial);

			Renderer.materials = Materials.ToArray();
		}
	}

	void OnValidate()
	{
		// Update material properties
		bNeedsUpdate = true;

		// Clear cache when baking is disabled or corrupted
		if (!bShouldPrecomputeOutline && BakeKeys.Count != 0 || BakeKeys.Count != BakeValues.Count)
		{
			BakeKeys.Clear();
			BakeValues.Clear();
		}

		// Generate smooth normals when baking is enabled
		if (bShouldPrecomputeOutline && BakeKeys.Count == 0)
		{
			Bake();
		}
	}

	void Update()
	{
		if (bNeedsUpdate)
		{
			bNeedsUpdate = false;

			UpdateMaterialProperties();
		}
	}

	void OnDisable()
	{
		foreach (Renderer Renderer in Renderers)
		{

			// Remove outline shaders.
			List<Material> Materials = Renderer.sharedMaterials.ToList();

			Materials.Remove(OutlineMaskMaterial);
			Materials.Remove(OutlineFillMaterial);

			Renderer.materials = Materials.ToArray();
		}
	}

	void OnDestroy()
	{
		// Destroy material instances
		Destroy(OutlineMaskMaterial);
		Destroy(OutlineFillMaterial);
	}

	void Bake()
	{
		// Generate smooth normals for each mesh
		HashSet<Mesh> BakedMeshes = new HashSet<Mesh>();

		foreach (MeshFilter MeshFilter in GetComponentsInChildren<MeshFilter>())
		{

			// Skip duplicates
			if (!BakedMeshes.Add(MeshFilter.sharedMesh))
			{
				continue;
			}

			// Serialise smooth normals
			List<Vector3> SmoothedNormals = SmoothNormals(MeshFilter.sharedMesh);

			BakeKeys.Add(MeshFilter.sharedMesh);
			BakeValues.Add(new ListVector3() { Data = SmoothedNormals });
		}
	}

	void LoadSmoothNormals()
	{
		// Retrieve or generate smooth normals
		foreach (MeshFilter MeshFilter in GetComponentsInChildren<MeshFilter>())
		{
			// Skip if smooth normals have already been adopted
			if (!RegisteredMeshes.Add(MeshFilter.sharedMesh))
			{
				continue;
			}

			// Retrieve or generate smooth normals
			int Index = BakeKeys.IndexOf(MeshFilter.sharedMesh);
			List<Vector3> smoothNormals = (Index >= 0) ? BakeValues[Index].Data : SmoothNormals(MeshFilter.sharedMesh);

			// Store smooth normals in UV3
			MeshFilter.sharedMesh.SetUVs(3, smoothNormals);

			// Combine sub-meshes.
			if (MeshFilter.TryGetComponent<Renderer>(out Renderer Renderer))
			{
				CombineSubmeshes(MeshFilter.sharedMesh, Renderer.sharedMaterials);
			}
		}

		// Clear UV3 on skinned mesh renderers
		foreach (SkinnedMeshRenderer SkinnedMeshRenderer in GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			// Skip if UV3 has already been reset
			if (!RegisteredMeshes.Add(SkinnedMeshRenderer.sharedMesh))
			{
				continue;
			}

			// Clear UV3
			SkinnedMeshRenderer.sharedMesh.uv4 = new Vector2[SkinnedMeshRenderer.sharedMesh.vertexCount];

			// Combine sub-meshes.
			CombineSubmeshes(SkinnedMeshRenderer.sharedMesh, SkinnedMeshRenderer.sharedMaterials);
		}
	}

	List<Vector3> SmoothNormals(Mesh Mesh)
	{
		// Group vertices by location
		IEnumerable<IGrouping<Vector3, KeyValuePair<Vector3, int>>> Groups = Mesh.vertices.Select((vertex, index) => new KeyValuePair<Vector3, int>(vertex, index)).GroupBy(pair => pair.Key);

		// Copy normals to a new list
		List<Vector3> Normals = new List<Vector3>(Mesh.normals);

		// Average normals for grouped vertices
		foreach (IGrouping<Vector3, KeyValuePair<Vector3, int>> Group in Groups)
		{
			// Skip single vertices
			if (Group.Count() == 1)
			{
				continue;
			}

			// Calculate the average normal
			Vector3 SmoothedNormal = Vector3.zero;

			foreach (KeyValuePair<Vector3, int> pair in Group)
			{
				SmoothedNormal += Normals[pair.Value];
			}

			SmoothedNormal.Normalize();

			// Assign smooth normal to each vertex
			foreach (KeyValuePair<Vector3, int> Vertex in Group)
			{
				Normals[Vertex.Value] = SmoothedNormal;
			}
		}

		return Normals;
	}

	void CombineSubmeshes(Mesh Mesh, Material[] Materials)
	{
		// Skip meshes with a single sub-mesh.
		if (Mesh.subMeshCount == 1)
		{
			return;
		}

		// Skip if sub-mesh count exceeds material count
		if (Mesh.subMeshCount > Materials.Length)
		{
			return;
		}

		// Append combined sub-mesh
		Mesh.subMeshCount++;
		Mesh.SetTriangles(Mesh.triangles, Mesh.subMeshCount - 1);
	}

	const string k_OutlineColor = "_OutlineColour";
	const string k_ZTest = "_ZTest";
	const string k_OutlineWidth = "_OutlineWidth";

	void UpdateMaterialProperties()
	{
		// Apply properties according to mode
		OutlineFillMaterial.SetColor(k_OutlineColor, Colour);

		switch (Mode)
		{
			case EOutlineMode.OutlineAll:
				OutlineMaskMaterial.SetFloat(k_ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
				OutlineFillMaterial.SetFloat(k_ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
				OutlineFillMaterial.SetFloat(k_OutlineWidth, Width);
				break;

			case EOutlineMode.OutlineVisible:
				OutlineMaskMaterial.SetFloat(k_ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
				OutlineFillMaterial.SetFloat(k_ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
				OutlineFillMaterial.SetFloat(k_OutlineWidth, Width);
				break;

			case EOutlineMode.OutlineHidden:
				OutlineMaskMaterial.SetFloat(k_ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
				OutlineFillMaterial.SetFloat(k_ZTest, (float)UnityEngine.Rendering.CompareFunction.Greater);
				OutlineFillMaterial.SetFloat(k_OutlineWidth, Width);
				break;

			case EOutlineMode.OutlineAndSilhouette:
				OutlineMaskMaterial.SetFloat(k_ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
				OutlineFillMaterial.SetFloat(k_ZTest, (float)UnityEngine.Rendering.CompareFunction.Always);
				OutlineFillMaterial.SetFloat(k_OutlineWidth, Width);
				break;

			case EOutlineMode.SilhouetteOnly:
				OutlineMaskMaterial.SetFloat(k_ZTest, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
				OutlineFillMaterial.SetFloat(k_ZTest, (float)UnityEngine.Rendering.CompareFunction.Greater);
				OutlineFillMaterial.SetFloat(k_OutlineWidth, 0f);
				break;
		}
	}
}


public enum EOutlineMode
{
	OutlineAll,
	OutlineVisible,
	OutlineHidden,
	OutlineAndSilhouette,
	SilhouetteOnly
}