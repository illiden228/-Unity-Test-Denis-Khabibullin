using System;
using System.Collections.Generic;
using UniRx;
using Random = UnityEngine.Random;

public class Damager : BaseDisposable
{
    private ReactiveCollection<CardModel> _cards;
    private CardModel _selectedCard;
    private int _selectedIndex = 0;
    private List<Action<int>> _actions = new List<Action<int>>();
    private int _minValue;
    private int _maxValue;

    public Damager(ReactiveCollection<CardModel> cards, int min, int max)
    {
        _cards = cards;
        _minValue = min;
        _maxValue = max;

        SetCard();
        AddDispose(_cards.ObserveRemove().Subscribe(OnCardRemoved));
    }

    public CardModel ChangeCurrentCardRandomValue()
    {
        CardModel damagableCard = _selectedCard;
        int randomActionIndex = Random.Range(0, _actions.Count);
        int randomValue = Random.Range(_minValue, _maxValue + 1);
        _actions[randomActionIndex]?.Invoke(randomValue);

        NextCard();

        return damagableCard;
    }

    private void OnCardRemoved(CollectionRemoveEvent<CardModel> removeEvent)
    {
        SetCard();
    }

    private void NextCard()
    {
        _selectedIndex++;
        SetCard();
    }

    private void SetCard()
    {
        if(_cards.Count <= 0)
            return;
        if (_selectedIndex >= _cards.Count)
            _selectedIndex = 0;

        _selectedCard = _cards[_selectedIndex];
        
        _actions.Clear();
        _actions.Add(_selectedCard.ChangeAttack);
        _actions.Add(_selectedCard.ChangeHp);
        _actions.Add(_selectedCard.ChangeMana);
    }
}