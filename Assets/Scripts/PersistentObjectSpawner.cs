using UnityEngine;

public class PersistentObjectSpawner : MonoBehaviour
{
    // CONFIG DATA
    [Tooltip("Esses prefabs serão instanciados apenas uma vez e persistirão entre as cenas.")]
    [SerializeField] private GameObject[] persistentObjectPrefabs;

    // PRIVATE STATE
    private static bool hasSpawned = false;

    // PRIVATE
    private void Awake()
    {
        if (hasSpawned) return;

        SpawnPersistentObjects();

        hasSpawned = true; // Garante que, ao recarregar a cena, não duplicaremos os objetos.
    }

    private void SpawnPersistentObjects()
    {
        foreach (GameObject prefab in persistentObjectPrefabs)
        {
            if (prefab != null)
            {
                GameObject persistentObject = Instantiate(prefab);
                DontDestroyOnLoad(persistentObject);
            }
            else
            {
                Debug.LogWarning("PersistentObjectSpawner: Um dos prefabs na lista está vazio!");
            }
        }
    }
}