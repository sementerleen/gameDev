using UnityEngine;
using UnityEditor;

/// <summary>
/// Menu: Hospital > Add Interior Details
/// Adds beds, chairs, lockers, nurse stations, IV poles, medical carts etc.
/// All built from primitives — zero external asset dependency.
/// </summary>
public static class HospitalInteriorEditor
{
    static Transform _root;
    static Material _furnMat, _metalMat, _redMat;

    [MenuItem("Hospital/Add Interior Details")]
    static void AddInterior()
    {
        // Grab or create detail root
        GameObject rootGo = GameObject.Find("InteriorDetails");
        if (rootGo != null) Object.DestroyImmediate(rootGo);
        rootGo  = new GameObject("InteriorDetails");
        _root   = rootGo.transform;

        // Load materials (must run after Apply Hospital Materials)
        _furnMat  = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Furniture.mat");
        _metalMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_Metal.mat");
        _redMat   = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/M_MedRed.mat");

        float floorH  = 4f;
        float roomW   = 4.5f;
        float corrW   = 3f;
        float bldDepth= 40f;
        float bldWidth= 24f;
        int   floors  = 5;

        for (int f = 0; f < floors; f++)
        {
            float y = f * floorH;

            int roomCount = Mathf.FloorToInt((bldDepth - 4f) / roomW);

            for (int i = 0; i < roomCount; i++)
            {
                float z = -bldDepth / 2f + 2f + i * roomW + roomW / 2f;
                if (z + roomW / 2f > bldDepth / 2f - 0.3f) break;

                float xL = -(corrW / 2f + 3f);  // left room center X
                float xR =  (corrW / 2f + 3f);  // right room center X

                switch (f)
                {
                    case 0: AddOfficeRoom(xL, y, z, f, i, "L"); AddStorageRoom(xR, y, z, f, i, "R"); break;
                    case 1:
                    case 2: AddPatientRoom(xL, y, z, f, i, "L"); AddPatientRoom(xR, y, z, f, i, "R"); break;
                    case 3: /* Surgery handled separately */ break;
                    case 4: AddAdminRoom(xL, y, z, f, i, "L"); AddAdminRoom(xR, y, z, f, i, "R"); break;
                }
            }

            // Corridor details every floor
            AddCorridorDetails(f, y, bldDepth, corrW);

            if (f == 3) AddSurgeryDetails(y, bldWidth, corrW);
        }

        // Lobby (ground floor south)
        AddLobbyDetails(bldDepth);

        EditorUtility.SetDirty(rootGo);
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[HospitalInterior] Interior details added.");
        EditorUtility.DisplayDialog("Tamam!", "Ic mekan detaylari eklendi!\nSahneyi kaydet (Ctrl+S).", "OK");
    }

    // ── PATIENT ROOM ────────────────────────────────────────────────
    static void AddPatientRoom(float x, float y, float z, int f, int i, string side)
    {
        string label = $"Patient_F{f}_{i}_{side}";

        // Hospital bed frame
        Cube($"{label}_BedFrame",  new Vector3(x, y + 0.25f, z), new Vector3(0.9f, 0.5f, 2.0f), _furnMat);
        Cube($"{label}_Mattress",  new Vector3(x, y + 0.53f, z), new Vector3(0.88f, 0.06f, 1.9f), _furnMat);
        Cube($"{label}_Pillow",    new Vector3(x, y + 0.62f, z + 0.8f), new Vector3(0.5f, 0.08f, 0.35f), _furnMat);
        // Bed rails
        Cube($"{label}_RailL",     new Vector3(x - 0.46f, y + 0.65f, z), new Vector3(0.02f, 0.25f, 1.8f), _metalMat);
        Cube($"{label}_RailR",     new Vector3(x + 0.46f, y + 0.65f, z), new Vector3(0.02f, 0.25f, 1.8f), _metalMat);
        // Bed legs
        for (int leg = 0; leg < 4; leg++)
        {
            float lx = x + (leg < 2 ? -0.4f : 0.4f);
            float lz = z + (leg % 2 == 0 ? -0.85f : 0.85f);
            Cube($"{label}_Leg{leg}", new Vector3(lx, y + 0.125f, lz), new Vector3(0.06f, 0.25f, 0.06f), _metalMat);
        }
        // IV Pole
        float poleX = x + (side == "L" ? 0.6f : -0.6f);
        Cube($"{label}_IVPole",    new Vector3(poleX, y + 0.9f, z - 0.7f), new Vector3(0.04f, 1.8f, 0.04f), _metalMat);
        Cube($"{label}_IVBag",     new Vector3(poleX, y + 1.75f, z - 0.7f), new Vector3(0.12f, 0.22f, 0.06f), _furnMat);
        // Bedside table
        Cube($"{label}_BedTable",  new Vector3(poleX, y + 0.5f, z + 0.7f), new Vector3(0.4f, 1f, 0.4f), _furnMat);
        Cube($"{label}_TableTop",  new Vector3(poleX, y + 1.01f, z + 0.7f), new Vector3(0.44f, 0.04f, 0.44f), _furnMat);
        // Monitor (on table)
        Cube($"{label}_Monitor",   new Vector3(poleX, y + 1.25f, z + 0.7f), new Vector3(0.3f, 0.2f, 0.04f), _furnMat);
        // Chair for visitor
        AddChair($"{label}_Chair", new Vector3(x, y, z - 1.2f));
    }

