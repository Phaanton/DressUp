using UnityEngine;

public class GameBootstrapper : MonoBehaviour
{
    [Header("Dependências da Cena")]
    [SerializeField] private DressUpView mainView;
    [SerializeField] private AdsManager adsManager; // Pode instanciar na mão se preferir, ou arrastar o prefab aqui por enquanto

    [Header("Banco de Dados")]
    [SerializeField] private CatalogGroupSO mainCatalog;

    private DressUpModel model;
    private DressUpPresenter presenter;

    private void Start()
    {
        // 1. Cria o Model (Regras e Inventário)
        model = new DressUpModel();

        // 2. Cria o Presenter injetando todas as peças
        presenter = new DressUpPresenter(model, mainView, mainCatalog, adsManager);

        // 3. Inicializa a UI preenchendo o menu com os itens do catálogo
        // (Usando == == para evitar bugs no chat, ajuste na sua IDE se necessário!)
        mainView.PopulateCategoryMenu(mainCatalog.GetAllItems());
    }
}