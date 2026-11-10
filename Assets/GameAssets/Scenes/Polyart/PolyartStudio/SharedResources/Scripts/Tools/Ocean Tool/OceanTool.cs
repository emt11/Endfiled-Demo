using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Polyart
{
    [ExecuteAlways]
    public class OceanTool : MonoBehaviour
    {
        private Vector3 prevPos;
        private Vector3 prevScale;

        [Range(1, 7)]
        public int gridSubdivisions = 5;

        private Bounds[] quadTree;
        private byte[] quadTreeCullResults;
        private Matrix4x4[] planesTRSMatrices;
        private List<Matrix4x4>[] planeRenderedMatricesPerLOD;
        private MaterialPropertyBlock materialPropertyBlock;

        private Camera cam;

        private Plane[] frustum = new Plane[6];

        private const float startingSize = 500;
        private Bounds oceanBounds, oceanBoundsWithDepth;

        public Material material;

        [Range(1, 32)]
        public int numWaves = 16;

        public WaveDirectionMode waveDirectionMode = WaveDirectionMode.LinearDirection;

        public Vector2 WavelengthMinMax = new Vector2(1f, 200f);
        public Vector2 heightMinMax = new Vector2(0.03f, 2f);
        public Vector2 OffsetMinMax = new Vector2(0f, 0.75f);
        public float shoreWPODampeningDistance = 11f;
        private float maxHeight;


        public float WavelengthFalloff = 1.0f;
        public float heightFalloff = 1.0f;
        public float OffsetFalloff = 1.0f;


        public OceanPlaneLODInfo[] planeLODInfo;
        private Mesh[] planeLODs;
        private float planeSize;
        private int numLODs;

        public Vector3 flowDirectionPosition; // Local-space ocean position
        public Texture2D flowDirectionPositionIcon;

        public float waterDepth = 50f;
        public BoxCollider boxCollider;
        private Mesh quadMesh;
        private CommandBuffer cmdBuffer;
        public Material waterLineMaterial;
        private int tempRT = Shader.PropertyToID("_TempPostFX");

        public bool debugPlanes = false;

#if BuoyancyEnabled

        public ComputeShader buoyancyComputeShader;
        private const int maxFloatingObjectsNum = 512;
        private int buoyancyKernel;
        private Dictionary<float, Transform> floatingObjects = new Dictionary<float, Transform>();
        private Vector4[] buoyancyInput, buoyancyOutput;
        private ComputeBuffer objectPositionsBuffer, buoyancyResultsBuffer;

#endif
        [System.Serializable]
        public struct OceanPlaneLODInfo
        {
            public int segments;
            public float distanceFromCamera;
        }

        public enum WaveDirectionMode
        {
            LinearDirection, 
            RadialDirection
        }

        private void Initialize()
        {
#if BuoyancyEnabled
            InitBuoyancy();
#endif
            InitMaterial();
            InitMeshes();
            InitQuadMesh();
            InitQuadTree();
            InitMatrices();
            InitCollision();
            InitUnderwaterEffects();
            cam = GetActiveCamera();
        }

        private void InitQuadTree()
        {
            int treeLength = QuadTree.GetTreeLengthAtDepthIndex(gridSubdivisions - 1);
            quadTree = new Bounds[treeLength];
            quadTreeCullResults = new byte[treeLength];

            quadTree[0] = new Bounds()
            {
                center = oceanBounds.center,
                size = oceanBounds.size
            };

            for (int di = 0; di < gridSubdivisions; ++di)
            {
                int startIndex = QuadTree.GetFirstIndexAtDepthIndex(di);
                int siblingCount = QuadTree.GetSiblingCountAtDepthIndex(di);
                for (int si = 0; si < siblingCount; ++si)
                {
                    int i = startIndex + si;
                    int firstChildIndex = QuadTree.GetFirstChildIndexOf(i);
                    if (firstChildIndex >= quadTree.Length)
                        return;

                    Bounds cell = quadTree[i];
                    Vector3 min = cell.min;
                    Vector3 max = cell.max;
                    Vector3 center = cell.center;
                    Vector3 size = cell.size;
                    Vector3 halfSize = new Vector3(size.x * 0.5f, size.y, size.z * 0.5f);

                    Bounds bottomLeftCell =
                        new Bounds(new Vector3((min.x + center.x) * 0.5f, center.y, (min.z + center.z) * 0.5f), halfSize);
                    Bounds topLeftCell =
                        new Bounds(new Vector3((min.x + center.x) * 0.5f, center.y, (center.z + max.z) * 0.5f), halfSize);
                    Bounds topRightCell =
                        new Bounds(new Vector3((center.x + max.x) * 0.5f, center.y, (center.z + max.z) * 0.5f), halfSize);
                    Bounds bottomRightCell =
                        new Bounds(new Vector3((center.x + max.x) * 0.5f, center.y, (min.z + center.z) * 0.5f), halfSize);

                    quadTree[firstChildIndex + 0] = bottomLeftCell;
                    quadTree[firstChildIndex + 1] = topLeftCell;
                    quadTree[firstChildIndex + 2] = topRightCell;
                    quadTree[firstChildIndex + 3] = bottomRightCell;
                }
            }
        }

        private void InitMaterial()
        {
            if (material == null)
            {
                Debug.LogError("Ocean Material is NOT Valid!");
                return;
            }

            Vector4[] waveData = new Vector4[32];

            System.Random rand = new System.Random(1234);

            maxHeight = 0;

            for (int i = 0; i < numWaves; i++)
            {
                float baseAlpha = 1f - (float)i / numWaves;

                // Add controlled randomness to alpha
                float randOffset = (float)(rand.NextDouble() * 2.0 - 1.0) * (0.1f /* this can be made a parameter to control randomness */ / numWaves);
                float alpha = Mathf.Clamp01(baseAlpha + randOffset);

                float wavelength = Mathf.Lerp(WavelengthMinMax.x, WavelengthMinMax.y, Mathf.Pow(alpha, WavelengthFalloff));
                float height = Mathf.Max(Mathf.Lerp(heightMinMax.x, heightMinMax.y, Mathf.Pow(alpha, heightFalloff)), 0.0001f);
                maxHeight += height;
                float offset = Mathf.Lerp(OffsetMinMax.x, OffsetMinMax.y, Mathf.Pow((float)i / numWaves, OffsetFalloff));

                waveData[i] = new Vector4(wavelength, height, offset, 0);
            }

            oceanBounds.size = new Vector3(oceanBounds.size.x, maxHeight, oceanBounds.size.z);
            Vector3 oceanBoundsWithDepthCenter = oceanBounds.center;
            oceanBoundsWithDepthCenter.y -= waterDepth / 2f;
            Vector3 oceanBoundsWithDepthSize = oceanBounds.size;
            oceanBoundsWithDepthSize.y += waterDepth;
            oceanBoundsWithDepth = new Bounds(oceanBoundsWithDepthCenter, oceanBoundsWithDepthSize);


            Vector3 flowDirectionWorldPosition = transform.TransformPoint(flowDirectionPosition);
            Vector4 FlowPivot = new Vector4(flowDirectionWorldPosition.x, flowDirectionWorldPosition.z, waveDirectionMode == WaveDirectionMode.RadialDirection ? 1 : 0, transform.position.y);

            Terrain terrain = Terrain.activeTerrain;
            if (terrain != null)
            {
                TerrainData terrainData = terrain.terrainData;
                if (terrainData != null)
                {
                    RenderTexture heightmap = terrainData.heightmapTexture;
                    if (heightmap != null)
                    {
                        Vector3 terrainPosition = terrain.GetPosition();
                        Vector3 terrainSize = terrainData.size;
                        Vector4 terrainPosAndSize = new Vector4(terrainPosition.x, terrainPosition.z, terrainSize.x, terrainSize.z);

                        float terrainHeight = terrainData.heightmapScale.y * 2f;


                        materialPropertyBlock = new MaterialPropertyBlock();
                        materialPropertyBlock.SetTexture("_TerrainHeightMap", heightmap);
                        materialPropertyBlock.SetVector("_TerrainPosAndSize", terrainPosAndSize);
                        materialPropertyBlock.SetFloat("_TerrainHeight", terrainHeight);


                        materialPropertyBlock.SetInt("_WaveCount", numWaves);
                        materialPropertyBlock.SetVectorArray("_WaveData", waveData);
                        materialPropertyBlock.SetVector("_FlowPivot", FlowPivot);

                        materialPropertyBlock.SetFloat("_ShoreDistanceWPODampening", shoreWPODampeningDistance);

#if BuoyancyShader
                        buoyancyComputeShader.SetTexture(buoyancyKernel, "_TerrainHeightMap", heightmap);
                        buoyancyComputeShader.SetVector("_TerrainPositionAndSize", terrainPosAndSize);
                        buoyancyComputeShader.SetFloat("_TerrainHeight", terrainHeight);

                        buoyancyComputeShader.SetInt("_WaveCount", numWaves);
                        buoyancyComputeShader.SetVectorArray("_WaveData", waveData);
                        buoyancyComputeShader.SetVector("_FlowPivot", FlowPivot);

                        buoyancyComputeShader.SetFloat("_ShoreDistanceWPODampening", shoreWPODampeningDistance);
#endif
                        SetWindDirection();

                        Invoke("InitReflectionProbe", 2f);
                    }
                }
            }

            // Assign to all child renderers
            foreach (var renderer in GetComponentsInChildren<MeshRenderer>())
            {
                renderer.sharedMaterial = material;
            }
        }

        private void InitReflectionProbe()
        {
            ReflectionProbe probe = FindObjectOfType<ReflectionProbe>();
            if (probe != null)
            {

                switch (probe.mode)
                {
                    case ReflectionProbeMode.Baked:
                        Texture cubemapBaked = probe.bakedTexture;
                        if (cubemapBaked != null)
                            materialPropertyBlock.SetTexture("_ReflectionProbeTexture", cubemapBaked);
                        else
                            Debug.LogWarning("Baked Reflection Probe Texture NOT Valid!");
                        break;
                    case ReflectionProbeMode.Realtime:
                        RenderTexture cubemapRealTime = probe.realtimeTexture;
                        if (cubemapRealTime != null)
                            materialPropertyBlock.SetTexture("_ReflectionProbeTexture", cubemapRealTime);
                        else
                            Debug.LogWarning("Real Time Reflection Probe Texture NOT Valid!");
                        break;
                    case ReflectionProbeMode.Custom:
                        Texture cubemapCustomBaked = probe.customBakedTexture;
                        if (cubemapCustomBaked != null)
                            materialPropertyBlock.SetTexture("_ReflectionProbeTexture", cubemapCustomBaked);
                        else
                            Debug.LogWarning("Custom Reflection Probe Texture NOT Valid!");
                        break;
                }
            }
        }

        private void SetWindDirection()
        {
            if (materialPropertyBlock == null)
                return;

            Vector3 flowDirectionWorldPosition = transform.TransformPoint(flowDirectionPosition);
            Vector3 windDirectionVector = flowDirectionWorldPosition - transform.position;
            windDirectionVector.y = 0;
            windDirectionVector = windDirectionVector.normalized;

            Vector4 windDirection = new Vector4(-windDirectionVector.x, -windDirectionVector.z, transform.position.x, transform.position.z);

            materialPropertyBlock.SetVector("_WindDirection", windDirection);

#if BuoyancyEnabled
            buoyancyComputeShader.SetVector("_WindDirection", windDirection);
#endif
        }

        public void SetHandlePos(Vector3 newPos)
        {
            flowDirectionPosition = newPos;

            if (materialPropertyBlock == null)
                return;

            Vector3 flowDirectionWorldPosition = transform.TransformPoint(flowDirectionPosition);
            materialPropertyBlock.SetVector("_FlowPivot", new Vector4(flowDirectionWorldPosition.x, flowDirectionWorldPosition.z, waveDirectionMode == WaveDirectionMode.RadialDirection ? 1 : 0, transform.position.y));
            SetWindDirection();
        }

        private void InitMeshes()
        {
            if (planeLODInfo == null)
                return;

            planeSize = oceanBounds.size.x / Mathf.Pow(2, gridSubdivisions - 1f);

            numLODs = planeLODInfo.Length;

            planeLODs = new Mesh[numLODs];

            for (int i = 0; i < numLODs; i++)
            {
                planeLODs[i] = GeneratePlane(i);
            }
        }

        private void InitQuadMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[] {
                new Vector3(-1, -1, 0),
                new Vector3( 1, -1, 0),
                new Vector3(-1,  1, 0),
                new Vector3( 1,  1, 0),
            };
                    mesh.uv = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
            };
            mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
            mesh.RecalculateNormals();

            mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000); // Prevent frustum culling

            quadMesh = mesh;
        }

        private void InitMatrices()
        {
            int startIndex = QuadTree.GetFirstIndexAtDepthIndex(gridSubdivisions - 1);
            int siblingCount = QuadTree.GetSiblingCountAtDepthIndex(gridSubdivisions - 1);

            planesTRSMatrices = new Matrix4x4[siblingCount];

            for (int si = 0; si < siblingCount; ++si)
            {
                Bounds bounds = quadTree[si + startIndex];
                Vector3 pos = bounds.center;
                pos.y = transform.position.y;
                Matrix4x4 currMatrix = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one);
                planesTRSMatrices[si] = currMatrix;
            }

            planeRenderedMatricesPerLOD = new List<Matrix4x4>[numLODs];
        }

        private void InitCollision()
        {
            if (boxCollider == null)
            {
                boxCollider = gameObject.AddComponent<BoxCollider>();
            }
            Vector3 scale = transform.lossyScale;
            Vector3 boundsCenter = transform.InverseTransformPoint(oceanBoundsWithDepth.center);
            Vector3 boundsSize = transform.InverseTransformVector(oceanBoundsWithDepth.size);
            boxCollider.center = boundsCenter;//new Vector3(0f,- (waterDepth * 0.5f /scale.y), 0f);
            boxCollider.size = boundsSize;//new Vector3(oceanBounds.size.x/scale.x, (maxHeight + waterDepth) / scale.y, oceanBounds.size.z/scale.z);
        }

        private void InitUnderwaterEffects()
        {
            CreateFullScreenQuad();
            SetupCommandBuffer();
        }

        private Mesh GeneratePlane(int lod)
        {
            Mesh mesh = new Mesh();

            int segments = planeLODInfo[lod].segments;

            int vertCount = segments + 1;
            Vector3[] vertices = new Vector3[vertCount * vertCount];
            Vector2[] uv = new Vector2[vertices.Length];
            Vector3[] normals = new Vector3[vertices.Length];
            Vector4[] tangents = new Vector4[vertices.Length];
            int[] triangles = new int[segments * segments * 6];

            float step = planeSize / segments;

            for (int z = 0; z < vertCount; z++)
            {
                for (int x = 0; x < vertCount; x++)
                {
                    int i = z * vertCount + x;
                    float posX = x * step - planeSize / 2;
                    float posZ = z * step - planeSize / 2;
                    vertices[i] = new Vector3(posX, 0, posZ);
                    uv[i] = new Vector2((float)x / segments, (float)z / segments);
                }
            }

            int triIndex = 0;
            for (int z = 0; z < segments; z++)
            {
                for (int x = 0; x < segments; x++)
                {
                    int i = z * vertCount + x;

                    triangles[triIndex++] = i;
                    triangles[triIndex++] = i + vertCount;
                    triangles[triIndex++] = i + 1;

                    triangles[triIndex++] = i + 1;
                    triangles[triIndex++] = i + vertCount;
                    triangles[triIndex++] = i + vertCount + 1;
                }
            }

            Vector3 normal = Vector3.up; // (0, 1, 0)
            Vector4 tangent = new Vector4(1, 0, 0, 1); // (1, 0, 0), w = 1

            for (int i = 0; i < vertices.Length; i++)
            {
                normals[i] = normal;
                tangents[i] = tangent;
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.normals = normals;
            mesh.tangents = tangents;
            mesh.triangles = triangles;

            return mesh;
        }

#if BuoyancyEnabled
        private void InitBuoyancy()
        {
            floatingObjects = new Dictionary<float, Transform>();
            buoyancyInput = new Vector4[maxFloatingObjectsNum];
            buoyancyOutput = new Vector4[maxFloatingObjectsNum];

            buoyancyKernel = buoyancyComputeShader.FindKernel("BuoyancyCalculation");

            if (objectPositionsBuffer != null )
                objectPositionsBuffer.Release();
            if (buoyancyResultsBuffer != null )
                buoyancyResultsBuffer.Release();

            objectPositionsBuffer = new ComputeBuffer(maxFloatingObjectsNum, sizeof(float) * 4);
            buoyancyResultsBuffer = new ComputeBuffer(maxFloatingObjectsNum, sizeof(float) * 4);

            buoyancyComputeShader.SetBuffer(buoyancyKernel, "results", buoyancyResultsBuffer);
        }
#endif

#if UNITY_EDITOR
        private void OnValidate()
        {
#if BuoyancyEnabled
            InitBuoyancy();
#endif
            InitMaterial();
            InitCollision();
        }
#endif
        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            Vector3 currPos = transform.position;
            Vector3 currScale = transform.localScale;

            if (prevScale != currScale || prevPos != currPos)
            {
                // Update bounds
                oceanBounds = new Bounds(currPos, new Vector3(startingSize * currScale.x, maxHeight, startingSize * currScale.z));

                prevPos = currPos;
                prevScale = currScale;

                Initialize();
            }
#if UNITY_EDITOR
            cam = GetActiveCamera();
#endif
            GeometryUtility.CalculateFrustumPlanes(cam, frustum);
            CullCells();
            RenderOceanPlanes();

            bool isCurrentlyUnderwater = boxCollider.bounds.Contains(cam.transform.position);

            quadMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000); // Prevent frustum culling

            if (isCurrentlyUnderwater)
            {
                // Draw with MaterialPropertyBlock
                Graphics.DrawMesh(
                    quadMesh,
                    Matrix4x4.identity,
                    waterLineMaterial,
                    0,
                    cam,
                    0,
                    materialPropertyBlock,
                    false,
                    false,
                    false
                );
            }
