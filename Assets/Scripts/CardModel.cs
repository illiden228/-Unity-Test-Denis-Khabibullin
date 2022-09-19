using System;
using UniRx;
using UnityEngine;

public class CardModel : BaseDisposable
{
    private CardView _view;
    private IDisposable _imageLoading;
    
    public ReactiveProperty<int> Hp { get; private set; }
    public ReactiveProperty<int> Mana { get; private set; }
    public ReactiveProperty<int> Attack { get; private set; }
    public CardView View => _view;

    public CardModel(CardView view, int hp, int mana, int attack, string title, string desc, ReactiveProperty<Texture> image)
    {
        _view = view;
        Hp = new ReactiveProperty<int>(hp);
        Mana = new ReactiveProperty<int>(mana);
        Attack = new ReactiveProperty<int>(attack);

        _imageLoading = image?.Subscribe(value =>
        {
            if (value != null)
            {
                _view.Image.texture = value;
                _imageLoading?.Dispose();
            }
        });
        
        _view.Title.text = title;
        _view.Description.text = desc;
        _view.Init(hp, mana, attack);
    }

    public void ChangeHp(int value)
    {
        _view.SetNewHp(value,() => Hp.Value = value);
    }

    public void ChangeMana(int value)
    {
        _view.SetNewMana(value,() => Mana.Value = value);
    }

    public void ChangeAttack(int value)
    {
        _view.SetNewAttack(value,() => Attack.Value = value);
    }
}