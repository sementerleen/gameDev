using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Procedural abandoned hospital generator.
/// 5 floors, proportional rooms, corridors and special areas.
/// </summary>
public class HospitalGenerator : MonoBehaviour
{
    [Header("Building Dimensions")]
    public float floorHeight = 4f;
    public int floorCount = 5;
    public float buildingWidth = 24f;   // X
    public float buildingDepth = 40f;   // Z
    public float wallThickness = 0.3f;

    [Header("Room Layout")]
    public float corridorWidth = 3f;
    public float roomWidth = 4.5f;      // along corridor
    public float roomDepth = 5.5f;      // into building side

    [Header("Materials")]
    public Material floorMaterial;
    public Material wallMaterial;
    public Material ceilingMaterial;
    public Material windowMaterial;

    private GameObject _root;
    private int _objCounter;

    // ──────────────────────────────────────────────
    // ENTRY POINT
    // ──────────────────────────────────────────────
    [ContextMenu("Generate Hospital")]
    public void Generate()
    {
        // Destroy previous build
        if (_root != null) DestroyImmediate(_root);
        _objCounter = 0;

        _root = new GameObject("Hospital");
        _root.transform.position = Vector3.zero;

        for (int i = 0; i < floorCount; i++)
        {
            BuildFloor(i);
        }

        // Exterior stairwell towers (north + south ends)
        BuildStairwell("StairN", new Vector3(-buildingWidth / 2f - 2f, 0, 0));
        BuildStairwell("StairS", new Vector3( buildingWidth / 2f + 2f, 0, 0));

        Debug.Log($"[HospitalGenerator] Hospital built: {floorCount} floors, {_objCounter} objects.");

#if UNITY_EDITOR
        EditorUtility.SetDirty(_root);
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
#endif
    }

    // ──────────────────────────────────────────────
    // FLOOR
    // ──────────────────────────────────────────────
    void BuildFloor(int floorIndex)
    {
        float y = floorIndex * floorHeight;
        GameObject floorRoot = new GameObject($"Floor_{floorIndex}");
        floorRoot.transform.SetParent(_root.transform);

        // ── Slab (floor plate) ──
        CreateBox($"Slab_F{floorIndex}", floorRoot.transform,
            new Vector3(0, y - 0.15f, 0),
            new Vector3(buildingWidth, 0.3f, buildingDepth),
            floorMaterial);

        // ── Ceiling (= underside of next slab, skip top floor) ──
        if (floorIndex < floorCount - 1)
        {
            CreateBox($"Ceil_F{floorIndex}", floorRoot.transform,
                new Vector3(0, y + floorHeight - 0.15f, 0),
                new Vector3(buildingWidth, 0.3f, buildingDepth),
                ceilingMaterial);
        }
        else
        {
            // Roof
            CreateBox($"Roof", floorRoot.transform,
                new Vector3(0, y + floorHeight, 0),
                new Vector3(buildingWidth + 0.6f, 0.4f, buildingDepth + 0.6f),
                wallMaterial);
        }

        // ── Exterior walls ──
        BuildExteriorWalls(floorIndex, y, floorRoot.transform);

        // ── Interior layout ──
        switch (floorIndex)
        {
            case 0: BuildGroundFloor(y, floorRoot.transform); break;
            case 1:
            case 2: BuildPatientFloor(floorIndex, y, floorRoot.transform); break;
            case 3: BuildSurgeryFloor(y, floorRoot.transform); break;
            case 4: BuildTopFloor(y, floorRoot.transform); break;
        }
    }