#if BuoyancyEnabled
            buoyancyInput = new Vector4[maxFloatingObjectsNum];
            int counter = 0;
            foreach (var floatingObject in floatingObjects)
            {
                Vector3 pos = floatingObject.Value.position;
                float id = floatingObject.Key;

                buoyancyInput[counter] = new Vector4(pos.x, pos.y, pos.z, id);

                counter++;
            }

            objectPositionsBuffer.SetData( buoyancyInput );
            buoyancyComputeShader.SetBuffer(buoyancyKernel, "objectInputData", objectPositionsBuffer);
            buoyancyComputeShader.SetFloat("_Time", Time.time);

            buoyancyComputeShader.Dispatch(buoyancyKernel, maxFloatingObjectsNum / 64, 1, 1);

            // Request async readback (schedules it — not immediate!)
            AsyncGPUReadback.Request(buoyancyResultsBuffer, HandleBuoyancy);
#endif
        }

#if BuoyancyEnabled
        private void HandleBuoyancy(AsyncGPUReadbackRequest req)
        {
            if (!req.hasError)
            {
                var result = req.GetData<Vector4>().ToArray();
                buoyancyOutput = result;
            }
            else
            {
                Debug.Log("Buoyancy GPU Callback NOT Valid!");
            }
        }
#endif

        public Camera GetActiveCamera()
        {
            Camera activeCamera = null;

            // Check if we're in the Unity Editor (not in Play mode)
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                // Get the SceneView camera (Editor-only)
                activeCamera = SceneView.lastActiveSceneView.camera;
                if (activeCamera != null)
                {
                    return activeCamera;
                }
                else
                {
                    Debug.Log("Scene view Invalid Camera");
                }
            }
