using UnityEngine;

// 1. A interface que dita a regra: tudo o que é visual precisa de um Sprite e de uma Cor
public interface IClothingVisual
{
    Sprite GetSprite();
    Color GetColor();
}

// 2. O Visual Padrão (Lê os dados originais do seu ScriptableObject)
public class DefaultClothingVisual : IClothingVisual
{
    private readonly ClothingItemSO baseItem;

    public DefaultClothingVisual(ClothingItemSO baseItem)
    {
        this.baseItem = baseItem;
    }

    // Retorna o sprite original que desenhou
    public Sprite GetSprite() => baseItem.itemSprite;

    // A roupa original assume sempre a cor branca (sem tintura)
    public Color GetColor() => Color.white;
}

// 3. O Decorator (A magia acontece aqui!)
public class ColorClothingDecorator : IClothingVisual
{
    private readonly IClothingVisual wrappedVisual;
    private readonly Color appliedColor;

    // Construtor: Recebe qualquer visual existente e a nova cor a aplicar
    public ColorClothingDecorator(IClothingVisual wrappedVisual, Color appliedColor)
    {
        this.wrappedVisual = wrappedVisual;
        this.appliedColor = appliedColor;
    }

    // O Decorator não mexe no formato da roupa, apenas repassa o Sprite que está embrulhado
    public Sprite GetSprite() => wrappedVisual.GetSprite();

    // DECORAÇÃO: Em vez de devolver a cor original, ele injeta a nova cor escolhida
    public Color GetColor() => appliedColor;
}