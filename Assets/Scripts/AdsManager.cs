using System;
using System.Collections;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public void ShowRewardedAd(Action onSuccess, Action onFailed)
    {
        Debug.Log("AdsManager: Solicitando Rewarded Ad da CrazyGames...");

        // Inicia a simulação do tempo do anúncio
        StartCoroutine(SimulateAdViewing(onSuccess));
    }

    private IEnumerator SimulateAdViewing(Action onSuccess)
    {
        Debug.Log("AdsManager: Exibindo vídeo... (aguarde 3 segundos)");

        // Espera 3 segundos simulando o jogador assistindo ao vídeo
        yield return new WaitForSeconds(3f);

        Debug.Log("AdsManager: Vídeo concluído com sucesso!");

        // Dispara o callback de sucesso que o Presenter está esperando
        onSuccess?.Invoke();
    }
}