#endif

            // Fallback to MainCamera (works in Play mode and builds)
            activeCamera = Camera.main; // Uses the "MainCamera" tagged camera
            if (activeCamera == null)
            {
                // If no MainCamera, find any active camera
                activeCamera = Camera.current; // Current rendering camera (less reliable)
                if (activeCamera == null)
                    activeCamera = Object.FindAnyObjectByType<Camera>(); // Last resort
            }

            return activeCamera;
        }

        private void CullCells()
        {
            Bounds cell0 = quadTree[0];
            quadTreeCullResults[0] = TestFrustumAABB(frustum, ref cell0);

            for (int di = 0; di < gridSubdivisions; ++di)
            {
                int startIndex = QuadTree.GetFirstIndexAtDepthIndex(di);
                int siblingCount = QuadTree.GetSiblingCountAtDepthIndex(di);
                for (int si = 0; si < siblingCount; ++si)
                {
                    int i = startIndex + si;
                    int firstChildIndex = QuadTree.GetFirstChildIndexOf(i);
                    if (firstChildIndex >= quadTree.Length)
                        return;

                    byte selfCullResult = quadTreeCullResults[i];
                    if (selfCullResult == CullFlags.CULLED)
                    {
                        quadTreeCullResults[firstChildIndex + 0] = CullFlags.CULLED;
                        quadTreeCullResults[firstChildIndex + 1] = CullFlags.CULLED;
                        quadTreeCullResults[firstChildIndex + 2] = CullFlags.CULLED;
                        quadTreeCullResults[firstChildIndex + 3] = CullFlags.CULLED;
                    }
                    else if (selfCullResult == CullFlags.VISIBLE)
                    {
                        quadTreeCullResults[firstChildIndex + 0] = CullFlags.VISIBLE;
                        quadTreeCullResults[firstChildIndex + 1] = CullFlags.VISIBLE;
                        quadTreeCullResults[firstChildIndex + 2] = CullFlags.VISIBLE;
                        quadTreeCullResults[firstChildIndex + 3] = CullFlags.VISIBLE;
                    }
                    else
                    {
                        quadTreeCullResults[firstChildIndex + 0] = TestFrustumAABB(frustum, ref quadTree[firstChildIndex + 0]);
                        quadTreeCullResults[firstChildIndex + 1] = TestFrustumAABB(frustum, ref quadTree[firstChildIndex + 1]);
                        quadTreeCullResults[firstChildIndex + 2] = TestFrustumAABB(frustum, ref quadTree[firstChildIndex + 2]);
                        quadTreeCullResults[firstChildIndex + 3] = TestFrustumAABB(frustum, ref quadTree[firstChildIndex + 3]);
                    }
                }
            }
        }

        private void RenderOceanPlanes()
        {
            int startIndex = QuadTree.GetFirstIndexAtDepthIndex(gridSubdivisions - 1);
            int siblingCount = QuadTree.GetSiblingCountAtDepthIndex(gridSubdivisions - 1);

            if (planeRenderedMatricesPerLOD == null)
            {
                Initialize();
                return;
            }

            for (int i = 0; i < numLODs; ++i)
            {
                planeRenderedMatricesPerLOD[i] = new List<Matrix4x4>();
            }

            for (int si = 0; si < siblingCount; ++si)
            {
                int i = startIndex + si;

                if (quadTreeCullResults[i] != CullFlags.CULLED)
                {
                    for (int j = 0; j < numLODs; ++j)
                    {
                        if (Vector3.Distance(quadTree[i].center, cam.transform.position) < planeLODInfo[j].distanceFromCamera)
                        {
                            planeRenderedMatricesPerLOD[j].Add(planesTRSMatrices[si]);
                            break;
                        }
                    }
                }
            }
            for (int j = 0; j < numLODs; ++j)
            {
                if (planeRenderedMatricesPerLOD[j].Count > 0)
                {
                    Graphics.DrawMeshInstanced(planeLODs[j], 0, material, planeRenderedMatricesPerLOD[j], materialPropertyBlock, ShadowCastingMode.Off, true, LayerMask.NameToLayer("Water"), cam, LightProbeUsage.BlendProbes);
                }
            }
        }

