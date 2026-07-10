using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EditableColliderTool))]
public class EditableColliderToolEditor : UnityEditor.Editor
{
    private EditableColliderTool tool;
    private int selectedPoint = -1;
    private int hoveredSegment = -1;
    private bool isEditing = false;

    private void OnEnable()
    {
        tool = (EditableColliderTool)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (isEditing)
        {
            GUILayout.Label("Click izquierdo: agrega o mueve.");
            GUILayout.Label("Click derecho: elimina punto.");
            GUILayout.Label("Shift + Click izquierdo en una linea: inserta un punto entre dos puntos.");
        }

        if (GUILayout.Button(isEditing ? "Stop Edit" : "Edit"))
        {
            isEditing = !isEditing;
            SceneView.RepaintAll();
        }

        if (GUILayout.Button("Bake"))
        {
            tool.Bake();
            isEditing = false;
            EditorUtility.SetDirty(tool);
        }

        if (GUILayout.Button("Clear Shape"))
        {
            Undo.RecordObject(tool, "Clear Shape");
            tool.ClearShape();
            EditorUtility.SetDirty(tool);
        }
    }

    private void OnSceneGUI()
    {
        if (!isEditing || tool == null)
            return;

        Event evt = Event.current;

        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        Vector3 mouseWorld = GetMouseWorldPoint(evt.mousePosition);
        if (!float.IsNegativeInfinity(mouseWorld.x))
            hoveredSegment = GetSegmentIndex(mouseWorld);
        else
            hoveredSegment = -1;

        DrawShape(evt.shift);

        switch (evt.type)
        {
            case EventType.MouseDown:
                OnMouseDown(evt);
                break;

            case EventType.MouseDrag:
                OnMouseDrag(evt);
                break;

            case EventType.MouseUp:
                selectedPoint = -1;
                break;
        }
    }

    private void DrawShape(bool showInsertPreview)
    {
        Transform tr = tool.transform;

        for (int i = 0; i < tool.points.Count; i++)
        {
            Vector3 point = tr.TransformPoint(tool.points[i]);
            Handles.color = selectedPoint == i ? Color.yellow : Color.white;
            Handles.SphereHandleCap(
                0,
                point,
                Quaternion.identity,
                HandleUtility.GetHandleSize(point) * 0.08f,
                EventType.Repaint
            );
        }

        if (tool.points.Count > 1)
        {
            for (int i = 0; i < tool.points.Count - 1; i++)
            {
                Vector3 a = tr.TransformPoint(tool.points[i]);
                Vector3 b = tr.TransformPoint(tool.points[i + 1]);

                bool isHovered = showInsertPreview && i == hoveredSegment;
                Handles.color = isHovered ? Color.cyan : Color.green;
                Handles.DrawLine(a, b);

                if (isHovered)
                {
                    Vector3 insertPos = ClosestPointOnSegment(GetMouseWorldPoint(Event.current.mousePosition), a, b);

                    Handles.color = Color.cyan;
                    Handles.SphereHandleCap(
                        0,
                        insertPos,
                        Quaternion.identity,
                        HandleUtility.GetHandleSize(insertPos) * 0.06f,
                        EventType.Repaint
                    );
                }
            }

            if (tool.isClosedShape && tool.points.Count > 2)
            {
                Vector3 a = tr.TransformPoint(tool.points[tool.points.Count - 1]);
                Vector3 b = tr.TransformPoint(tool.points[0]);

                bool isHovered = showInsertPreview && hoveredSegment == tool.points.Count - 1;
                Handles.color = isHovered ? Color.cyan : Color.green;
                Handles.DrawLine(a, b);

                if (isHovered)
                {
                    Vector3 insertPos = ClosestPointOnSegment(GetMouseWorldPoint(Event.current.mousePosition), a, b);

                    Handles.color = Color.cyan;
                    Handles.SphereHandleCap(
                        0,
                        insertPos,
                        Quaternion.identity,
                        HandleUtility.GetHandleSize(insertPos) * 0.06f,
                        EventType.Repaint
                    );
                }
            }
        }
    }

