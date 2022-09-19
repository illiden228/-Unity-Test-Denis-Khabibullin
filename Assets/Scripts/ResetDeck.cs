using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HorizontalLayoutGroup))]
public class ResetDeck : BaseMonoBehaviour, ICardContainer
{
    public void TakeCard(CardModel card)
    {
        card.View.transform.SetParent(transform);
    }
}