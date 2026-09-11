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
            item.OnItemUsed += OnItemUsed;
        }
    }

    private void OnDisable()
    {
        foreach (Item item in items)
        {
            item.OnItemUsed -= OnItemUsed;
        }
    }

    private void Start()
    {
        UpdateEnergyBar();
    }

    private void OnItemUsed(ItemType type, float value)
    {
        if (type == ItemType.Tea)
        {
            currentEnergy += Mathf.RoundToInt(value);

            currentEnergy = Mathf.Clamp(
                currentEnergy,
                0,
                maxEnergy
            );

            UpdateEnergyBar();
        }
        else
        {
            currentEnergy--;

            currentEnergy = Mathf.Clamp(
                currentEnergy,
                0,
                maxEnergy
            );

            UpdateEnergyBar();
        }
    }

    private void UpdateEnergyBar()
    {
        for (int i = 0; i < energyPoints.Length; i++)
        {
            energyPoints[i].enabled = i < currentEnergy;
        }
    }
}