    // ── OFFICE ROOM (Ground floor) ─────────────────────────────────
    static void AddOfficeRoom(float x, float y, float z, int f, int i, string side)
    {
        string label = $"Office_F{f}_{i}_{side}";
        // Desk
        Cube($"{label}_Desk",      new Vector3(x, y + 0.4f, z - 1f), new Vector3(1.2f, 0.8f, 0.6f), _furnMat);
        Cube($"{label}_DeskTop",   new Vector3(x, y + 0.81f, z - 1f), new Vector3(1.22f, 0.04f, 0.62f), _furnMat);
        // Chair
        AddChair($"{label}_Chair", new Vector3(x, y, z - 0.5f));
        // Filing cabinet
        Cube($"{label}_Cabinet",   new Vector3(x + 0.7f, y + 0.7f, z + 1.2f), new Vector3(0.5f, 1.4f, 0.4f), _furnMat);
        // Papers on desk
        Cube($"{label}_Papers",    new Vector3(x - 0.3f, y + 0.84f, z - 1.1f), new Vector3(0.3f, 0.02f, 0.22f), _furnMat);
    }

    // ── STORAGE ROOM ───────────────────────────────────────────────
    static void AddStorageRoom(float x, float y, float z, int f, int i, string side)
    {
        string label = $"Storage_F{f}_{i}_{side}";
        // Shelving units
        for (int shelf = 0; shelf < 3; shelf++)
        {
            float sy = y + 0.4f + shelf * 0.7f;
            Cube($"{label}_Shelf{shelf}", new Vector3(x, sy, z - 0.8f), new Vector3(1.4f, 0.05f, 0.35f), _metalMat);
        }
        // Vertical supports
        Cube($"{label}_SuppL", new Vector3(x - 0.7f, y + 1.05f, z - 0.8f), new Vector3(0.05f, 2.1f, 0.05f), _metalMat);
        Cube($"{label}_SuppR", new Vector3(x + 0.7f, y + 1.05f, z - 0.8f), new Vector3(0.05f, 2.1f, 0.05f), _metalMat);
        // Medical boxes on shelves
        for (int b = 0; b < 4; b++)
        {
            Cube($"{label}_Box{b}", new Vector3(x - 0.4f + b * 0.28f, y + 0.48f, z - 0.8f),
                new Vector3(0.22f, 0.16f, 0.28f), _furnMat);
        }
        // Medical cart
        AddMedCart($"{label}_Cart", new Vector3(x + 0.4f, y, z + 0.8f));
    }

    // ── ADMIN ROOM ─────────────────────────────────────────────────
    static void AddAdminRoom(float x, float y, float z, int f, int i, string side)
    {
        string label = $"Admin_F{f}_{i}_{side}";
        Cube($"{label}_Desk",    new Vector3(x, y + 0.4f, z), new Vector3(1.3f, 0.8f, 0.7f), _furnMat);
        Cube($"{label}_DeskTop", new Vector3(x, y + 0.81f, z), new Vector3(1.32f, 0.04f, 0.72f), _furnMat);
        AddChair($"{label}_Chair", new Vector3(x, y, z + 0.6f));
        // Lockers
        for (int lk = 0; lk < 3; lk++)
            Cube($"{label}_Locker{lk}", new Vector3(x - 0.6f + lk * 0.45f, y + 1f, z - 1.3f), new Vector3(0.4f, 2f, 0.5f), _furnMat);
    }

