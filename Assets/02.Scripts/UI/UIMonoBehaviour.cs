using T.GameSystems;
using T.EventSystems;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace T.UI
{
    public abstract class UIMonoBehaviour : MonoBehaviour, IUI, IInitializable
    {
        public int sortingOrder 
        { 
            get => canvas.sortingOrder;
            set => canvas.sortingOrder = value;
        }
        public bool inputActionEnabled { get; set; }

        public event Action onShow;
        public event Action onHide;

        protected Canvas canvas;
        [SerializeField] private bool _hideWhenPointerDownOutside = true;
        [SerializeField] private bool _allowOtherCanvasInputAction = true;

        public virtual void InputAction()
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                if (CustomInputModule.main.TryGetHovered<GraphicRaycaster, CanvasRenderer>
                    (out CanvasRenderer hovered))
                {
                    if (hovered.transform.root.TryGetComponent(out UIMonoBehaviour ui) &&
                        ui != this)
                    {
                        OnOtherCanvasSelected(ui);
                    }
                }
            }
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            onShow?.Invoke();
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
            onHide?.Invoke();
        }

        public virtual void Init()
        {
            canvas = GetComponent<Canvas>();
            UIManager.instance.Register(this);

            if (_hideWhenPointerDownOutside)
                CreateOutsidePanel();
        }

        private void Update()
        {
            if (inputActionEnabled)
                InputAction();
        }

        protected virtual void OnOtherCanvasSelected(UIMonoBehaviour other)
        {
            if (_allowOtherCanvasInputAction)
                other.InputAction();
        }

        private void CreateOutsidePanel()
        {
            GameObject outsidePanel = new GameObject("Outside");
            outsidePanel.transform.SetParent(transform);
            outsidePanel.transform.SetAsFirstSibling();
            Image image = outsidePanel.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.5f);

            RectTransform rectTransform = outsidePanel.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.localScale = Vector3.one;

            EventTrigger trigger = outsidePanel.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener(eventData => Hide());
            trigger.triggers.Add(entry);
        }
    }
}