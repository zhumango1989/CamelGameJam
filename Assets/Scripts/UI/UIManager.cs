using UnityEngine;
using System.Collections.Generic;
using GameJam.Core;

namespace GameJam.UI
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private Transform panelContainer;

        private readonly Dictionary<string, UIPanel> _panels = new();
        private readonly Stack<UIPanel> _panelStack = new();

        public Canvas MainCanvas => mainCanvas;

        protected override void Awake()
        {
            base.Awake();

            if (mainCanvas == null)
            {
                mainCanvas = FindFirstObjectByType<Canvas>();
            }

            if (mainCanvas != null && panelContainer == null)
            {
                panelContainer = mainCanvas.transform;
            }
        }

        public void RegisterPanel(string panelName, UIPanel panel)
        {
            if (!_panels.ContainsKey(panelName))
            {
                _panels.Add(panelName, panel);
            }
        }

        public void UnregisterPanel(string panelName)
        {
            _panels.Remove(panelName);
        }

        public T GetPanel<T>(string panelName) where T : UIPanel
        {
            if (_panels.TryGetValue(panelName, out var panel))
            {
                return panel as T;
            }
            return null;
        }

        public void ShowPanel(string panelName, bool pushToStack = true)
        {
            if (_panels.TryGetValue(panelName, out var panel))
            {
                panel.Show();
                if (pushToStack && !_panelStack.Contains(panel))
                {
                    _panelStack.Push(panel);
                }
            }
        }

        public void HidePanel(string panelName)
        {
            if (_panels.TryGetValue(panelName, out var panel))
            {
                panel.Hide();
            }
        }

        public void GoBack()
        {
            if (_panelStack.Count > 0)
            {
                var currentPanel = _panelStack.Pop();
                currentPanel.Hide();

                if (_panelStack.Count > 0)
                {
                    _panelStack.Peek().Show();
                }
            }
        }

        public void HideAll()
        {
            foreach (var panel in _panels.Values)
            {
                panel.Hide();
            }
            _panelStack.Clear();
        }

        public T CreatePanel<T>(string panelName, T prefab) where T : UIPanel
        {
            if (_panels.ContainsKey(panelName))
            {
                return _panels[panelName] as T;
            }

            var panel = Instantiate(prefab, panelContainer);
            panel.name = panelName;
            RegisterPanel(panelName, panel);
            return panel;
        }
    }

    public abstract class UIPanel : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected bool hideOnStart = true;

        public bool IsVisible { get; private set; }

        protected virtual void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        protected virtual void Start()
        {
            if (hideOnStart)
            {
                Hide();
            }
        }

        public virtual void Show()
        {
            IsVisible = true;
            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            OnShow();
        }

        public virtual void Hide()
        {
            IsVisible = false;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            gameObject.SetActive(false);
            OnHide();
        }

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }
    }
}
