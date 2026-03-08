using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Menu: Hospital > Add Doors and Seating
/// Places sliding doors at every room entrance and benches/chairs throughout corridors.
/// </summary>
public static class HospitalDoorsAndSeatingEditor
{
    // ── Building constants (must match HospitalGenerator) ──────────
    const float FloorH      = 4f;
    const int   Floors      = 5;
    const float BuildingD   = 40f;
    const float BuildingW   = 24f;
    const float CorrW       = 3f;
    const float RoomW       = 4.5f;
    const float RoomD       = 5.5f;
    const float WallT       = 0.3f;
    const float DoorH       = 2.4f;
    const float DoorW       = 1.2f;

    static Transform _doorRoot, _seatingRoot, _detailRoot;
    static Material  _wallMat, _furnMat, _metalMat, _glassMat;

    [MenuItem("Hospital/Add Doors and Seating")]
    static void Run()
    {
        LoadMaterials();
        ResetRoot("HospitalDoors",   ref _doorRoot);
        ResetRoot("HospitalSeating", ref _seatingRoot);
        ResetRoot("HospitalDetails2",ref _detailRoot);

        int roomCount = Mathf.FloorToInt((BuildingD - 4f) / RoomW);

        for (int f = 0; f < Floors; f++)
        {
            float y = f * FloorH;

            // ── Room doors (left & right corridor walls) ──────────
            for (int i = 0; i < roomCount; i++)
            {
                float z = -BuildingD / 2f + 2f + i * RoomW + RoomW / 2f;
                if (z + RoomW / 2f > BuildingD / 2f - WallT) break;

                float doorX_L = -(CorrW / 2f + WallT / 2f);
                float doorX_R =  (CorrW / 2f + WallT / 2f);

                PlaceSlidingDoor($"Door_F{f}_{i}_L", new Vector3(doorX_L, y, z), true,  f);
                PlaceSlidingDoor($"Door_F{f}_{i}_R", new Vector3(doorX_R, y, z), false, f);
            }

            // ── Corridor seating ──────────────────────────────────
            AddCorridorSeating(f, y, roomCount);

            // ── Room window curtain tracks ────────────────────────
            AddCurtainTracks(f, y, roomCount);

            // ── Overhead door frames ──────────────────────────────
            AddDoorFrames(f, y, roomCount);

            // ── Corridor hand-rail along walls ────────────────────
            AddHandrail(f, y);

            // ── Floor baseboard strips ────────────────────────────
            AddBaseboards(f, y);
        }

        // ── Stairwell doors (each floor landing) ──────────────────
        for (int f = 0; f < Floors; f++)
        {
            float y = f * FloorH;
            PlaceSlidingDoor($"StairDoor_N_F{f}", new Vector3(-BuildingW / 2f - 2f, y, 0), true,  f);
            PlaceSlidingDoor($"StairDoor_S_F{f}", new Vector3( BuildingW / 2f + 2f, y, 0), false, f);
        }

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[HospitalDoors] Doors and seating placed.");
        EditorUtility.DisplayDialog("Tamam!", "Kapi ve oturma elemanlari eklendi!\nCtrl+S ile kaydet.", "OK");
    }

