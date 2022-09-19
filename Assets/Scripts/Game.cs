using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Game : BaseMonoBehaviour
{
    [SerializeField] private Button _changeValueButton;
    [SerializeField] private CurveAnimatedLayout _layout;
    [SerializeField] private float _selectAnimationDuration;
    [SerializeField] private float _selectUpOffset;

    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _capacity;
    [SerializeField] private Vector2Int _range;

    private CardFabric _fabric;
    private ReactiveCollection<CardModel> _cards;
    private CardModel _selectedCardView;

    private void Start()
    {
        _fabric = new CardFabric(_prefab, _capacity).AddTo(transform);
        int randomCount = Random.Range(_range.x, _range.y + 1);
        _cards = new ReactiveCollection<CardModel>();

        for (int i = 0; i < randomCount; i++)
        {
            AddNewCard();
        }

        Damager damager = new Damager(_cards, -2, 9).AddTo(transform);

        _changeValueButton.OnClickAsObservable().Subscribe(_ => damager.ChangeCurrentCardRandomValue()).AddTo(transform);

        CardSelector cardSelector = new CardSelector(
            _cards, 
            _layout.IsFreeze,
            _selectAnimationDuration,
            _selectUpOffset,
            _layout.TimeToChange).AddTo(transform);
    }

    private void AddNewCard()
    {
        CardModel newCard = _fabric.GetRandom().AddTo(transform);
        newCard.View.transform.SetParent(_layout.transform);
        newCard.View.gameObject.SetActive(true);

        newCard.Hp.Subscribe(value =>
        {
            if (value < 1)
            {
                _cards.Remove(newCard);
                _fabric.Return(newCard);
                newCard.Dispose();
            }
        }).AddTo(transform);

        _cards.Add(newCard);
    }
}