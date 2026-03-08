using UnityEngine;
using UnityEditor;

/// <summary>
/// Menu: Hospital > Apply Hospital Materials
/// Applies color-coded HDRP/Lit materials: cream walls, grey floor, white ceiling,
/// mint accent, window glass, medical red detail.
/// </summary>
public static class HospitalMaterialsEditor
{
    [MenuItem("Hospital/Apply Hospital Materials")]
    static void ApplyMaterials()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        // ── Palette ────────────────────────────────────────────────
        // Walls: off-white cream
        Material wallMat    = Make("M_Wall",       new Color(0.92f, 0.90f, 0.85f), 0.3f, 0.1f);
        // Accent wall: mint green (classic hospital)
        Material accentMat  = Make("M_WallAccent", new Color(0.72f, 0.85f, 0.78f), 0.3f, 0.05f);
        // Floor: worn light grey linoleum
        Material floorMat   = Make("M_Floor",      new Color(0.55f, 0.56f, 0.54f), 0.5f, 0.05f);
        // Ceiling: bright white
        Material ceilMat    = Make("M_Ceiling",    new Color(0.96f, 0.96f, 0.95f), 0.2f, 0.05f);
        // Window glass: light blue transparent-ish
        Material winMat     = Make("M_Window",     new Color(0.55f, 0.78f, 0.92f), 0.05f, 0.8f);
        // Furniture: dirty white / beige
        Material furnMat    = Make("M_Furniture",  new Color(0.80f, 0.78f, 0.72f), 0.4f, 0.05f);
        // Medical red detail
        Material redMat     = Make("M_MedRed",     new Color(0.75f, 0.10f, 0.10f), 0.3f, 0.1f);
        // Metal (sinks, beds)
        Material metalMat   = Make("M_Metal",      new Color(0.70f, 0.70f, 0.72f), 0.8f, 0.5f);
        // Stair / concrete
        Material concreteMat= Make("M_Concrete",   new Color(0.50f, 0.50f, 0.50f), 0.6f, 0.02f);

        int count = 0;
        foreach (var mr in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            string n = mr.gameObject.name.ToLower();
            Material mat = wallMat;

            if (n.Contains("slab"))                              mat = floorMat;
            else if (n.Contains("step"))                         mat = concreteMat;
            else if (n.Contains("ceil") || n.Contains("roof"))   mat = ceilMat;
            else if (n.Contains("win"))                          mat = winMat;
            else if (n.Contains("shaft"))                        mat = concreteMat;
            else if (n.Contains("desk") || n.Contains("bench") ||
                     n.Contains("table") || n.Contains("bed") ||
                     n.Contains("locker") || n.Contains("cart") ||
                     n.Contains("chair") || n.Contains("cabinet"))
                                                                 mat = furnMat;
            else if (n.Contains("sink") || n.Contains("lamp") ||
                     n.Contains("rail") || n.Contains("pole"))   mat = metalMat;
            else if (n.Contains("cross") || n.Contains("sign") ||
                     n.Contains("redline"))                      mat = redMat;
            // Accent: every other room's back wall
            else if (n.Contains("_back") && IsAccentRoom(n))    mat = accentMat;

            mr.sharedMaterial = mat;
            count++;
        }

        AssetDatabase.SaveAssets();
        UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        Debug.Log($"[HospitalMaterials] {count} renderer guncellendi.");
        EditorUtility.DisplayDialog("Tamam!", $"{count} objeye materyal atandi.", "OK");
    }

    static bool IsAccentRoom(string name)
    {
        // Accent the even-numbered rooms for visual variety
        for (int i = 0; i < 20; i += 2)
            if (name.Contains($"_{i}_")) return true;
        return false;
    }

    static Material Make(string matName, Color color, float roughness, float metallic)
    {
        string path = $"Assets/Materials/{matName}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader s = Shader.Find("HDRP/Lit")
                    ?? Shader.Find("Universal Render Pipeline/Lit")
                    ?? Shader.Find("Standard");
            mat = new Material(s) { name = matName };
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;

        // Try HDRP property names, fallback to Standard
        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", 1f - roughness);
        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", metallic);

        EditorUtility.SetDirty(mat);
        return mat;
    }
}