    // ── SLIDING DOOR ─────────────────────────────────────────────────
    static void PlaceSlidingDoor(string label, Vector3 pos, bool facingX, int floor)
    {
        GameObject root = new GameObject(label);
        root.transform.SetParent(_doorRoot);
        root.transform.position = pos + Vector3.up * (DoorH / 2f);

        // Door frame (top)
        GameObject frame = Cube($"{label}_Frame", root.transform,
            Vector3.up * (DoorH / 2f + 0.1f),
            new Vector3(facingX ? WallT + 0.02f : DoorW + 0.2f,
                        0.2f,
                        facingX ? DoorW + 0.2f : WallT + 0.02f), _wallMat);
        // Frame sides
        for (int s = -1; s <= 1; s += 2)
        {
            Vector3 sideOff = facingX
                ? new Vector3(0f, 0f, s * (DoorW / 2f + 0.1f))
                : new Vector3(s * (DoorW / 2f + 0.1f), 0f, 0f);
            Cube($"{label}_FrameSide{s}", root.transform,
                sideOff + Vector3.zero,
                new Vector3(facingX ? WallT + 0.02f : 0.15f,
                            DoorH,
                            facingX ? 0.15f : WallT + 0.02f), _wallMat);
        }

        // Panel Left
        GameObject panelL = new GameObject($"{label}_PanelL");
        panelL.transform.SetParent(root.transform);
        panelL.transform.localPosition = facingX
            ? new Vector3(0f, 0f, -DoorW * 0.28f)
            : new Vector3(-DoorW * 0.28f, 0f, 0f);

        Cube($"{label}_PanelL_Mesh", panelL.transform, Vector3.zero,
            new Vector3(facingX ? 0.04f : DoorW * 0.5f,
                        DoorH - 0.05f,
                        facingX ? DoorW * 0.5f : 0.04f), _glassMat);

        // Panel Right
        GameObject panelR = new GameObject($"{label}_PanelR");
        panelR.transform.SetParent(root.transform);
        panelR.transform.localPosition = facingX
            ? new Vector3(0f, 0f,  DoorW * 0.28f)
            : new Vector3( DoorW * 0.28f, 0f, 0f);

        Cube($"{label}_PanelR_Mesh", panelR.transform, Vector3.zero,
            new Vector3(facingX ? 0.04f : DoorW * 0.5f,
                        DoorH - 0.05f,
                        facingX ? DoorW * 0.5f : 0.04f), _glassMat);

        // Door track (top rail)
        Cube($"{label}_Track", root.transform,
            Vector3.up * (DoorH / 2f - 0.05f),
            new Vector3(facingX ? 0.05f : DoorW + 0.1f,
                        0.06f,
                        facingX ? DoorW + 0.1f : 0.05f), _metalMat);

        // SlidingDoor component
        SlidingDoor sd = root.AddComponent<SlidingDoor>();
        sd.panelLeft   = panelL.transform;
        sd.panelRight  = panelR.transform;
        sd.openDistance = DoorW * 0.48f;
    }

