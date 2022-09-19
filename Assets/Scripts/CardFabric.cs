using UniRx;
using UnityEngine;
using UnityEngine.Networking;

public class CardFabric : BaseDisposable
{
    private GameObject _cardPrefab;
    private Transform _parent;
    private int _poolCapacity;
    private IPoolObject<CardView> _cardsPool;
    private CardDefaultSettings _cardDefaultSettings;
    private const string CARD_DEFAULT_SETTINGS_PATH = "CardDefaultSettings";

    public CardFabric(GameObject cardPrefab, int poolCapacity)
    {
        _cardPrefab = cardPrefab;
        _poolCapacity = poolCapacity;
        _parent = new GameObject("CardsPool").transform;
        _cardDefaultSettings = Resources.Load<CardDefaultSettings>(CARD_DEFAULT_SETTINGS_PATH);

        Init();
    }

    private void Init()
    {
        PoolObject<CardView>.Ctx poolCtx = new PoolObject<CardView>.Ctx
        {
            Parent = _parent,
            Prefab = _cardPrefab,
            StartCapacity = _poolCapacity
        };
        _cardsPool = new PoolObject<CardView>(poolCtx);
        AddDispose(_cardsPool);
    }

    public CardModel GetRandom()
    {
        ReactiveProperty<Texture> image = new ReactiveProperty<Texture>();

        UnityWebRequest spriteRequest = UnityWebRequestTexture.GetTexture(_cardDefaultSettings.ImageDownloadLink);

        spriteRequest.SendWebRequest().completed += operarion =>
        {
            if (spriteRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download card sprite");
            }
            else
            {
                var texture = DownloadHandlerTexture.GetContent(spriteRequest);
                image.Value = texture;
            }
        };
        
        CardView newCardViewView = _cardsPool.Get();
        CardModel newCardModel = new CardModel(
            newCardViewView, 
            _cardDefaultSettings.Hp, 
            _cardDefaultSettings.Mana,
            _cardDefaultSettings.Attack,
            _cardDefaultSettings.Title, 
            _cardDefaultSettings.Description,
            image);

        return newCardModel;
    }

    public void Return(CardModel cardModel)
    {
        _cardsPool.Return(cardModel.View.gameObject);
    }

    protected override void OnDispose()
    {
        base.OnDispose();
        GameObject.Destroy(_parent);
    }
}