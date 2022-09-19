using UnityEngine;

[CreateAssetMenu]
public class CardDefaultSettings : ScriptableObject
{
    [SerializeField] private int _hp;
    [SerializeField] private int _mana;
    [SerializeField] private int _attack;
    [SerializeField] private string _title;
    [SerializeField] private string _description;
    [SerializeField] private string _imageDownloadLink;

    public int Hp => _hp;

    public int Mana => _mana;

    public int Attack => _attack;

    public string Title => _title;

    public string Description => _description;

    public string ImageDownloadLink => _imageDownloadLink;
}