using System;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSelector : BaseDisposable
{
    private ReactiveCollection<CardModel> _cards;

    private Dictionary<CardView, LayoutElement> _cardLayouts =
        new Dictionary<CardView, LayoutElement>();

    private Dictionary<CardView, List<ObservableTriggerBase>> _cardTriggers =
        new Dictionary<CardView, List<ObservableTriggerBase>>();

    private Dictionary<CardView, List<IDisposable>> _cardSubscribes =
        new Dictionary<CardView, List<IDisposable>>();

    private float _animationDuration;
    private float _upOffset;
    private float _layoutAnimationDuration;
    private ReactiveProperty<bool> _isFreezeLayout;
    private int _indexSelectedCard;
    private Transform _parentSelectedCard;

    private CardModel _selectedCard;
    private int _draggingCardIndex;

    public CardSelector(ReactiveCollection<CardModel> cards, ReactiveProperty<bool> isFreezeLayout,
        float animationDuration, float upOffset, float layoutAnimationDuration)
    {
        _cards = cards;
        _isFreezeLayout = isFreezeLayout;
        _animationDuration = animationDuration;
        _upOffset = upOffset;
        _layoutAnimationDuration = layoutAnimationDuration;

        foreach (CardModel card in _cards)
        {
            AddCardTriggers(card);
            AddCardLayout(card);
        }

        AddDispose(_cards.ObserveRemove().Subscribe(OnCardRemoved));
        AddDispose(_cards.ObserveAdd().Subscribe(OnCardAdded));
    }

    private void OnCardRemoved(CollectionRemoveEvent<CardModel> removeEvent)
    {
        CardView deletedCard = removeEvent.Value.View;
        GameObject.Destroy(_cardLayouts[deletedCard]);
        _cardLayouts.Remove(deletedCard);

        var triggers = _cardTriggers[deletedCard];
        foreach (var trigger in triggers)
            GameObject.Destroy(trigger);

        _cardTriggers.Remove(deletedCard);

        var subscribes = _cardSubscribes[deletedCard];
        foreach (var subscribe in subscribes)
            subscribe.Dispose();
        _cardSubscribes.Remove(deletedCard);
    }

    private void OnCardAdded(CollectionAddEvent<CardModel> addEvent)
    {
        AddCardTriggers(addEvent.Value);
        AddCardLayout(addEvent.Value);
    }

    private void AddCardLayout(CardModel cardModel)
    {
        CardView card = cardModel.View;
        LayoutElement layout;
        if (!card.TryGetComponent(out layout))
            layout = card.AddComponent<LayoutElement>();
        _cardLayouts.Add(card, layout);
    }


    private void AddCardTriggers(CardModel cardModel)
    {
        CardView card = cardModel.View;
        var enterTrigger = card.gameObject.AddComponent<ObservablePointerEnterTrigger>();
        var subscribeEnterTrigger = enterTrigger.OnPointerEnterAsObservable()
            .Subscribe(_ => SelectCard(cardModel));

        var exitTrigger = card.gameObject.AddComponent<ObservablePointerExitTrigger>();
        var subscribeExitTrigger = exitTrigger.OnPointerExitAsObservable()
            .Subscribe(_ => UnselectCard(cardModel));

        var beginDragTrigger = card.gameObject.AddComponent<ObservableBeginDragTrigger>();
        var beginDragSubscribe = beginDragTrigger.OnBeginDragAsObservable()
            .Subscribe(data => OnBeginDrag(cardModel, data));

        var dragTrigger = card.gameObject.AddComponent<ObservableDragTrigger>();
        var dragSubscribe = dragTrigger.OnDragAsObservable()
            .Subscribe(data => OnDrag(cardModel, data));

        var endDragTrigger = card.gameObject.AddComponent<ObservableEndDragTrigger>();
        var endDragSubscribe = endDragTrigger.OnEndDragAsObservable()
            .Subscribe(data => OnEndDrag(cardModel, data));

        List<ObservableTriggerBase> triggers = new List<ObservableTriggerBase>();
        triggers.Add(enterTrigger);
        triggers.Add(exitTrigger);
        triggers.Add(beginDragTrigger);
        triggers.Add(dragTrigger);
        triggers.Add(endDragTrigger);
        _cardTriggers.Add(card, triggers);

        List<IDisposable> subscribes = new List<IDisposable>();
        subscribes.Add(subscribeEnterTrigger);
        subscribes.Add(subscribeExitTrigger);
        subscribes.Add(beginDragSubscribe);
        subscribes.Add(dragSubscribe);
        subscribes.Add(endDragSubscribe);
        _cardSubscribes.Add(card, subscribes);
    }

    private void SelectCard(CardModel cardModel)
    {
        CardView card = cardModel.View;

        if (card.IsSelectPossibile.Value == false)
            return;
        _selectedCard = cardModel;
        Transform cardTransform = card.transform;
        _isFreezeLayout.Value = true;
        _cardLayouts[card].ignoreLayout = true;
        _indexSelectedCard = cardTransform.GetSiblingIndex();
        _parentSelectedCard = cardTransform.parent;
        cardTransform.SetSiblingIndex(_parentSelectedCard.childCount);

        cardTransform.DOKill();
        cardTransform.DOLocalRotate(Vector3.zero, _animationDuration);

        var newPos = card.transform.localPosition;
        newPos.y += _upOffset;
        card.transform.DOLocalMove(newPos, _animationDuration);
    }

    private void UnselectCard(CardModel cardModel)
    {
        CardView card = cardModel.View;
        if (card.IsSelectPossibile.Value == false)
            return;
        if (_selectedCard != cardModel)
            return;
        card.transform.SetSiblingIndex(_indexSelectedCard);
        _cardLayouts[card].ignoreLayout = false;
        _isFreezeLayout.Value = false;
    }

    private void OnBeginDrag(CardModel cardModel, PointerEventData pointerData)
    {
        if (_selectedCard != cardModel)
            return;

        CardView card = cardModel.View;
        card.IsSelectPossibile.Value = false;
        card.transform.SetParent(card.transform.root);
        card.transform.position = pointerData.position;
        _cardLayouts[card].ignoreLayout = false;
        _isFreezeLayout.Value = false;
    }

    private void OnDrag(CardModel cardModel, PointerEventData pointerData)
    {
        if (_selectedCard != cardModel)
            return;

        CardView card = cardModel.View;
        card.transform.position = pointerData.position;
    }

    private void OnEndDrag(CardModel cardModel, PointerEventData pointerData)
    {
        if (_selectedCard != cardModel)
            return;

        CardView card = cardModel.View;

        var container = EventSystem.current.GetFirstComponentUnderPointer<ICardContainer>(pointerData);
        if (container != null)
        {
            container.TakeCard(cardModel);
            _cards.Remove(cardModel);
        }
        else
        {
            Transform cardTransform = cardModel.View.transform;
            cardTransform.SetParent(_parentSelectedCard);
            cardTransform.SetSiblingIndex(_indexSelectedCard);
        }

        _selectedCard = null;
        AddDispose(Observable.Timer(TimeSpan.FromSeconds(_layoutAnimationDuration)).Take(1)
            .Subscribe(_ => card.IsSelectPossibile.Value = true));
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        foreach (var cardSubscribes in _cardSubscribes)
        {
            foreach (var cardSubscribe in cardSubscribes.Value)
            {
                cardSubscribe?.Dispose();
            }
        }
    }
}