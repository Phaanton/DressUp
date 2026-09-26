using System;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    // Usamos Actions para injetar o que deve acontecer após o AD
    public void ShowRewardedAd(Action onSuccess, Action onFailed)
    {
        Debug.Log("AdsManager: Solicitando Rewarded Ad da CrazyGames...");

        // AQUI ENTRARÁ O CÓDIGO REAL DO SDK. 
        // Exemplo da documentação deles: 
        // CrazyAds.Instance.beginAdBreakRewarded(onSuccess, onFailed);

        // Simulando que o jogador assistiu até o fim com sucesso:
        onSuccess?.Invoke();
    }
}