    // ── SURGERY FLOOR DETAILS ───────────────────────────────────────
    static void AddSurgeryDetails(float y, float bldWidth, float corrW)
    {
        string label = "Surgery";
        float xL = -(corrW / 2f + 3f);
        float xR =  (corrW / 2f + 3f);

        // OR Tables (left & right rooms)
        foreach (float rx in new[] { xL, xR })
        {
            Cube($"{label}_ORTable_{(rx < 0 ? "L" : "R")}",
                new Vector3(rx, y + 0.55f, -8f), new Vector3(0.7f, 0.1f, 2.2f), _metalMat);
            // Overhead surgical lamp
            Cube($"{label}_SurgLamp_{(rx < 0 ? "L" : "R")}",
                new Vector3(rx, y + 3.5f, -8f), new Vector3(0.7f, 0.15f, 0.7f), _metalMat);
            Cube($"{label}_LampArm_{(rx < 0 ? "L" : "R")}",
                new Vector3(rx, y + 3.2f, -8f), new Vector3(0.05f, 0.6f, 0.05f), _metalMat);
            // Instrument tray
            Cube($"{label}_Tray_{(rx < 0 ? "L" : "R")}",
                new Vector3(rx + (rx < 0 ? -1f : 1f), y + 0.85f, -8f), new Vector3(0.5f, 0.04f, 0.8f), _metalMat);
            // Anesthesia machine
            Cube($"{label}_AnMachine_{(rx < 0 ? "L" : "R")}",
                new Vector3(rx + (rx < 0 ? 1f : -1f), y + 1f, -6f), new Vector3(0.6f, 2f, 0.5f), _furnMat);
        }

        // ICU beds (south of surgery floor)
        for (int i = 0; i < 3; i++)
        {
            float z = 5f + i * 4.5f;
            Cube($"{label}_ICUBed_L_{i}", new Vector3(xL, y + 0.25f, z), new Vector3(0.9f, 0.5f, 2f), _furnMat);
            Cube($"{label}_ICUBed_R_{i}", new Vector3(xR, y + 0.25f, z), new Vector3(0.9f, 0.5f, 2f), _furnMat);
            // Heart monitor
            Cube($"{label}_Monitor_L_{i}", new Vector3(xL + 0.8f, y + 1.4f, z - 0.5f), new Vector3(0.35f, 0.25f, 0.08f), _furnMat);
            Cube($"{label}_Monitor_R_{i}", new Vector3(xR - 0.8f, y + 1.4f, z - 0.5f), new Vector3(0.35f, 0.25f, 0.08f), _furnMat);
        }
    }

