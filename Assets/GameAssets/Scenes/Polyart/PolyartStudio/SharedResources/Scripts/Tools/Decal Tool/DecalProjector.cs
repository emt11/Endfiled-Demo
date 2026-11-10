using UnityEngine;
using System.Reflection;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Polyart
{

    public class DecalProjector : MonoBehaviour
    {
        public int subdivisionsX = 4; // X-axis subdivisions
        public int subdivisionsZ = 4; // Z-axis subdivisions
        public float offset = 0.02f;
        public Material material;
        [Range(0f, 10f)]
        [Tooltip("Values < 1 make the Opacity Mask sharper.\nValues > 1 make the Opacity Mask less sharp.")]
        public float textureOpacityGradience = 1f;
        [Range(0f, 1f)]
        [Tooltip("Controls the Opacity on the edge of the mesh where it doesnt collide with any geometry.")]
        public float edgeOpacityCutout = 0.5f;
        public LayerMask raycastLayer; // LayerMask for detecting objects below

        private Vector3 offsetVector;
        public MeshRenderer meshRenderer;
        public MeshFilter meshFilter;
        private Mesh mesh;
        private Vector3 center, bounds;
        private Vector3 prevPosition, prevScale;
        private Quaternion prevRotation;
        private float prevOffset, prevTilingX, prevTilingZ;
#if UNITY_EDITOR
        private void OnValidate()
        {
            //Hide renderer
            meshRenderer.gameObject.hideFlags = HideFlags.HideInHierarchy;

            center = transform.position;
            bounds = transform.localScale;
            offsetVector = transform.up * offset;
            if (subdivisionsX < 1) subdivisionsX = 1;
            if (subdivisionsZ < 1) subdivisionsZ = 1;


            EditorApplication.delayCall += () => GenerateMesh();

            if (material != null)
            {
                Material tempMat = new Material(material);
                tempMat.SetFloat("_Texture_Alpha_Pow", textureOpacityGradience);
                tempMat.SetFloat("_Vertex_Color_Step", edgeOpacityCutout);
                meshRenderer.material = tempMat;
            }
        }
#endif

        private void Start()
        {
            GenerateMesh();
        }

        void GenerateMesh()
        {
            if (this == null) return;

            if (!(transform.position != prevPosition || prevScale != transform.localScale || prevRotation != transform.rotation ||
                prevTilingX != subdivisionsX || prevTilingZ != subdivisionsZ || prevOffset != offset))
            {
                return;
            }

            prevPosition = transform.position;
            prevScale = transform.localScale;
            prevRotation = transform.rotation;
            prevTilingX = subdivisionsX;
            prevTilingZ = subdivisionsZ;
            prevOffset = offset;

            offsetVector = transform.up * offset;

            mesh = new Mesh();
            meshFilter.mesh = mesh;

            int vertsPerRow = subdivisionsX + 1;
            int vertsPerCol = subdivisionsZ + 1;
            int totalVerts = vertsPerRow * vertsPerCol;
            int totalTris = subdivisionsX * subdivisionsZ * 2 * 3; // 2 triangles per grid cell

            Vector3[] vertices = new Vector3[totalVerts];
            int[] triangles = new int[totalTris];
            Vector2[] uvs = new Vector2[totalVerts];
            Color[] vertexColors = new Color[totalVerts]; // New array to store colors for each vertex

            // Get the top face of the cube
            Vector3 cubeSize = transform.localScale;
            Vector3 topCenter = transform.position + transform.up * (cubeSize.y / 2f);
            Vector3 right = transform.right * cubeSize.x / 2f;
            Vector3 forward = transform.forward * cubeSize.z / 2f;

            // Generate vertices and project them downward
            for (int z = 0; z < vertsPerCol; z++)
            {
                for (int x = 0; x < vertsPerRow; x++)
                {
                    float percentX = (float)x / subdivisionsX;
                    float percentZ = (float)z / subdivisionsZ;

                    Vector3 startPoint = topCenter
                        - right + right * 2f * percentX  // X axis
                        - forward + forward * 2f * percentZ; // Z axis

                    Vector3 adjustedPoint = PerformRaycast(startPoint, out bool hit); // Adjust vertex position and get hit status

                    // Set color based on hit status
                    vertexColors[z * vertsPerRow + x] = hit ? Color.white : Color.black;

                    int index = z * vertsPerRow + x;
                    vertices[index] = transform.InverseTransformPoint(adjustedPoint); // Convert to local space
                    uvs[index] = new Vector2(percentX, percentZ);
                }
            }

            // Generate triangles
            int triIndex = 0;
            for (int z = 0; z < subdivisionsZ; z++)
            {
                for (int x = 0; x < subdivisionsX; x++)
                {
                    int bottomLeft = z * vertsPerRow + x;
                    int bottomRight = bottomLeft + 1;
                    int topLeft = bottomLeft + vertsPerRow;
                    int topRight = topLeft + 1;

                    // First Triangle
                    triangles[triIndex++] = bottomLeft;
                    triangles[triIndex++] = topLeft;
                    triangles[triIndex++] = topRight;

                    // Second Triangle
                    triangles[triIndex++] = bottomLeft;
                    triangles[triIndex++] = topRight;
                    triangles[triIndex++] = bottomRight;
                }
            }

            // Assign mesh data
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.colors = vertexColors; // Assign vertex colors to the mesh
            mesh.RecalculateNormals();

            if (material != null)
            {
                Material tempMat = new Material(material);
                tempMat.SetFloat("_Texture_Alpha_Pow", textureOpacityGradience);
                tempMat.SetFloat("_Vertex_Color_Step", edgeOpacityCutout);
                meshRenderer.material = tempMat;
            }
        }

        private Vector3 PerformRaycast(Vector3 start, out bool hitStatus)
        {
            RaycastHit hit;
            Vector3 direction = -transform.up; // Raycast downwards

            if (Physics.Raycast(start, direction, out hit, bounds.y, raycastLayer))
            {
                hitStatus = true; // Ray hit something
                return hit.point + offsetVector; // Move vertex to hit point
            }
            else
            {
                hitStatus = false; // No hit
                                   // Calculate the world position of the bottom face
                Vector3 bottomPoint = transform.InverseTransformPoint(start);
                bottomPoint.y = -(bounds.y / 2f) / bounds.y;
                bottomPoint = transform.TransformPoint(bottomPoint);
                return bottomPoint; // Move vertex to the bottom face (considering rotation)
            }
        }

#if UNITY_EDITOR
        private void ToggleSelectionGizmo(bool enabled)
        {
            Type AnnotationUtility = Type.GetType("UnityEditor.AnnotationUtility, UnityEditor");
            var ShowOutlineOption = AnnotationUtility.GetProperty("showSelectionOutline", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            ShowOutlineOption.SetValue(null, enabled);
        }
        private void OnDrawGizmosSelected()
        {
            if (meshRenderer != null)
            {
                EditorUtility.SetSelectedRenderState(meshRenderer, 0);
                ToggleSelectionGizmo(false);
            }

            center = transform.position;
            bounds = transform.localScale;

            GenerateMesh();

            Gizmos.color = Color.green;

            // Store original Gizmos matrix
            Matrix4x4 oldMatrix = Gizmos.matrix;

            // Apply object rotation & position to Gizmos
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            // Draw rotated wire cube
            Gizmos.DrawWireCube(Vector3.zero, bounds);

            // Reset Gizmos matrix to avoid affecting other objects
            Gizmos.matrix = oldMatrix;

            // Compute arrow start and end points in WORLD space
            Vector3 arrowStart = center - transform.up * (bounds.y / 2f);
            DrawArrow(arrowStart, 0.1f);
        }

        void DrawArrow(Vector3 from, float headSize)
        {
            // Compute arrow end position in WORLD space
            Vector3 to = from + -transform.up * Mathf.Clamp(bounds.y, 0.1f, 0.75f);

            // Draw main line
            Gizmos.DrawLine(from, to);

            // Compute arrowhead directions in WORLD space
            Vector3 direction = (to - from).normalized;
            Vector3 right = Quaternion.AngleAxis(150, transform.right) * direction;
            Vector3 left = Quaternion.AngleAxis(-150, transform.right) * direction;

            // Draw arrowhead lines
            Gizmos.DrawLine(to, to + right * headSize);
            Gizmos.DrawLine(to, to + left * headSize);
        }
#endif
    }
}

