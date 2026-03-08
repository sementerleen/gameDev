using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Reflection;

/// <summary>
/// Menu: Hospital > Setup Daylight Atmosphere
/// Creates sun, sky volume, interior lights for a bright abandoned hospital.
/// </summary>
public static class HospitalAtmosphereEditor
{
    [MenuItem("Hospital/Setup Daylight Atmosphere")]
    static void SetupAtmosphere()
    {
        // ── 1. SUN (Directional Light) ──────────────────────────────
        GameObject sunGo = GameObject.Find("Sun") ?? new GameObject("Sun");
        Light sun = sunGo.GetComponent<Light>() ?? sunGo.AddComponent<Light>();
        sun.type      = LightType.Directional;
        sun.color     = new Color(1f, 0.95f, 0.85f);
        sun.intensity = 3.5f;
        sun.shadows   = LightShadows.Soft;
        sunGo.transform.rotation = Quaternion.Euler(52f, -30f, 0f);
        Undo.RegisterCreatedObjectUndo(sunGo, "Create Sun");

        // ── 2. AMBIENT / SKY VOLUME ─────────────────────────────────
        GameObject volGo = GameObject.Find("SkySunVolume") ?? new GameObject("SkySunVolume");
        Volume vol = volGo.GetComponent<Volume>() ?? volGo.AddComponent<Volume>();
        vol.isGlobal  = true;
        vol.priority  = 1f;

        if (vol.profile == null)
        {
            VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
            if (!AssetDatabase.IsValidFolder("Assets/Settings"))
                AssetDatabase.CreateFolder("Assets", "Settings");
            AssetDatabase.CreateAsset(profile, "Assets/Settings/HospitalAtmosphere.asset");
            vol.sharedProfile = profile;
        }

        Undo.RegisterCreatedObjectUndo(volGo, "Create Sky Volume");

        // ── 3. CORRIDOR / INTERIOR LIGHTS ───────────────────────────
        // One fluorescent light strip per corridor segment, per floor
        float floorHeight = 4f;
        int floors = 5;
        float buildingDepth = 40f;
        int lightsPerFloor = 6;

        GameObject lightParent = GameObject.Find("InteriorLights") ?? new GameObject("InteriorLights");
        // Clear old lights
        while (lightParent.transform.childCount > 0)
            Object.DestroyImmediate(lightParent.transform.GetChild(0).gameObject);

        for (int f = 0; f < floors; f++)
        {
            float y = f * floorHeight + floorHeight - 0.3f;
            for (int i = 0; i < lightsPerFloor; i++)
            {
                float z = -buildingDepth / 2f + 3f + i * (buildingDepth / lightsPerFloor);

                // Fluorescent strip (emissive quad stand-in + point light)
                GameObject lightGo = new GameObject($"CorridorLight_F{f}_{i}");
                lightGo.transform.SetParent(lightParent.transform);
                lightGo.transform.position = new Vector3(0f, y, z);

                Light l = lightGo.AddComponent<Light>();
                l.type      = LightType.Point;
                l.color     = new Color(0.95f, 0.97f, 1f);   // cool white fluorescent
                l.intensity = 600f;                           // HDRP lumen units
                l.range     = 8f;
                l.shadows   = LightShadows.None;              // perf

                // Side room lights
                GameObject roomLightL = new GameObject($"RoomLight_F{f}_{i}_L");
                roomLightL.transform.SetParent(lightParent.transform);
                roomLightL.transform.position = new Vector3(-6f, y, z);
                Light rl = roomLightL.AddComponent<Light>();
                rl.type = LightType.Point; rl.color = new Color(0.95f, 0.97f, 1f);
                rl.intensity = 400f; rl.range = 7f; rl.shadows = LightShadows.None;

                GameObject roomLightR = new GameObject($"RoomLight_F{f}_{i}_R");
                roomLightR.transform.SetParent(lightParent.transform);
                roomLightR.transform.position = new Vector3(6f, y, z);
                Light rr = roomLightR.AddComponent<Light>();
                rr.type = LightType.Point; rr.color = new Color(0.95f, 0.97f, 1f);
                rr.intensity = 400f; rr.range = 7f; rr.shadows = LightShadows.None;
            }
        }

        // ── 4. WINDOW SUN SHAFTS (spot lights from windows) ─────────
        GameObject shaftParent = GameObject.Find("SunShafts") ?? new GameObject("SunShafts");
        while (shaftParent.transform.childCount > 0)
            Object.DestroyImmediate(shaftParent.transform.GetChild(0).gameObject);

        float hw = 12f; // half building width
        for (int f = 1; f < floors; f++)
        {
            float y = f * floorHeight + floorHeight * 0.55f;
            for (int i = 0; i < 4; i++)
            {
                float z = -15f + i * 10f;
                // Left wall shaft
                GameObject sh = new GameObject($"Shaft_F{f}_{i}");
                sh.transform.SetParent(shaftParent.transform);
                sh.transform.position = new Vector3(-hw, y, z);
                sh.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                Light sl = sh.AddComponent<Light>();
                sl.type      = LightType.Spot;
                sl.color     = new Color(1f, 0.95f, 0.8f);
                sl.intensity = 3000f;
                sl.range     = 15f;
                sl.spotAngle = 25f;
                sl.shadows   = LightShadows.Soft;
            }
        }

        EditorUtility.SetDirty(lightParent);
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[HospitalAtmosphere] Daylight atmosphere setup complete.");
        EditorUtility.DisplayDialog("Tamam!", "Gun isigi atmosferi hazir!\nSahneyi kaydet (Ctrl+S).", "OK");
    }
}
