using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    [Header("Dependências da Cena")]
    [SerializeField] private DressUpView mainView;

    // Removemos o SerializeField daqui, pois vamos achá-lo dinamicamente
    private AdsManager adsManager;

    [Header("Banco de Dados")]
    [SerializeField] private CatalogGroupSO mainCatalog;

    private DressUpModel model;
    private DressUpPresenter presenter;

    private void Start()
    {
        // Busca o AdsManager que o PersistentObjectSpawner acabou de instanciar na cena
        adsManager = Object.FindAnyObjectByType<AdsManager>();

        if (adsManager == null)
        {
            Debug.LogError("Bootstrapper não encontrou o AdsManager! Verifique se ele está no Spawner.");
            return;
        }

        // 1. Cria o Model 
        model = new DressUpModel();

        // 2. Cria o Presenter injetando todas as peças
        presenter = new DressUpPresenter(model, mainView, mainCatalog, adsManager);

        // 3. Inicializa a UI preenchendo o menu com os itens do catálogo
        mainView.PopulateCategoryMenu(mainCatalog.GetAllItems(), model.IsItemUnlocked);
    }
}