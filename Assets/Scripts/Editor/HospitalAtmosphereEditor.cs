using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Menu: Hospital > Setup Daylight Atmosphere
/// </summary>
public static class HospitalAtmosphereEditor
{
    [MenuItem("Hospital/Setup Daylight Atmosphere")]
    static void SetupAtmosphere()
    {
        // ── SUN ──────────────────────────────────────────────────────
        GameObject sunGo = GameObject.Find("Sun");
        if (sunGo == null) sunGo = new GameObject("Sun");
        Light sun = sunGo.GetComponent<Light>();
        if (sun == null) sun = sunGo.AddComponent<Light>();
        sun.type      = LightType.Directional;
        sun.color     = new Color(1f, 0.95f, 0.85f);
        sun.intensity = 3.5f;
        sun.shadows   = LightShadows.Soft;
        sunGo.transform.rotation = Quaternion.Euler(52f, -30f, 0f);

        // ── AMBIENT ───────────────────────────────────────────────────
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = new Color(0.6f, 0.7f, 0.9f);
        RenderSettings.ambientEquatorColor = new Color(0.5f, 0.55f, 0.6f);
        RenderSettings.ambientGroundColor  = new Color(0.2f, 0.2f, 0.2f);

        // ── FOG (light haze) ─────────────────────────────────────────
        RenderSettings.fog           = true;
        RenderSettings.fogColor      = new Color(0.75f, 0.80f, 0.85f);
        RenderSettings.fogMode       = FogMode.Linear;
        RenderSettings.fogStartDistance = 20f;
        RenderSettings.fogEndDistance   = 80f;

        // ── CORRIDOR LIGHTS ───────────────────────────────────────────
        GameObject lightParent = GameObject.Find("InteriorLights");
        if (lightParent == null) lightParent = new GameObject("InteriorLights");
        // Clear old
        while (lightParent.transform.childCount > 0)
            Object.DestroyImmediate(lightParent.transform.GetChild(0).gameObject);

        float floorHeight  = 4f;
        float buildingDepth= 40f;
        int   floors       = 5;
        int   lightsPerFloor = 6;

        for (int f = 0; f < floors; f++)
        {
            float y = f * floorHeight + floorHeight - 0.4f;
            for (int i = 0; i < lightsPerFloor; i++)
            {
                float z = -buildingDepth / 2f + 3f + i * (buildingDepth / lightsPerFloor);
                CreatePointLight($"CorridorLight_F{f}_{i}", lightParent.transform,
                    new Vector3(0f, y, z), new Color(0.95f, 0.97f, 1f), 800f, 9f);
                CreatePointLight($"RoomLightL_F{f}_{i}", lightParent.transform,
                    new Vector3(-6f, y, z), new Color(0.95f, 0.97f, 1f), 500f, 7f);
                CreatePointLight($"RoomLightR_F{f}_{i}", lightParent.transform,
                    new Vector3( 6f, y, z), new Color(0.95f, 0.97f, 1f), 500f, 7f);
            }
        }

        // ── WINDOW SUN SHAFTS ─────────────────────────────────────────
        GameObject shaftParent = GameObject.Find("SunShafts");
        if (shaftParent == null) shaftParent = new GameObject("SunShafts");
        while (shaftParent.transform.childCount > 0)
            Object.DestroyImmediate(shaftParent.transform.GetChild(0).gameObject);

        float hw = 12f;
        for (int f = 1; f < floors; f++)
        {
            float y = f * floorHeight + floorHeight * 0.55f;
            for (int i = 0; i < 4; i++)
            {
                float z = -15f + i * 10f;
                GameObject sh = new GameObject($"Shaft_F{f}_{i}");
                sh.transform.SetParent(shaftParent.transform);
                sh.transform.position = new Vector3(-hw, y, z);
                sh.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
                Light sl = sh.AddComponent<Light>();
                sl.type      = LightType.Spot;
                sl.color     = new Color(1f, 0.95f, 0.8f);
                sl.intensity = 2f;
                sl.range     = 15f;
                sl.spotAngle = 25f;
                sl.shadows   = LightShadows.Soft;
            }
        }

        EditorSceneManager.MarkAllScenesDirty();
        Debug.Log("[HospitalAtmosphere] Done.");
        EditorUtility.DisplayDialog("Tamam!", "Atmosfer hazir! Ctrl+S ile kaydet.", "OK");
    }

    static void CreatePointLight(string name, Transform parent, Vector3 pos, Color color, float intensity, float range)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        Light l = go.AddComponent<Light>();
        l.type      = LightType.Point;
        l.color     = color;
        l.intensity = intensity;
        l.range     = range;
        l.shadows   = LightShadows.None;
    }
}
