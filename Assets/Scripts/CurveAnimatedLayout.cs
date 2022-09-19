using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CurveAnimatedLayout : LayoutGroup
{
    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private float _minDistanceBetweenObjects;
    [SerializeField] private bool _rotateToCenter;
    [SerializeField] private bool _anmateMoveAndRotate;
    [SerializeField] private float _timeToChange;
    private IDisposable _freezeSubscribe;
    
    public ReactiveProperty<bool> IsFreeze { get; private set; }
    public float TimeToChange => _timeToChange;

    protected override void OnEnable()
    {
        DOTween.Init();
        IsFreeze = new ReactiveProperty<bool>();
        base.OnEnable();
        
        _freezeSubscribe = IsFreeze.Subscribe(value =>
        {
            if (!value)
            {
                CalculateRadial();
            }
        });
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _freezeSubscribe?.Dispose();
    }

    public override void SetLayoutHorizontal()
    {
        
    }

    public override void SetLayoutVertical()
    {
        
    }

    public override void CalculateLayoutInputVertical()
    {
        if(IsFreeze.Value)
            return;
        
        CalculateRadial();
    }

    public override void CalculateLayoutInputHorizontal()
    {
        if(IsFreeze.Value)
            return;
        
        CalculateRadial();
    }
#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        CalculateRadial();
    }
#endif
    private void CalculateRadial()
    {
        SetDirty();
        m_Tracker.Clear();
        int count = transform.childCount;

        if (count == 0)
            return;

        RectTransform rectTrandform = (RectTransform)transform;
        float width = rectTrandform.rect.size.x;
        float height = rectTrandform.rect.size.y;
        float arcHeight = height / 2;

        var evaluationStep = 1f / (count + 1);
        float stepPosX = Mathf.Min(_minDistanceBetweenObjects, width / (count + 1));
        Vector3 startPosition = new Vector3(-(stepPosX * (((float)count - 1) / 2)), -height / 2);

        float horde = stepPosX * (count - 1);
        float radius = arcHeight / 2 + ((horde * horde) / (8 * arcHeight));
        Vector3 centerPoint = new Vector3(width / 2, arcHeight - radius);

        for (int i = 0; i < count; i++)
        {
            if (!(transform.GetChild(i) is RectTransform child))
                continue;
            if (child == null)
                continue;
            if (child.TryGetComponent(out LayoutElement element) && element.ignoreLayout)
                continue;

            m_Tracker.Add(this, child,
                DrivenTransformProperties.Anchors |
                DrivenTransformProperties.AnchoredPosition |
                DrivenTransformProperties.Pivot);
            child.pivot = rectTrandform.pivot;

            Vector3 childNewLocalPosition = CalcNewPosition();

            var childNewLocalRotation = Quaternion.FromToRotation(Vector3.up, child.position - centerPoint);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                SetNewValues();
            }
#endif
            if (_anmateMoveAndRotate)
            {
                child.DOLocalMove(childNewLocalPosition, _timeToChange);
                if (_rotateToCenter)
                    child.DOLocalRotateQuaternion(childNewLocalRotation, _timeToChange);
            }
            else
            {
                SetNewValues();
            }

            Vector3 CalcNewPosition()
            {
                float newPosX = startPosition.x + stepPosX * i;
                float newPosY = -arcHeight + child.rect.height / 2 +
                                _curve.Evaluate(evaluationStep + evaluationStep * i) * arcHeight;
                return new Vector3(newPosX, newPosY);
            }

            void SetNewValues()
            {
                child.transform.localPosition = childNewLocalPosition;
                if (_rotateToCenter)
                    child.localRotation = childNewLocalRotation;
            }
        }
    }
}