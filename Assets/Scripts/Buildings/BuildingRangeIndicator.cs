using UnityEngine;

// Draws a semi-transparent disc + crisp outline showing attack range.
// Disc and outline live on child GameObjects to avoid conflict with
// the building's own SpriteRenderer.
public class BuildingRangeIndicator : MonoBehaviour
{
    [Header("Colors")]
    public Color fillColor    = new Color(0.3f, 0.75f, 1f, 0.15f);
    public Color outlineColor = new Color(0.3f, 0.75f, 1f, 0.85f);

    [Header("Outline")]
    public float outlineWidth = 0.05f;
    [Range(24, 64)] public int segments = 48;

    [Header("Sorting — adjust if circle renders above/below wrong layer")]
    public int fillSortingOrder    = -1;   // relative to building sprite
    public int outlineSortingOrder =  0;

    private MeshFilter   _meshFilter;
    private MeshRenderer _meshRenderer;
    private LineRenderer _outline;

    void Awake()
    {
        // ── Filled disc on its own child ──────────────────────────
        var discGO = new GameObject("RangeDisc");
        discGO.transform.SetParent(transform, false);

        _meshFilter   = discGO.AddComponent<MeshFilter>();
        _meshRenderer = discGO.AddComponent<MeshRenderer>();

        // Pick a shader that works in both built-in and URP
        Shader fillShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                         ?? Shader.Find("Sprites/Default");
        var fillMat   = new Material(fillShader) { color = fillColor };

        // Inherit sorting layer from parent SpriteRenderer so disc stays in the same pass
        var sr = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();
        _meshRenderer.sortingLayerName = sr != null ? sr.sortingLayerName : "Default";
        _meshRenderer.sortingOrder     = (sr != null ? sr.sortingOrder : 0) + fillSortingOrder;
        _meshRenderer.material         = fillMat;

        // ── Outline on its own child ──────────────────────────────
        var outlineGO = new GameObject("RangeOutline");
        outlineGO.transform.SetParent(transform, false);

        _outline = outlineGO.AddComponent<LineRenderer>();
        _outline.useWorldSpace   = false;
        _outline.loop            = true;
        _outline.widthMultiplier = outlineWidth;
        _outline.positionCount   = segments;

        Shader outlineShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                            ?? Shader.Find("Sprites/Default");
        var outlineMat = new Material(outlineShader) { color = outlineColor };

        _outline.sortingLayerName = _meshRenderer.sortingLayerName;
        _outline.sortingOrder     = (sr != null ? sr.sortingOrder : 0) + outlineSortingOrder;
        _outline.material         = outlineMat;

        discGO.SetActive(false);
        outlineGO.SetActive(false);
    }

    public void SetRadius(float radius)
    {
        if (radius <= 0f) return;
        BuildDisc(radius);
        BuildOutline(radius);
    }

    public void Show()
    {
        if (_meshFilter)  _meshFilter.gameObject.SetActive(true);
        if (_outline)     _outline.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (_meshFilter)  _meshFilter.gameObject.SetActive(false);
        if (_outline)     _outline.gameObject.SetActive(false);
    }

    void BuildDisc(float r)
    {
        var mesh  = new Mesh();
        var verts = new Vector3[segments + 1];
        var tris  = new int[segments * 3];

        verts[0] = Vector3.zero;
        for (int i = 0; i < segments; i++)
        {
            float a = (float)i / segments * Mathf.PI * 2f;
            verts[i + 1] = new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f);
        }
        for (int i = 0; i < segments; i++)
        {
            tris[i * 3]     = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = (i + 1) % segments + 1;
        }

        mesh.vertices  = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        _meshFilter.mesh = mesh;
    }

    void BuildOutline(float r)
    {
        _outline.positionCount = segments;
        for (int i = 0; i < segments; i++)
        {
            float a = (float)i / segments * Mathf.PI * 2f;
            _outline.SetPosition(i, new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f));
        }
    }
}
