using UnityEngine;
using UnityEngine.UI;

public class EnergyBar : MonoBehaviour
{
    [SerializeField] private Image[] energyPoints;
    [SerializeField] private Item[] items;

    [SerializeField] private int currentEnergy = 5;
    [SerializeField] private int maxEnergy = 5;

    private void OnEnable()
    {
        foreach (Item item in items)
        {
            if (item != null) item.OnItemUsed += OnItemUsed;
        }

        SubscribeToEnergyManager();
    }

    private void OnDisable()
    {
        foreach (Item item in items)
        {
            if (item != null) item.OnItemUsed -= OnItemUsed;
        }

        UnsubscribeFromEnergyManager();
    }

    private void Start()
    {
        SubscribeToEnergyManager();
        RefreshDisplay();
    }

    private void SubscribeToEnergyManager()
    {
        if (LateBloom.Raising.PlayerEnergyManager.Instance != null)
        {
            LateBloom.Raising.PlayerEnergyManager.Instance.OnEnergyChanged -= SyncWithEnergyManager;
            LateBloom.Raising.PlayerEnergyManager.Instance.OnEnergyChanged += SyncWithEnergyManager;
        }
    }

    private void UnsubscribeFromEnergyManager()
    {
        if (LateBloom.Raising.PlayerEnergyManager.Instance != null)
        {
            LateBloom.Raising.PlayerEnergyManager.Instance.OnEnergyChanged -= SyncWithEnergyManager;
        }
    }

    private void RefreshDisplay()
    {
        if (LateBloom.Raising.PlayerEnergyManager.Instance != null)
        {
            SyncWithEnergyManager(
                LateBloom.Raising.PlayerEnergyManager.Instance.CurrentEnergy,
                LateBloom.Raising.PlayerEnergyManager.Instance.MaxEnergy
            );
        }
        else
        {
            UpdateEnergyBar();
        }
    }

    private void SyncWithEnergyManager(int current, int max)
    {
        if (energyPoints == null || energyPoints.Length == 0) return;
        // Setiap 20 energi = 1 bar pasti (100 = 5, 80 = 4, 60 = 3, 40 = 2, 20 = 1, 0 = 0)
        int activeCount = Mathf.Clamp(current / 20, 0, energyPoints.Length);
        currentEnergy = activeCount;

        for (int i = 0; i < energyPoints.Length; i++)
        {
            if (energyPoints[i] != null)
            {
                energyPoints[i].enabled = (i < activeCount);
            }
        }
    }

    private void OnItemUsed(ItemType type, float value)
    {
        if (LateBloom.Raising.PlayerEnergyManager.Instance != null)
        {
            // PlayerEnergyManager langsung mengupdate stamina, sinkronkan tampilan seketika
            SyncWithEnergyManager(
                LateBloom.Raising.PlayerEnergyManager.Instance.CurrentEnergy,
                LateBloom.Raising.PlayerEnergyManager.Instance.MaxEnergy
            );
            return;
        }

        // Fallback jika mode standalone
        if (type == ItemType.Tea)
        {
            currentEnergy = Mathf.Clamp(currentEnergy + 2, 0, maxEnergy);
        }
        else
        {
            currentEnergy = Mathf.Clamp(currentEnergy - 1, 0, maxEnergy);
        }

        UpdateEnergyBar();
    }

    private void UpdateEnergyBar()
    {
        if (energyPoints == null) return;

        for (int i = 0; i < energyPoints.Length; i++)
        {
            if (energyPoints[i] != null)
            {
                energyPoints[i].enabled = (i < currentEnergy);
            }
        }
    }
}