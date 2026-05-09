using UnityEngine;
using UnityEditor;
using UnityEngine.AI;
using NavMeshPlus.Components;

public class RebakeNavMesh
{
    public static void Execute()
    {
        var surfaces = Object.FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None);
        if (surfaces.Length == 0)
        {
            // Fallback: legacy NavMesh bake
            UnityEditor.AI.NavMeshBuilder.BuildNavMesh();
            Debug.Log("[RebakeNavMesh] Legacy NavMesh rebaked (no NavMeshSurface found).");
            return;
        }

        foreach (var surface in surfaces)
        {
            var so = new SerializedObject(surface);
            var radiusProp = so.FindProperty("m_AgentRadius");
            if (radiusProp != null)
            {
                radiusProp.floatValue = 0.75f;
                so.ApplyModifiedProperties();
            }
            surface.BuildNavMesh();
            Debug.Log($"[RebakeNavMesh] Surface '{surface.name}' rebaked with agentRadius=0.75.");
        }
    }
}
