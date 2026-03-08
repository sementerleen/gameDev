using UnityEngine;

/// <summary>
/// İki monitör sistemini yönetir.
/// Display 1 → Arda (normal dünya)
/// Display 2 → Ela (paranormal dünya)
///
/// Kullanım: Bu script, iki kameranın parent'ı olan bir GameObject'e eklenir.
/// Arda kamerasının targetDisplay = 0, Ela kamerasının targetDisplay = 1 olmalı.
/// </summary>
public class DisplayManager : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Camera ardaCamera;
    [SerializeField] private Camera elaCamera;

    [Header("Settings")]
    [SerializeField] private bool activateSecondDisplayOnStart = true;

    void Awake()
    {
        if (ardaCamera != null)
            ardaCamera.targetDisplay = 0;

        if (elaCamera != null)
            elaCamera.targetDisplay = 1;
    }

    void Start()
    {
        if (!activateSecondDisplayOnStart) return;

        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            Debug.Log("[DisplayManager] İkinci monitör aktive edildi.");
        }
        else
        {
            Debug.LogWarning("[DisplayManager] İkinci monitör bulunamadı. Tek ekran modunda çalışılıyor.");
        }
    }

    /// <summary>
    /// Editor'da test ederken Ela'nın ekranını birinci monitörde görmek için çağrılabilir.
    /// </summary>
    public void DebugShowElaOnPrimary()
    {
        if (elaCamera != null)
            elaCamera.targetDisplay = 0;
    }

    public void ResetDisplayTargets()
    {
        if (ardaCamera != null) ardaCamera.targetDisplay = 0;
        if (elaCamera != null) elaCamera.targetDisplay = 1;
    }
}