    // ──────────────────────────────────────────────
    // EXTERIOR WALLS (with window cutout placeholders)
    // ──────────────────────────────────────────────
    void BuildExteriorWalls(int fi, float y, Transform parent)
    {
        float h = floorHeight;
        float hw = buildingWidth / 2f;
        float hd = buildingDepth / 2f;
        float t = wallThickness;

        // North / South long walls
        CreateBox($"WallN_F{fi}", parent, new Vector3(0, y + h/2f, -hd - t/2f), new Vector3(buildingWidth + t*2, h, t), wallMaterial);
        CreateBox($"WallS_F{fi}", parent, new Vector3(0, y + h/2f,  hd + t/2f), new Vector3(buildingWidth + t*2, h, t), wallMaterial);

        // East / West short walls
        CreateBox($"WallE_F{fi}", parent, new Vector3(-hw - t/2f, y + h/2f, 0), new Vector3(t, h, buildingDepth), wallMaterial);
        CreateBox($"WallW_F{fi}", parent, new Vector3( hw + t/2f, y + h/2f, 0), new Vector3(t, h, buildingDepth), wallMaterial);

        // Windows on long walls (simple thin quads as placeholders)
        if (fi > 0) // no windows on ground floor south facade (lobby glass handled separately)
        {
            int winCount = Mathf.FloorToInt(buildingDepth / 3f);
            for (int i = 0; i < winCount; i++)
            {
                float wz = -hd + 1.5f + i * (buildingDepth / winCount);
                CreateBox($"WinN_F{fi}_{i}", parent,
                    new Vector3(-hw - t, y + h * 0.55f, wz),
                    new Vector3(t * 0.2f, h * 0.4f, 1.4f),
                    windowMaterial);
                CreateBox($"WinS_F{fi}_{i}", parent,
                    new Vector3( hw + t, y + h * 0.55f, wz),
                    new Vector3(t * 0.2f, h * 0.4f, 1.4f),
                    windowMaterial);
            }
        }
    }

    // ──────────────────────────────────────────────
    // GROUND FLOOR — Lobby + ER + Reception
    // ──────────────────────────────────────────────
    void BuildGroundFloor(float y, Transform parent)
    {
        float halfD = buildingDepth / 2f;

        // Central corridor
        BuildCorridor("Corr_G", y, parent);

        // Lobby (south half, open area)
        // Just a low reception desk
        CreateBox("ReceptionDesk", parent,
            new Vector3(0, y + 0.5f, halfD - 5f),
            new Vector3(6f, 1f, 1.2f), wallMaterial);

        // Waiting area benches
        for (int i = 0; i < 3; i++)
        {
            CreateBox($"Bench_G_{i}", parent,
                new Vector3(-4f + i * 4f, y + 0.25f, halfD - 9f),
                new Vector3(1.2f, 0.5f, 3f), wallMaterial);
        }

        // Emergency entrance (north end) — wide doorway header
        CreateBox("ERHeader", parent,
            new Vector3(0, y + floorHeight - 0.5f, -buildingDepth/2f + wallThickness),
            new Vector3(4f, 1f, wallThickness * 2), wallMaterial);

        // Rooms: left & right of corridor (ground floor: offices / storage)
        BuildRoomRow("Office", y, parent, side: -1, roomCount: 4, startZ: -buildingDepth/2f + 2f);
        BuildRoomRow("Storage", y, parent, side:  1, roomCount: 4, startZ: -buildingDepth/2f + 2f);
    }

    // ──────────────────────────────────────────────
    // PATIENT FLOORS (1 & 2)
    // ──────────────────────────────────────────────
    void BuildPatientFloor(int fi, float y, Transform parent)
    {
        BuildCorridor($"Corr_F{fi}", y, parent);

        int roomCount = Mathf.FloorToInt((buildingDepth - 4f) / roomWidth);
        BuildRoomRow($"PatientL_F{fi}", y, parent, side: -1, roomCount: roomCount, startZ: -buildingDepth/2f + 2f);
        BuildRoomRow($"PatientR_F{fi}", y, parent, side:  1, roomCount: roomCount, startZ: -buildingDepth/2f + 2f);

        // Nurse station in corridor center
        CreateBox($"NurseStation_F{fi}", parent,
            new Vector3(0, y + 0.6f, 0),
            new Vector3(2f, 1.2f, 1.5f), wallMaterial);
    }