#if UNITY_EDITOR
        void OnEnable()
        {
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.update += EditorUpdate;
        }

        void OnDisable()
        {
            if (!Application.isPlaying)
                UnityEditor.EditorApplication.update -= EditorUpdate;
        }

        private void EditorUpdate()
        {
            Update();
        }

#endif
        private byte TestFrustumAABB(Plane[] frustum, ref Bounds b)
        {
            return CullingUtililies.TestFrustumAABB(ref frustum[0], ref frustum[1], ref frustum[2], ref frustum[3], ref frustum[4], ref frustum[5], ref b);
        }

        private void CreateFullScreenQuad()
        {
            // A simple quad that spans the screen in clip space
            quadMesh = new Mesh();
            quadMesh.vertices = new Vector3[]
            {
            new Vector3(-1, -1, 0),
            new Vector3(1, -1, 0),
            new Vector3(1, 1, 0),
            new Vector3(-1, 1, 0)
            };

            quadMesh.uv = new Vector2[]
            {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
            };

            quadMesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
            quadMesh.RecalculateBounds();
        }

        private void SetupCommandBuffer()
        {
            cmdBuffer = new CommandBuffer { name = "FullScreenEffectBuffer" };

            // Create a temporary texture, blit with material, then blit back
            cmdBuffer.GetTemporaryRT(tempRT, -1, -1, 0, FilterMode.Bilinear);
            cmdBuffer.Blit(BuiltinRenderTextureType.CameraTarget, tempRT);
            cmdBuffer.Blit(tempRT, BuiltinRenderTextureType.CameraTarget, waterLineMaterial);
            cmdBuffer.ReleaseTemporaryRT(tempRT);
        }