    // ── CORRIDOR SEATING ─────────────────────────────────────────────
    static void AddCorridorSeating(int f, float y, int roomCount)
    {
        // Benches between room doors on the right side of corridor
        for (int i = 0; i < roomCount; i += 2)  // every 2 rooms
        {
            float z = -BuildingD / 2f + 2f + i * RoomW + RoomW;
            float benchX = CorrW / 2f + 0.25f;

            // Bench seat
            Cube($"CorridorBench_F{f}_{i}", _seatingRoot,
                new Vector3(benchX, y + 0.44f, z),
                new Vector3(0.4f, 0.06f, RoomW * 0.7f), _furnMat);
            // Bench back support
            Cube($"CorridorBenchBack_F{f}_{i}", _seatingRoot,
                new Vector3(benchX + 0.18f, y + 0.75f, z),
                new Vector3(0.06f, 0.55f, RoomW * 0.7f), _furnMat);
            // Bench legs
            for (int leg = 0; leg < 2; leg++)
            {
                float lz = z - RoomW * 0.28f + leg * RoomW * 0.56f;
                Cube($"BenchLeg_F{f}_{i}_{leg}L", _seatingRoot,
                    new Vector3(benchX - 0.15f, y + 0.22f, lz),
                    new Vector3(0.06f, 0.44f, 0.06f), _metalMat);
                Cube($"BenchLeg_F{f}_{i}_{leg}R", _seatingRoot,
                    new Vector3(benchX + 0.15f, y + 0.22f, lz),
                    new Vector3(0.06f, 0.44f, 0.06f), _metalMat);
            }

            // Single chair on opposite side
            if (i + 1 < roomCount)
            {
                float cz = -BuildingD / 2f + 2f + (i + 1) * RoomW + RoomW / 2f;
                PlaceChair($"CorridorChair_F{f}_{i}", _seatingRoot,
                    new Vector3(-CorrW / 2f - 0.3f, y, cz), 90f);
            }
        }

        // Lobby floor extra seating (ground floor only)
        if (f == 0)
        {
            for (int row = 0; row < 2; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    float sx = -4f + col * 2.5f;
                    float sz =  BuildingD / 2f - 8f - row * 2f;
                    PlaceChair($"LobbyChair_R{row}_C{col}", _seatingRoot,
                        new Vector3(sx, 0f, sz), row == 0 ? 180f : 0f);
                }
            }
        }
    }

    // ── CURTAIN TRACKS ───────────────────────────────────────────────
    static void AddCurtainTracks(int f, float y, int roomCount)
    {
        if (f < 1 || f > 2) return; // only patient floors

        for (int i = 0; i < roomCount; i++)
        {
            float z = -BuildingD / 2f + 2f + i * RoomW + RoomW / 2f;
            float trackY = y + FloorH - 0.5f;

            foreach (float rx in new[] { -(CorrW / 2f + RoomD / 2f), (CorrW / 2f + RoomD / 2f) })
            {
                // Track rail
                Cube($"CurtainTrack_F{f}_{i}_{(rx < 0 ? "L" : "R")}", _detailRoot,
                    new Vector3(rx, trackY, z),
                    new Vector3(0.03f, 0.04f, RoomW - 0.3f), _metalMat);

                // Curtain panel (partly drawn)
                Cube($"Curtain_F{f}_{i}_{(rx < 0 ? "L" : "R")}", _detailRoot,
                    new Vector3(rx, trackY - 0.9f, z - RoomW * 0.15f),
                    new Vector3(0.02f, 1.8f, RoomW * 0.55f), _furnMat);
            }
        }
    }

    // ── DOOR FRAMES ──────────────────────────────────────────────────
    static void AddDoorFrames(int f, float y, int roomCount)
    {
        for (int i = 0; i < roomCount; i++)
        {
            float z = -BuildingD / 2f + 2f + i * RoomW + RoomW / 2f;

            foreach (float rx in new[] { -(CorrW / 2f + WallT / 2f), (CorrW / 2f + WallT / 2f) })
            {
                // Top of door frame strip
                Cube($"DoorFrameTop_F{f}_{i}_{(rx < 0 ? "L" : "R")}", _detailRoot,
                    new Vector3(rx, y + DoorH + 0.1f, z),
                    new Vector3(WallT + 0.04f, 0.2f, DoorW + 0.25f), _wallMat);

                // Doorstep / threshold
                Cube($"Threshold_F{f}_{i}_{(rx < 0 ? "L" : "R")}", _detailRoot,
                    new Vector3(rx, y + 0.02f, z),
                    new Vector3(0.35f, 0.04f, DoorW + 0.1f), _metalMat);

                // Room number plate
                Cube($"RoomNumber_F{f}_{i}_{(rx < 0 ? "L" : "R")}", _detailRoot,
                    new Vector3(rx + (rx < 0 ? -0.18f : 0.18f), y + DoorH - 0.3f, z + DoorW * 0.4f),
                    new Vector3(0.05f, 0.18f, 0.25f), _furnMat);
            }
        }
    }

    // ── WALL HANDRAIL ────────────────────────────────────────────────
    static void AddHandrail(int f, float y)
    {
        float railY  = y + 0.9f;
        float railT  = 0.05f;

        // Left corridor wall handrail
        Cube($"Handrail_L_F{f}", _detailRoot,
            new Vector3(-(CorrW / 2f + WallT + 0.06f), railY, 0f),
            new Vector3(railT, railT, BuildingD - 2f), _metalMat);

        // Right corridor wall handrail
        Cube($"Handrail_R_F{f}", _detailRoot,
            new Vector3( (CorrW / 2f + WallT + 0.06f), railY, 0f),
            new Vector3(railT, railT, BuildingD - 2f), _metalMat);

        // Handrail supports every 2m
        int supports = Mathf.FloorToInt(BuildingD / 2f);
        for (int s = 0; s < supports; s++)
        {
            float sz = -BuildingD / 2f + 1f + s * 2f;
            foreach (float rx in new[] { -(CorrW / 2f + WallT + 0.06f), (CorrW / 2f + WallT + 0.06f) })
            {
                Cube($"HRSupport_F{f}_{s}_{(rx < 0 ? "L" : "R")}", _detailRoot,
                    new Vector3(rx, y + 0.5f, sz),
                    new Vector3(0.04f, 0.8f, 0.04f), _metalMat);
            }
        }
    }

    // ── BASEBOARDS ───────────────────────────────────────────────────
    static void AddBaseboards(int f, float y)
    {
        float bh = 0.12f;
        float bt = 0.04f;

        // Along corridor walls
        Cube($"BaseL_F{f}", _detailRoot,
            new Vector3(-(CorrW / 2f + WallT + bt / 2f), y + bh / 2f, 0f),
            new Vector3(bt, bh, BuildingD), _wallMat);
        Cube($"BaseR_F{f}", _detailRoot,
            new Vector3( (CorrW / 2f + WallT + bt / 2f), y + bh / 2f, 0f),
            new Vector3(bt, bh, BuildingD), _wallMat);
        // Exterior walls
        Cube($"BaseN_F{f}", _detailRoot,
            new Vector3(0f, y + bh / 2f, -BuildingD / 2f - WallT - bt / 2f),
            new Vector3(BuildingW + bt * 2, bh, bt), _wallMat);
        Cube($"BaseS_F{f}", _detailRoot,
            new Vector3(0f, y + bh / 2f,  BuildingD / 2f + WallT + bt / 2f),
            new Vector3(BuildingW + bt * 2, bh, bt), _wallMat);
    }

    // ── HELPERS ──────────────────────────────────────────────────────
    static void PlaceChair(string label, Transform parent, Vector3 pos, float yRot)
    {
        GameObject seat = Cube($"{label}_Seat", parent,
            pos + new Vector3(0f, 0.44f, 0f), new Vector3(0.48f, 0.06f, 0.44f), _furnMat);
        Cube($"{label}_Back", parent,
            pos + new Vector3(0f, 0.72f, -0.2f), new Vector3(0.48f, 0.52f, 0.05f), _furnMat);
        seat.transform.parent.eulerAngles = new Vector3(0f, yRot, 0f);

        for (int leg = 0; leg < 4; leg++)
        {
            float lx = leg < 2 ? -0.2f : 0.2f;
            float lz = leg % 2 == 0 ? -0.18f : 0.18f;
            Cube($"{label}_Leg{leg}", parent,
                pos + new Vector3(lx, 0.22f, lz), new Vector3(0.05f, 0.44f, 0.05f), _metalMat);
        }
    }

    static void LoadMaterials()
    {
        _wallMat  = Load("M_Wall");
        _furnMat  = Load("M_Furniture");
        _metalMat = Load("M_Metal");
        _glassMat = Load("M_Window");

        // Fallback: create glass material if missing
        if (_glassMat == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");
            Shader s = Shader.Find("HDRP/Lit") ?? Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            _glassMat = new Material(s) { name = "M_Window", color = new Color(0.5f, 0.75f, 0.9f, 0.35f) };
            AssetDatabase.CreateAsset(_glassMat, "Assets/Materials/M_Window.mat");
        }
        if (_furnMat == null)
        {
            Shader s = Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
            _furnMat = new Material(s) { name = "M_Furniture", color = new Color(0.8f, 0.78f, 0.72f) };
            AssetDatabase.CreateAsset(_furnMat, "Assets/Materials/M_Furniture.mat");
        }
        if (_metalMat == null)
        {
            Shader s = Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
            _metalMat = new Material(s) { name = "M_Metal", color = new Color(0.7f, 0.7f, 0.72f) };
            AssetDatabase.CreateAsset(_metalMat, "Assets/Materials/M_Metal.mat");
        }
        if (_wallMat == null)
        {
            Shader s = Shader.Find("HDRP/Lit") ?? Shader.Find("Standard");
            _wallMat = new Material(s) { name = "M_Wall", color = new Color(0.92f, 0.90f, 0.85f) };
            AssetDatabase.CreateAsset(_wallMat, "Assets/Materials/M_Wall.mat");
        }
        AssetDatabase.SaveAssets();
    }

    static Material Load(string name) =>
        AssetDatabase.LoadAssetAtPath<Material>($"Assets/Materials/{name}.mat");

    static void ResetRoot(string name, ref Transform t)
    {
        GameObject go = GameObject.Find(name);
        if (go != null) Object.DestroyImmediate(go);
        go = new GameObject(name);
        t  = go.transform;
    }

    static GameObject Cube(string name, Transform parent, Vector3 localPos, Vector3 scale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localPosition = localPos;
        go.transform.localScale    = scale;
        Object.DestroyImmediate(go.GetComponent<Collider>());
        if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return go;
    }
}