    // ──────────────────────────────────────────────
    // SURGERY FLOOR (3)
    // ──────────────────────────────────────────────
    void BuildSurgeryFloor(float y, Transform parent)
    {
        BuildCorridor("Corr_F3", y, parent);

        float hw = buildingWidth / 2f - wallThickness;
        float hd = buildingDepth / 2f;

        // Two large operating rooms (each side)
        BuildLargeRoom("OperatingRoom_L", y, parent,
            new Vector3(-(corridorWidth/2f + roomDepth/2f), y + floorHeight/2f, -8f),
            new Vector3(roomDepth, floorHeight - 0.3f, roomWidth * 2.5f));

        BuildLargeRoom("OperatingRoom_R", y, parent,
            new Vector3( (corridorWidth/2f + roomDepth/2f), y + floorHeight/2f, -8f),
            new Vector3(roomDepth, floorHeight - 0.3f, roomWidth * 2.5f));

        // ICU rooms (south half)
        int icuCount = 3;
        BuildRoomRow("ICU_L", y, parent, side: -1, roomCount: icuCount, startZ: 4f);
        BuildRoomRow("ICU_R", y, parent, side:  1, roomCount: icuCount, startZ: 4f);

        // Scrub sinks
        CreateBox("ScrubSinks", parent,
            new Vector3(0, y + 0.8f, -buildingDepth/2f + 3f),
            new Vector3(3f, 0.1f, 0.6f), wallMaterial);
    }

    // ──────────────────────────────────────────────
    // TOP FLOOR (4) — Admin / Abandoned records
    // ──────────────────────────────────────────────
    void BuildTopFloor(float y, Transform parent)
    {
        BuildCorridor("Corr_F4", y, parent);

        int roomCount = Mathf.FloorToInt((buildingDepth - 4f) / roomWidth);
        BuildRoomRow("AdminL_F4", y, parent, side: -1, roomCount: roomCount, startZ: -buildingDepth/2f + 2f);
        BuildRoomRow("AdminR_F4", y, parent, side:  1, roomCount: roomCount, startZ: -buildingDepth/2f + 2f);

        // Collapsed ceiling section (horror detail)
        CreateBox("CollapsedCeil_F4", parent,
            new Vector3(3f, y + floorHeight * 0.6f, 5f),
            new Vector3(4f, 0.2f, 3f), wallMaterial);
    }

    // ──────────────────────────────────────────────
    // HELPERS
    // ──────────────────────────────────────────────

    void BuildCorridor(string label, float y, Transform parent)
    {
        // Corridor is the central spine — no fill, just side walls
        float hd = buildingDepth / 2f;
        float t = wallThickness;
        float h = floorHeight;

        // Left corridor wall
        CreateBox($"{label}_WallL", parent,
            new Vector3(-corridorWidth/2f - t/2f, y + h/2f, 0),
            new Vector3(t, h, buildingDepth), wallMaterial);

        // Right corridor wall
        CreateBox($"{label}_WallR", parent,
            new Vector3( corridorWidth/2f + t/2f, y + h/2f, 0),
            new Vector3(t, h, buildingDepth), wallMaterial);
    }

    /// <summary>Builds a row of rooms on one side of the corridor.</summary>
    void BuildRoomRow(string label, float y, Transform parent, int side, int roomCount, float startZ)
    {
        float xOffset = side * (corridorWidth/2f + wallThickness + roomDepth/2f);
        float h = floorHeight;

        for (int i = 0; i < roomCount; i++)
        {
            float z = startZ + i * roomWidth + roomWidth/2f;
            if (z + roomWidth/2f > buildingDepth/2f - wallThickness) break;

            // Room back wall
            CreateBox($"{label}_{i}_Back", parent,
                new Vector3(side * (corridorWidth/2f + wallThickness + roomDepth), y + h/2f, z),
                new Vector3(wallThickness, h, roomWidth), wallMaterial);

            // Room side walls
            CreateBox($"{label}_{i}_SideA", parent,
                new Vector3(xOffset, y + h/2f, z - roomWidth/2f),
                new Vector3(roomDepth, h, wallThickness), wallMaterial);
            CreateBox($"{label}_{i}_SideB", parent,
                new Vector3(xOffset, y + h/2f, z + roomWidth/2f),
                new Vector3(roomDepth, h, wallThickness), wallMaterial);

            // Door header (top of doorway opening)
            CreateBox($"{label}_{i}_DoorHeader", parent,
                new Vector3(side * (corridorWidth/2f + wallThickness/2f), y + h - 0.5f, z),
                new Vector3(wallThickness, 1f, 1.2f), wallMaterial);
        }
    }

