#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace Polyart
{

    [CustomEditor(typeof(DecalProjector))]
    public class DecalProjectorCustomInspector : Editor
    {
        private DecalProjector decal;
        private bool painting = false;
        private float brushRadius = 0.5f;
        private float brushStrength = 0.02f;

        public override void OnInspectorGUI()
        {
            GUILayout.Space(15);

            GUIStyle largeLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 36,
                fontStyle = FontStyle.Bold,
                normal = { textColor = UnityEngine.Color.white },
                alignment = TextAnchor.MiddleCenter
            };

            GUILayout.Label("Decal", largeLabelStyle);

            EditorGUILayout.Space(20, true);

            GUIStyle medLabelStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 16,
                fontStyle = FontStyle.Normal,
                normal = { textColor = UnityEngine.Color.white },
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.LabelField("Mesh Parameters", medLabelStyle);

            EditorGUILayout.Space(2);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("subdivisionsX"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("subdivisionsZ"));
            EditorGUILayout.Space(1);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("offset"));
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(8);

            EditorGUILayout.LabelField("Material Parameters", medLabelStyle);

            EditorGUILayout.Space(2);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("material"));
            EditorGUILayout.Space(1);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("textureOpacityGradience"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("edgeOpacityCutout"));
            EditorGUILayout.EndVertical();

            GUILayout.Space(5);

            GUILayout.Space(15);
            GUIStyle headerStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("Decal Paint Tool", medLabelStyle);
            EditorGUILayout.Space(8);

            // Custom styles for enabled and disabled buttons
            GUIStyle enabledStyle = new GUIStyle(GUI.skin.button);
            enabledStyle.normal.textColor = Color.white;
            enabledStyle.fontSize = 12;
            enabledStyle.normal.background = MakeTex(2, 2, new Color(0f, 0.4f, 0f));

            GUIStyle disabledStyle = new GUIStyle(GUI.skin.button);
            disabledStyle.normal.textColor = Color.white;
            disabledStyle.normal.background = MakeTex(2, 2, new Color(0.4f, 0f, 0f));

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            brushRadius = EditorGUILayout.Slider("Brush Radius", brushRadius, 0.1f, 2f);
            brushStrength = EditorGUILayout.Slider("Brush Strength", brushStrength, 0.01f, 0.1f);
            GUILayout.Space(2);
            if (GUILayout.Button(painting ? "Painting" : "Not Painting", painting ? enabledStyle : disabledStyle))
            {
                painting = !painting;
                SceneView.RepaintAll();
            }
            GUILayout.Space(5);
            EditorGUILayout.EndVertical();

            GUILayout.Space(10);
            serializedObject.ApplyModifiedProperties();
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

        void OnEnable()
        {
            decal = (DecalProjector)target;
            SceneView.duringSceneGui += OnSceneGUI;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        void OnSceneGUI(SceneView sceneView)
        {
            if (!painting) return;

            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            Event e = Event.current;
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Transform meshTransform = decal.transform;
            Mesh mesh = decal.meshFilter.sharedMesh;

            if (RaycastMesh(ray, mesh, meshTransform, out RaycastHit hit))
            {
                Handles.color = Color.red;
                Handles.DrawWireDisc(hit.point, hit.normal, brushRadius);

                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    RaiseVertices(hit.point);
                    e.Use();
                }
            }
            SceneView.RepaintAll();
        }

        bool RaycastMesh(Ray ray, Mesh mesh, Transform meshTransform, out RaycastHit hit)
        {
            hit = new RaycastHit();
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            float closestDistance = float.MaxValue;
            bool hasHit = false;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v0 = meshTransform.TransformPoint(vertices[triangles[i]]);
                Vector3 v1 = meshTransform.TransformPoint(vertices[triangles[i + 1]]);
                Vector3 v2 = meshTransform.TransformPoint(vertices[triangles[i + 2]]);

                if (RayIntersectsTriangle(ray, v0, v1, v2, out Vector3 point, out float distance) && distance < closestDistance)
                {
                    closestDistance = distance;
                    hit.point = point;
                    hit.normal = Vector3.Cross(v1 - v0, v2 - v0).normalized;
                    hasHit = true;
                }
            }
            return hasHit;
        }

        bool RayIntersectsTriangle(Ray ray, Vector3 v0, Vector3 v1, Vector3 v2, out Vector3 hitPoint, out float distance)
        {
            hitPoint = Vector3.zero;
            distance = 0f;
            Vector3 edge1 = v1 - v0;
            Vector3 edge2 = v2 - v0;
            Vector3 h = Vector3.Cross(ray.direction, edge2);
            float a = Vector3.Dot(edge1, h);
            if (a > -Mathf.Epsilon && a < Mathf.Epsilon) return false;

            float f = 1.0f / a;
            Vector3 s = ray.origin - v0;
            float u = f * Vector3.Dot(s, h);
            if (u < 0.0f || u > 1.0f) return false;

            Vector3 q = Vector3.Cross(s, edge1);
            float v = f * Vector3.Dot(ray.direction, q);
            if (v < 0.0f || u + v > 1.0f) return false;

            distance = f * Vector3.Dot(edge2, q);
            if (distance > Mathf.Epsilon)
            {
                hitPoint = ray.origin + ray.direction * distance;
                return true;
            }
            return false;
        }

        void RaiseVertices(Vector3 hitPoint)
        {
            Mesh mesh = decal.meshFilter.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            Transform meshTransform = decal.transform;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldPos = meshTransform.TransformPoint(vertices[i]);
                if (Vector3.Distance(hitPoint, worldPos) < brushRadius)
                {
                    vertices[i] += meshTransform.InverseTransformDirection(meshTransform.up) * brushStrength;
                }
            }
            mesh.vertices = vertices;
            mesh.RecalculateNormals();
        }

    }
}
#endif