#if BuoyancyEnabled
        private float GetObjectID(GameObject go)
        {
            int id = go.GetInstanceID();
            return (id & 0x7FFFFFFF) / 1000000f;
        }
#endif
        void OnTriggerEnter(Collider other)
        {
#if BuoyancyEnabled
            if (other.GetComponent<Rigidbody>())
            {
                float idFloat = GetObjectID(other.gameObject);

                if (!floatingObjects.ContainsKey(idFloat))
                {
                    floatingObjects.Add(idFloat, other.transform);
                    Debug.Log($"Object {other.name} entered ocean with id {idFloat}");
                }
            }
#endif
        }

        void OnTriggerExit(Collider other)
        {
#if BuoyancyEnabled
            if (other.GetComponent<Rigidbody>())
            {
                float idFloat = GetObjectID(other.gameObject);

                floatingObjects.Remove(idFloat);
            }
#endif
        }

        void OnDestroy()
        {
#if BuoyancyEnabled
            objectPositionsBuffer.Release();
            buoyancyResultsBuffer.Release();
#endif
        }

        private void OnDrawGizmosSelected()
        {
            if (quadTree == null || quadTree.Length == 0)
                return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(oceanBoundsWithDepth.center, oceanBoundsWithDepth.size);

            if (!debugPlanes) return;

            int startIndex = QuadTree.GetFirstIndexAtDepthIndex(gridSubdivisions - 1);
            int count = QuadTree.GetSiblingCountAtDepthIndex(gridSubdivisions -1);

            for (int i = 0; i < count; ++i)
            {
                byte cullResult = quadTreeCullResults[startIndex + i];
                Gizmos.color = cullResult == CullFlags.CULLED ? UnityEngine.Color.red : UnityEngine.Color.green;

                //Gizmos.color = Color.yellow;
                Bounds b = quadTree[startIndex + i];
                Gizmos.DrawWireCube(b.center, b.size);
            }
        }

        private void OnDrawGizmos()
        {
#if BuoyancyEnabled
            if (buoyancyOutput != null)
            {
                for (int i = 0; i < buoyancyOutput.Length; i++)
                {
                    Vector4 data = buoyancyOutput[i];

                    float id = data.w;
                    if (floatingObjects.TryGetValue(id, out var floatingObject))
                    {
                        Vector3 pos = floatingObject.position;
                        pos.y = data.z;

                        Gizmos.DrawWireSphere(pos, 2f);
                        Debug.Log(floatingObject.name);
                    }
                }
            }
            else
            {
                Debug.Log("Buoyancy Output is NOT Valid!");
            }
#endif
        }
    }