    void BuildLargeRoom(string label, float y, Transform parent, Vector3 center, Vector3 size)
    {
        // Walls only (hollow box)
        float t = wallThickness;
        // Front wall (corridor side) with door gap
        CreateBox($"{label}_WallFront_A", parent,
            new Vector3(center.x, center.y, center.z - size.z/2f),
            new Vector3(size.x, size.y, t), wallMaterial);
        // Back wall
        CreateBox($"{label}_WallBack", parent,
            new Vector3(center.x, center.y, center.z + size.z/2f),
            new Vector3(size.x, size.y, t), wallMaterial);
        // Side walls
        CreateBox($"{label}_WallLeft", parent,
            new Vector3(center.x - size.x/2f, center.y, center.z),
            new Vector3(t, size.y, size.z), wallMaterial);
        CreateBox($"{label}_WallRight", parent,
            new Vector3(center.x + size.x/2f, center.y, center.z),
            new Vector3(t, size.y, size.z), wallMaterial);

        // Operating table
        CreateBox($"{label}_Table", parent,
            new Vector3(center.x, y + 0.6f, center.z),
            new Vector3(0.8f, 0.1f, 2.2f), wallMaterial);

        // Overhead lamp
        CreateBox($"{label}_Lamp", parent,
            new Vector3(center.x, y + floorHeight - 0.4f, center.z),
            new Vector3(0.6f, 0.1f, 0.6f), wallMaterial);
    }

    void BuildStairwell(string label, Vector3 basePos)
    {
        GameObject sw = new GameObject(label);
        sw.transform.SetParent(_root.transform);
        float t = wallThickness;

        for (int fi = 0; fi < floorCount; fi++)
        {
            float y = fi * floorHeight;
            // Stairwell shaft
            CreateBox($"{label}_Shaft_F{fi}", sw.transform,
                basePos + new Vector3(0, y + floorHeight/2f, 0),
                new Vector3(3f, floorHeight, 4f), wallMaterial);

            // Stair steps (simplified ramp)
            int steps = 10;
            for (int s = 0; s < steps; s++)
            {
                float stepH = (floorHeight / steps) * s;
                float stepZ = -1.8f + (3.6f / steps) * s;
                CreateBox($"{label}_Step_F{fi}_{s}", sw.transform,
                    basePos + new Vector3(0, y + stepH + 0.05f, stepZ),
                    new Vector3(2.4f, 0.1f, 3.6f / steps), wallMaterial);
            }
        }
    }

    // ──────────────────────────────────────────────
    // PRIMITIVE FACTORY
    // ──────────────────────────────────────────────
    GameObject CreateBox(string label, Transform parent, Vector3 position, Vector3 size, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = label;
        go.transform.SetParent(parent);
        go.transform.position = position;
        go.transform.localScale = size;
        _objCounter++;

        if (mat != null)
        {
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;
        }

        // Remove unnecessary colliders from small detail objects to save perf
        // (keep colliders on slabs, walls, ceiling)
        return go;
    }

#if UNITY_EDITOR
    [MenuItem("Hospital/Generate Hospital")]
    static void GenerateFromMenu()
    {
        var gen = FindObjectOfType<HospitalGenerator>();
        if (gen == null)
        {
            var go = new GameObject("HospitalGenerator");
            gen = go.AddComponent<HospitalGenerator>();
        }
        gen.Generate();
    }
#endif
}