    // ── CORRIDOR DETAILS ────────────────────────────────────────────
    static void AddCorridorDetails(int f, float y, float bldDepth, float corrW)
    {
        int count = Mathf.FloorToInt(bldDepth / 8f);
        for (int i = 0; i < count; i++)
        {
            float z = -bldDepth / 2f + 4f + i * 8f;

            // Wheelchair
            if (i % 3 == 0)
            {
                string wc = $"Wheelchair_F{f}_{i}";
                Cube($"{wc}_Seat",  new Vector3(corrW / 2f - 0.8f, y + 0.5f, z), new Vector3(0.5f, 0.05f, 0.4f), _metalMat);
                Cube($"{wc}_Back",  new Vector3(corrW / 2f - 0.8f, y + 0.75f, z - 0.2f), new Vector3(0.5f, 0.5f, 0.04f), _metalMat);
                Cube($"{wc}_WheelL",new Vector3(corrW / 2f - 1.05f, y + 0.3f, z), new Vector3(0.04f, 0.6f, 0.06f), _metalMat);
                Cube($"{wc}_WheelR",new Vector3(corrW / 2f - 0.55f, y + 0.3f, z), new Vector3(0.04f, 0.6f, 0.06f), _metalMat);
            }

            // Stretcher / gurney
            if (i % 4 == 1)
            {
                string g = $"Gurney_F{f}_{i}";
                Cube($"{g}_Bed",   new Vector3(-corrW / 2f + 0.8f, y + 0.65f, z), new Vector3(0.75f, 0.1f, 2f), _metalMat);
                Cube($"{g}_LegFL", new Vector3(-corrW / 2f + 0.45f, y + 0.32f, z - 0.85f), new Vector3(0.04f, 0.65f, 0.04f), _metalMat);
                Cube($"{g}_LegFR", new Vector3(-corrW / 2f + 1.1f, y + 0.32f, z - 0.85f), new Vector3(0.04f, 0.65f, 0.04f), _metalMat);
                Cube($"{g}_LegBL", new Vector3(-corrW / 2f + 0.45f, y + 0.32f, z + 0.85f), new Vector3(0.04f, 0.65f, 0.04f), _metalMat);
                Cube($"{g}_LegBR", new Vector3(-corrW / 2f + 1.1f, y + 0.32f, z + 0.85f), new Vector3(0.04f, 0.65f, 0.04f), _metalMat);
            }

            // Wall-mounted fire extinguisher
            if (i % 5 == 2)
            {
                Cube($"FireExt_F{f}_{i}", new Vector3(corrW / 2f + 0.15f, y + 1.2f, z),
                    new Vector3(0.18f, 0.45f, 0.18f), _redMat);
            }

            // Corridor bench (waiting)
            if (f == 0 && i % 3 == 0)
            {
                Cube($"CorridorBench_F{f}_{i}", new Vector3(0f, y + 0.22f, z), new Vector3(1.4f, 0.44f, 0.4f), _furnMat);
            }
        }

        // Nurse station per floor center
        string ns = $"NurseStation_F{f}";
        Cube($"{ns}_Desk",    new Vector3(0f, y + 0.55f, 0f),        new Vector3(3f, 1.1f, 1f), _furnMat);
        Cube($"{ns}_DeskTop", new Vector3(0f, y + 1.11f, 0f),        new Vector3(3.05f, 0.05f, 1.05f), _furnMat);
        Cube($"{ns}_Monitor", new Vector3(0.6f, y + 1.4f, -0.4f),    new Vector3(0.4f, 0.28f, 0.06f), _furnMat);
        Cube($"{ns}_Monitor2",new Vector3(-0.6f, y + 1.4f, -0.4f),   new Vector3(0.4f, 0.28f, 0.06f), _furnMat);
        AddChair($"{ns}_Chair", new Vector3(0f, y, 0.7f));
    }

