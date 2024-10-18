using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalManager : MonoBehaviour
{
    public static AnimalManager Instance;

    public List<Animal> ActiveAnimals { get; private set; }

    private void Awake()
    {
        Instance = this;
        ActiveAnimals = new List<Animal>();
    }

    private void Start()
    {
        LoadSavedAnimals();
    }

    public void AddActiveAnimal(Animal animal, bool saveToData = true)
    {
        ActiveAnimals.Add(animal);
        Debug.Log("Animal added to active animals list.");
        Debug.Log("Active Animal Count: " + ActiveAnimals.Count);

        if (saveToData)
        {
            UpdateDataManager();
        }
    }

    public void RemoveActiveAnimal(Animal animal)
    {
        if (ActiveAnimals.Contains(animal))
        {
            ActiveAnimals.Remove(animal);
            Debug.Log("Animal removed from active animals list.");
            Debug.Log("Active Animal Count: " + ActiveAnimals.Count);
            UpdateDataManager();
        }
        else
        {
            Debug.LogWarning("Attempted to remove an animal that was not in the active animals list.");
        }
    }

    private void LoadSavedAnimals()
    {
        if (DataManager.Instance.SavedActiveAnimals.ItemList.Count > 0)
        {
            // use copy of list in case of any changes
            List<AnimalData> savedAnimals = DataManager.Instance.SavedActiveAnimals.ItemList;

            // get item prefab array
            GameObject[] itemPrefabs = InventoryManager.Instance.ItemPrefabs;
            
            foreach (AnimalData animalData in savedAnimals)
            {
                // instantiate new prefab at saved position and rotation
                GameObject animalObject = Instantiate(itemPrefabs[animalData.prefabIndex], animalData.position, animalData.rotation);

                // add new prefab to active animals list
                if (animalObject.TryGetComponent(out Animal animal))
                {
                    ActiveAnimals.Add(animal);
                }
                else
                {
                    Debug.LogWarning("Incorrect item prefab index likely as instantiated object did not have an Animal component.");
                }
            }
        }
    }

    private void UpdateDataManager()
    {
        // update data manager with current animal locations
        DataManager.Instance.SaveActiveAnimals(ActiveAnimals);
    }
}
