using UnityEngine;
using UnityEditor;

/// <summary>
/// One-click setup: creates HDRP materials and fixes pink objects in the hospital.
/// Menu: Hospital > Fix Materials (HDRP)
/// </summary>
public static class HospitalSetupEditor
{
    [MenuItem("Hospital/Fix Materials (HDRP)")]
    static void FixMaterials()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");

        Material wallMat  = GetOrCreateMat("HospitalWall",    new Color(0.85f, 0.82f, 0.78f));
        Material floorMat = GetOrCreateMat("HospitalFloor",   new Color(0.45f, 0.43f, 0.40f));
        Material ceilMat  = GetOrCreateMat("HospitalCeiling", new Color(0.80f, 0.80f, 0.78f));
        Material winMat   = GetOrCreateMat("HospitalWindow",  new Color(0.4f,  0.7f,  0.9f));

        int count = 0;
        foreach (var mr in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None))
        {
            string n = mr.gameObject.name.ToLower();
            Material mat = wallMat;

            if (n.Contains("slab") || n.Contains("step"))
                mat = floorMat;
            else if (n.Contains("ceil") || n.Contains("roof"))
                mat = ceilMat;
            else if (n.Contains("win"))
                mat = winMat;

            mr.sharedMaterial = mat;
            count++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[HospitalSetup] {count} renderer duzeltildi.");
        EditorUtility.DisplayDialog("Tamam!", $"{count} objeye HDRP materyal atandi. Pembe yok!", "OK");
    }

    static Material GetOrCreateMat(string name, Color color)
    {
        string path = $"Assets/Materials/{name}.mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null)
        {
            Shader shader = Shader.Find("HDRP/Lit")
                         ?? Shader.Find("Universal Render Pipeline/Lit")
                         ?? Shader.Find("Standard");
            mat = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(mat, path);
        }
        mat.color = color;
        EditorUtility.SetDirty(mat);
        return mat;
    }
}
