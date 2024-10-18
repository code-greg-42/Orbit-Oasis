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

    public void AddActiveAnimal(Animal animal)
    {
        ActiveAnimals.Add(animal);
        Debug.Log("Animal added to active animals list.");
        Debug.Log("Active Animal Count: " + ActiveAnimals.Count);
        UpdateDataManager();
    }

    public void RemoveActiveAnimal(Animal animal)
    {
        ActiveAnimals.Remove(animal);
        Debug.Log("Animal removed from active animals list.");
        Debug.Log("Active Animal Count: " + ActiveAnimals.Count);
        UpdateDataManager();
    }

    private void UpdateDataManager()
    {
        // update data manager with current animal locations
        DataManager.Instance.SaveActiveAnimals(ActiveAnimals);
    }
}