    private void OnMouseDown(Event evt)
    {
        if (evt.alt || evt.control || evt.command)
            return;

        Vector3 position = GetMouseWorldPoint(evt.mousePosition);
        if (float.IsNegativeInfinity(position.x))
            return;

        int hitPoint = GetPointIndex(position);

        if (evt.button == 0)
        {
            Undo.RecordObject(tool, "Edit Shape");

            if (evt.shift && hoveredSegment >= 0)
            {
                InsertPointOnSegment(hoveredSegment, position);
                EditorUtility.SetDirty(tool);
                evt.Use();
                return;
            }

            if (hitPoint >= 0)
            {
                selectedPoint = hitPoint;
            }
            else
            {
                tool.AddPoint(tool.transform.InverseTransformPoint(position));
                selectedPoint = tool.points.Count - 1;
                tool.Bake();
                EditorUtility.SetDirty(tool);
            }

            evt.Use();
        }
        else if (evt.button == 1 && hitPoint >= 0)
        {
            Undo.RecordObject(tool, "Remove Point");
            tool.RemovePoint(hitPoint);
            tool.Bake();
            EditorUtility.SetDirty(tool);
            evt.Use();
        }
    }

    private void OnMouseDrag(Event evt)
    {
        if (selectedPoint < 0)
            return;

        Vector3 position = GetMouseWorldPoint(evt.mousePosition);
        if (float.IsNegativeInfinity(position.x))
            return;

        Undo.RecordObject(tool, "Move Point");
        tool.points[selectedPoint] = tool.transform.InverseTransformPoint(position);
        tool.Bake();
        EditorUtility.SetDirty(tool);
        evt.Use();
    }

    private void InsertPointOnSegment(int segmentIndex, Vector3 worldPos)
    {
        Transform tr = tool.transform;

        int nextIndex = (segmentIndex + 1) % tool.points.Count;

        Vector3 a = tr.TransformPoint(tool.points[segmentIndex]);
        Vector3 b = tr.TransformPoint(tool.points[nextIndex]);

        Vector3 insertWorld = ClosestPointOnSegment(worldPos, a, b);
        Vector3 insertLocal = tr.InverseTransformPoint(insertWorld);

        tool.InsertPoint(segmentIndex + 1, insertLocal);
        selectedPoint = segmentIndex + 1;
        tool.Bake();
    }

    private int GetPointIndex(Vector3 worldPos)
    {
        Transform tr = tool.transform;

        for (int i = 0; i < tool.points.Count; i++)
        {
            Vector3 point = tr.TransformPoint(tool.points[i]);
            float size = HandleUtility.GetHandleSize(point) * 0.1f;

            if (Vector3.Distance(point, worldPos) <= size)
                return i;
        }

        return -1;
    }

    private int GetSegmentIndex(Vector3 worldPos)
    {
        if (tool.points == null || tool.points.Count < 2)
            return -1;

        Transform tr = tool.transform;
        float minDistance = 0.35f;
        int bestSegment = -1;

        int segmentCount = tool.isClosedShape ? tool.points.Count : tool.points.Count - 1;

        for (int i = 0; i < segmentCount; i++)
        {
            int next = (i + 1) % tool.points.Count;

            Vector3 a = tr.TransformPoint(tool.points[i]);
            Vector3 b = tr.TransformPoint(tool.points[next]);

            float distance = DistancePointToSegment(worldPos, a, b);

            if (distance < minDistance)
            {
                minDistance = distance;
                bestSegment = i;
            }
        }

        return bestSegment;
    }

    private Vector3 ClosestPointOnSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float abLengthSqr = ab.sqrMagnitude;

        if (abLengthSqr < Mathf.Epsilon)
            return a;

        float t = Vector3.Dot(p - a, ab) / abLengthSqr;
        t = Mathf.Clamp01(t);

        return a + ab * t;
    }

    private float DistancePointToSegment(Vector3 p, Vector3 a, Vector3 b)
    {
        return Vector3.Distance(p, ClosestPointOnSegment(p, a, b));
    }

    private Vector3 GetMouseWorldPoint(Vector2 mousePosition)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
        Plane plane = new Plane(Vector3.up, tool.transform.position);

        if (plane.Raycast(ray, out float distance))
            return ray.GetPoint(distance);

        return Vector3.negativeInfinity;
    }
}