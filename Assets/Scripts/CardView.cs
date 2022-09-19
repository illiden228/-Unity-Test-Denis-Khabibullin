using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CardView : BaseMonoBehaviour
{
    [SerializeField] private TMP_Text _hpText;
    [SerializeField] private TMP_Text _manaText;
    [SerializeField] private TMP_Text _attackText;
    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private TMP_Text _descriotionText;
    [SerializeField] private RawImage _image;
    [SerializeField] private float _animationDuration;

    private List<IDisposable> _subscribes = new List<IDisposable>();
    private ReactiveProperty<int> _currentHp;
    private ReactiveProperty<int> _currentMana;
    private ReactiveProperty<int> _currentAttack;
    
    public ReactiveProperty<bool> IsSelectPossibile { get; private set; }
    public RawImage Image => _image;
    public TMP_Text Title => _titleText;
    public TMP_Text Description => _descriotionText;
    
    public void Init(int hp, int mana, int attack)
    {
        _currentHp = new ReactiveProperty<int>(hp);
        _currentMana = new ReactiveProperty<int>(mana);
        _currentAttack = new ReactiveProperty<int>(attack);

        AddTextSubscribe(_currentHp, _hpText);
        AddTextSubscribe(_currentMana, _manaText);
        AddTextSubscribe(_currentAttack, _attackText);

        IsSelectPossibile = new ReactiveProperty<bool>(true);
    }

    public void SetNewHp(int newValue, Action onComplete)
    {
        AnimateValueFrom(_currentHp, newValue, onComplete);
    }
    
    public void SetNewMana(int newValue, Action onComplete)
    {
        AnimateValueFrom(_currentMana, newValue, onComplete);
    }
    
    public void SetNewAttack(int newValue, Action onComplete)
    {
        AnimateValueFrom(_currentAttack, newValue, onComplete);
    }

    private void AnimateValueFrom(ReactiveProperty<int> property, int newValue, Action onComplete)
    {
        DOTween.To(() => property.Value, 
                x => property.Value = x, 
                newValue, _animationDuration)
            .OnComplete(() => onComplete?.Invoke());
    }

    private void AddTextSubscribe(ReactiveProperty<int> property, TMP_Text label)
    {
        IDisposable newSubscribe = property?.Subscribe(value =>
        {
            label.text = property.Value.ToString();
        });
        
        _subscribes.Add(newSubscribe);
    }

    private void OnDisable()
    {
        foreach (var subscribe in _subscribes)
        {
            subscribe.Dispose();
        }
    }
}