    // ── LOBBY DETAILS ───────────────────────────────────────────────
    static void AddLobbyDetails(float bldDepth)
    {
        float y = 0f;
        float z0 = bldDepth / 2f - 3f;

        // Reception desk (L-shape)
        Cube("Lobby_ReceptionMain",  new Vector3(0f, y + 0.55f, z0 - 0.5f), new Vector3(5f, 1.1f, 0.9f), _furnMat);
        Cube("Lobby_ReceptionSide",  new Vector3(2.7f, y + 0.55f, z0 - 1.5f), new Vector3(0.9f, 1.1f, 2f), _furnMat);
        Cube("Lobby_ReceptionTop",   new Vector3(0f, y + 1.11f, z0 - 0.5f), new Vector3(5.05f, 0.05f, 0.95f), _furnMat);
        Cube("Lobby_ReceptionTopS",  new Vector3(2.7f, y + 1.11f, z0 - 1.5f), new Vector3(0.95f, 0.05f, 2.05f), _furnMat);

        // Reception monitors
        Cube("Lobby_Monitor1", new Vector3(-1f, y + 1.4f, z0 - 0.9f), new Vector3(0.4f, 0.28f, 0.06f), _furnMat);
        Cube("Lobby_Monitor2", new Vector3(0.5f, y + 1.4f, z0 - 0.9f), new Vector3(0.4f, 0.28f, 0.06f), _furnMat);

        // Waiting area chairs (3 rows)
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 5; col++)
            {
                AddChair($"Lobby_Chair_R{row}_C{col}",
                    new Vector3(-4f + col * 2f, y, z0 - 5f - row * 1.5f));
            }
        }

        // Potted plant (decorative cylinder + sphere)
        Cube("Lobby_Pot1",    new Vector3(-8f, y + 0.3f, z0 - 2f), new Vector3(0.5f, 0.6f, 0.5f), _furnMat);
        Cube("Lobby_Plant1",  new Vector3(-8f, y + 0.9f, z0 - 2f), new Vector3(0.7f, 0.6f, 0.7f), _furnMat);
        Cube("Lobby_Pot2",    new Vector3(8f, y + 0.3f, z0 - 2f), new Vector3(0.5f, 0.6f, 0.5f), _furnMat);
        Cube("Lobby_Plant2",  new Vector3(8f, y + 0.9f, z0 - 2f), new Vector3(0.7f, 0.6f, 0.7f), _furnMat);

        // Information board
        Cube("Lobby_InfoBoard", new Vector3(0f, y + 1.5f, bldDepth / 2f - 0.35f), new Vector3(3f, 1.5f, 0.08f), _furnMat);
        // Red cross sign above reception
        Cube("Lobby_RedCross_H", new Vector3(0f, y + 2.8f, z0 - 0.5f), new Vector3(0.8f, 0.25f, 0.06f), _redMat);
        Cube("Lobby_RedCross_V", new Vector3(0f, y + 2.8f, z0 - 0.5f), new Vector3(0.25f, 0.8f, 0.06f), _redMat);
    }

    // ── HELPERS ─────────────────────────────────────────────────────
    static void AddChair(string label, Vector3 pos)
    {
        Cube($"{label}_Seat", pos + new Vector3(0f, 0.22f, 0f), new Vector3(0.45f, 0.06f, 0.42f), _furnMat);
        Cube($"{label}_Back", pos + new Vector3(0f, 0.52f, -0.2f), new Vector3(0.45f, 0.5f, 0.05f), _furnMat);
        Cube($"{label}_LegFL",pos + new Vector3(-0.19f, 0.1f,  0.18f), new Vector3(0.05f, 0.4f, 0.05f), _metalMat);
        Cube($"{label}_LegFR",pos + new Vector3( 0.19f, 0.1f,  0.18f), new Vector3(0.05f, 0.4f, 0.05f), _metalMat);
        Cube($"{label}_LegBL",pos + new Vector3(-0.19f, 0.1f, -0.18f), new Vector3(0.05f, 0.4f, 0.05f), _metalMat);
        Cube($"{label}_LegBR",pos + new Vector3( 0.19f, 0.1f, -0.18f), new Vector3(0.05f, 0.4f, 0.05f), _metalMat);
    }

    static void AddMedCart(string label, Vector3 pos)
    {
        Cube($"{label}_Body",  pos + new Vector3(0f, 0.45f, 0f), new Vector3(0.55f, 0.9f, 0.38f), _metalMat);
        Cube($"{label}_Top",   pos + new Vector3(0f, 0.91f, 0f), new Vector3(0.57f, 0.04f, 0.40f), _metalMat);
        Cube($"{label}_DrawerA", pos + new Vector3(0f, 0.65f, -0.2f), new Vector3(0.50f, 0.18f, 0.04f), _furnMat);
        Cube($"{label}_DrawerB", pos + new Vector3(0f, 0.40f, -0.2f), new Vector3(0.50f, 0.18f, 0.04f), _furnMat);
        Cube($"{label}_WheelFL",  pos + new Vector3(-0.22f, 0.05f,  0.15f), new Vector3(0.08f, 0.1f, 0.08f), _metalMat);
        Cube($"{label}_WheelFR",  pos + new Vector3( 0.22f, 0.05f,  0.15f), new Vector3(0.08f, 0.1f, 0.08f), _metalMat);
        Cube($"{label}_WheelBL",  pos + new Vector3(-0.22f, 0.05f, -0.15f), new Vector3(0.08f, 0.1f, 0.08f), _metalMat);
        Cube($"{label}_WheelBR",  pos + new Vector3( 0.22f, 0.05f, -0.15f), new Vector3(0.08f, 0.1f, 0.08f), _metalMat);
    }

    static void Cube(string name, Vector3 pos, Vector3 scale, Material mat)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(_root);
        go.transform.position = pos;
        go.transform.localScale = scale;
        // Remove collider from small props for performance
        var col = go.GetComponent<Collider>();
        if (col != null) Object.DestroyImmediate(col);
        if (mat != null) go.GetComponent<MeshRenderer>().sharedMaterial = mat;
    }
}