#if UNITY_EDITOR

    [CustomEditor(typeof(OceanTool))]
    public class OceanToolEditor : Editor
    {
        bool isHovering = false;
        bool showMeshSettings = false;
        bool showWaveSettings = false;

        void OnSceneGUI()
        {
            OceanTool ocean = (OceanTool)target;

            Vector3 basePos = ocean.transform.position;
            Vector3 handlePos = ocean.transform.TransformPoint( ocean.flowDirectionPosition );
            handlePos.y = basePos.y + 10f;

            Vector2 guiPoint = HandleUtility.WorldToGUIPoint(handlePos);
            float size = 64f;
            Rect iconRect = new Rect(guiPoint.x - size / 2, guiPoint.y - size / 2, size, size);

            Event guiEvent = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            EventType type = guiEvent.GetTypeForControl(controlID);

            // Hover detection
            isHovering = iconRect.Contains(guiEvent.mousePosition);

            switch (type)
            {
                case EventType.MouseDown:
                    if (isHovering && guiEvent.button == 0)
                    {
                        GUIUtility.hotControl = controlID;
                        guiEvent.Use();
                    }
                    break;

                case EventType.MouseDrag:
                    if (GUIUtility.hotControl == controlID)
                    {
                        Ray ray = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
                        Plane plane = new Plane(Vector3.up, basePos + Vector3.up * 10f);
                        if (plane.Raycast(ray, out float dist))
                        {
                            Vector3 worldPos = ray.GetPoint(dist);
                            Undo.RecordObject(ocean, "Drag Ocean Handle");
                            ocean.SetHandlePos(ocean.transform.InverseTransformPoint(worldPos));
                        }
                        guiEvent.Use();
                    }
                    break;

                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID)
                    {
                        GUIUtility.hotControl = 0;
                        guiEvent.Use();
                    }
                    break;
            }

            // Draw icon with hover tint
            if (ocean.flowDirectionPositionIcon != null)
            {
                Handles.BeginGUI();

                if (isHovering)
                    GUI.color = new Color(1, 1, 1, 1f); // slight highlight
                else
                    GUI.color = new Color(1, 1, 1, 0.8f); // dim when not hovered

                GUI.DrawTexture(iconRect, ocean.flowDirectionPositionIcon, ScaleMode.ScaleToFit, true);

                GUI.color = Color.white; // reset
                Handles.EndGUI();
            }

            // Draw helper line
            Handles.color = isHovering ? Color.yellow : new Color(0f, 1f, 1f, 0.25f);
            Handles.DrawLine(basePos, handlePos);
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
                pix[i] = col;

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private bool CenteredFoldout(bool isExpanded, string label, GUIStyle labelStyle, float height = 24f)
        {
            Rect foldoutRect = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));
            Rect arrowRect = new Rect(foldoutRect.x, foldoutRect.y, 20, foldoutRect.height);

            // Draw arrow (no label)
            isExpanded = EditorGUI.Foldout(arrowRect, isExpanded, GUIContent.none, true);

            // Handle full-width click
            Event e = Event.current;
            if (e.type == EventType.MouseDown && foldoutRect.Contains(e.mousePosition))
            {
                if (!arrowRect.Contains(e.mousePosition))
                {
                    isExpanded = !isExpanded;
                    e.Use();
                }
            }

            // Draw centered label
            GUI.Label(foldoutRect, label, labelStyle);

            return isExpanded;
        }

        public override void OnInspectorGUI()
        {
            OceanTool ocean = (OceanTool)target;

            GUILayout.Space(15);

            GUIStyle largeLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 36,
                fontStyle = FontStyle.Bold,
                normal = { textColor = UnityEngine.Color.white },
                alignment = TextAnchor.MiddleCenter
            };

            GUILayout.Label("Ocean Tool", largeLabelStyle);

            EditorGUILayout.Space(20, true);

            GUIStyle medLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Normal,
                normal = { textColor = UnityEngine.Color.white },
                alignment = TextAnchor.MiddleCenter
            };

            GUIStyle smallLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Normal,
                wordWrap = true, // Enable text wrapping
                normal = { textColor = new UnityEngine.Color(0.85f, 0.85f, 0.85f) },
                alignment = TextAnchor.UpperLeft // Better alignment for wrapped text
            };

            GUIStyle smallLabelStyleCenter = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                wordWrap = true, // Enable text wrapping
                normal = { textColor = new UnityEngine.Color(1f, 1f, 1f) },
                alignment = TextAnchor.MiddleCenter
            };

            GUIStyle smallBoldLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                wordWrap = true, // Enable text wrapping
                normal = { textColor = new UnityEngine.Color(0.95f, 0.95f, 0.95f) },
                alignment = TextAnchor.UpperLeft // Better alignment for wrapped text
            };

            // Custom styles for enabled and disabled buttons
            GUIStyle enabledStyle = new GUIStyle(GUI.skin.button);
            enabledStyle.normal.textColor = Color.white;
            enabledStyle.fontSize = 12;
            enabledStyle.normal.background = MakeTex(2, 2, new Color(0f, 0.4f, 0f));

            GUIStyle disabledStyle = new GUIStyle(GUI.skin.button);
            disabledStyle.normal.textColor = Color.white;
            disabledStyle.normal.background = MakeTex(2, 2, new Color(0.4f, 0f, 0f));

            // Create a box or area for the text to constrain width
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label(
                "This is a tool that helps you create any type of Ocean. It provides controls to help you Art Direct and Optimize your Ocean based on your needs.",
                smallLabelStyle
            );
            EditorGUILayout.EndVertical();

            GUILayout.Space(15);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);

            showMeshSettings = CenteredFoldout(showMeshSettings, "Mesh Settings", medLabelStyle);

            GUILayout.Space(5);
            if (showMeshSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField(
                    "The Ocean consists of many smaller subdivided planes placed on a grid. You can add your own lod settings depending on your needs.",
                    smallLabelStyle
                );
                EditorGUI.indentLevel--;

                GUILayout.Space(10);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(5);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("gridSubdivisions"), new GUIContent("Num Grid Subdivisions"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("debugPlanes"), new GUIContent("Debug Visualize Grid"));
                EditorGUILayout.Space(5);
                SerializedProperty lodList = serializedObject.FindProperty("planeLODInfo");
                for (int i=0; i< ocean.planeLODInfo.Length; i++)
                {
                    SerializedProperty lod = lodList.GetArrayElementAtIndex(i);
                    SerializedProperty segments = lod.FindPropertyRelative("segments");
                    SerializedProperty distance = lod.FindPropertyRelative("distanceFromCamera");

                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField($"LOD {i}", GUILayout.Width(40));

                    GUILayout.FlexibleSpace();

                    EditorGUILayout.LabelField("Segments", GUILayout.Width(65));
                    segments.intValue = EditorGUILayout.IntField(segments.intValue, GUILayout.Width(50));

                    GUILayout.FlexibleSpace();

                    EditorGUILayout.LabelField("Distance", GUILayout.Width(60));
                    distance.floatValue = EditorGUILayout.FloatField(distance.floatValue, GUILayout.Width(60));

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("-", GUILayout.Width(25)))
                    {
                        lodList.DeleteArrayElementAtIndex(i);
                        break;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("+ Add LOD", GUILayout.Width(120)))
                {
                    lodList.InsertArrayElementAtIndex(lodList.arraySize);
                    SerializedProperty newLOD = lodList.GetArrayElementAtIndex(lodList.arraySize - 1);
                    newLOD.FindPropertyRelative("segments").intValue = 32;
                    newLOD.FindPropertyRelative("distanceFromCamera").floatValue = 100f;
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(15);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(15);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Space(5);

            showWaveSettings = CenteredFoldout(showWaveSettings, "Waves Settings", medLabelStyle);

            GUILayout.Space(5);
            if (showWaveSettings)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField(
                    "The selected number of waves are blended to create the final result. Values are assigned to each wave independently based on the Min and Max values. Wave number 0 has the Min value and the last wave has the Max value.",
                    smallLabelStyle
                );
                EditorGUI.indentLevel--;

                GUILayout.Space(10);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                GUILayout.Space(5);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("numWaves"), new GUIContent("Num Waves"));
                EditorGUILayout.Space(5);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.Space(5);
                GUILayout.Label("Wave Length", smallLabelStyleCenter);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("WavelengthMinMax"), new GUIContent("Min Max"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("WavelengthFalloff"), new GUIContent("Falloff"));
                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(5);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.Space(5);
                GUILayout.Label("Wave Height", smallLabelStyleCenter);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("heightMinMax"), new GUIContent("Min Max"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("heightFalloff"), new GUIContent("Falloff"));
                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(5);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.Space(5);
                GUILayout.Label("Wave Offset", smallLabelStyleCenter);
                EditorGUILayout.Space(2);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OffsetMinMax"), new GUIContent("Min Max"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("OffsetFalloff"), new GUIContent("Falloff"));
                EditorGUILayout.Space(5);
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("shoreWPODampeningDistance"), new GUIContent("Shore WPO Dampening Distance"));
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("material"), new GUIContent("Water Material"));
                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("waveDirectionMode"), new GUIContent("Wave Direction Mode"));
                EditorGUILayout.Space(5);

                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(15);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(15);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}