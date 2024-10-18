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
        UpdateDataManager();
    }

    public void RemoveActiveAnimal(Animal animal)
    {
        ActiveAnimals.Remove(animal);
        UpdateDataManager();
    }

    private void UpdateDataManager()
    {
        // update data manager with current animal locations
        DataManager.Instance.SaveActiveAnimals(ActiveAnimals);
    }
}
