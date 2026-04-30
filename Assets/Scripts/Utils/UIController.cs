using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Utils
{
    public class UIController : MonoSingleton<UIController>
    {
        [DictionaryDrawerSettings(KeyLabel = "Key", ValueLabel = "Value")]
        public Dictionary<Type, BaseView> _uiPanels = new();

        public List<BaseView> UIPanels = new();

        private readonly Stack<BaseView> _uiStack = new();
        private readonly Dictionary<string, Transform> _canvasCache = new();
        private readonly Dictionary<string, GameObject> _scenePanelCache = new();

        public override void Awake()
        {
            base.Awake();
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            _uiPanels.Clear();
            _uiStack.Clear();
            _canvasCache.Clear();
            _scenePanelCache.Clear();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Back();
            }
        }

        public async void Show<T>(params object[] args) where T : BaseView
        {
            var type = typeof(T);
            if (!_uiPanels.TryGetValue(type, out var view) || view == null || view.gameObject == null)
            {
                var canvas = GetCanvasTransform("Canvas");
                var popCanvas = GetCanvasTransform("PopupCanvas") ?? canvas;

                if (canvas == null && popCanvas == null)
                {
                    Debug.LogError($"UI {type.Name} failed: Canvas not found.");
                    return;
                }

                var existingPanel = FindExistingPanel(type.Name);
                if (existingPanel != null)
                {
                    view = existingPanel.GetComponent<T>();
                    if (view != null)
                    {
                        _uiPanels[type] = view;
                    }
                }
                else
                {
                    try
                    {
                        var ui = await ResourceLoader.Instance.LoadUIAsync<GameObject>(type.Name);
                        if (ui == null)
                        {
                            Debug.LogError($"Load UI failed: {type.Name}");
                            return;
                        }

                        view = ui.GetComponent<T>();
                        if (view == null)
                        {
                            Debug.LogError($"UI {type.Name} is missing component {typeof(T).Name}");
                            Destroy(ui);
                            return;
                        }

                        var parent = view.IsPopup ? popCanvas : canvas;
                        if (parent == null)
                        {
                            Debug.LogError($"UI {type.Name} failed: target canvas not found.");
                            Destroy(ui);
                            return;
                        }

                        ui.transform.SetParent(parent, false);
                        RegisterScenePanel(type.Name, ui);
                        _uiPanels[type] = view;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Load UI failed: {type.Name}, {e}");
                        return;
                    }
                }
            }

            if (view == null || view.gameObject == null)
            {
                Debug.LogError($"UI {type.Name} failed to initialize.");
                return;
            }

            view.gameObject.SetActive(true);
            view.ShowImmediate();
            view.UpdateViewWithArgs(args);
            view.transform.SetAsLastSibling();

            if (view.IsPopup)
            {
                _uiStack.Push(view);
            }
        }

        public async void Preload<T>() where T : BaseView
        {
            var type = typeof(T);

            if (_uiPanels.TryGetValue(type, out var loadedView) && loadedView != null && loadedView.gameObject != null)
            {
                return;
            }

            if (_scenePanelCache.TryGetValue(type.Name, out var scenePanel) && scenePanel != null)
            {
                return;
            }

            try
            {
                await ResourceLoader.Instance.PreloadAssetAsync<GameObject>(type.Name);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Preload UI failed: {type.Name}, {e.Message}");
            }
        }

        public void Hide<T>() where T : BaseView
        {
            var type = typeof(T);
            if (_uiPanels.TryGetValue(type, out var view) && view != null && view.gameObject != null)
            {
                Hide(type, view);
            }
            else
            {
                _uiPanels.Remove(type);
            }
        }

        private void Hide(Type type, BaseView view)
        {
            view.HideImmediate();
            view.gameObject.SetActive(false);
        }

        public void Back()
        {
            if (_uiStack.Count <= 0)
            {
                return;
            }

            var top = _uiStack.Pop();
            if (top != null)
            {
                top.Hide();
            }
        }

        public void Remove<T>() where T : BaseView
        {
            var type = typeof(T);
            if (_uiPanels.TryGetValue(type, out var view) && view != null)
            {
                if (view.IsPopup && _uiStack.Contains(view))
                {
                    var newStack = new Stack<BaseView>(_uiStack.Count);
                    foreach (var v in _uiStack)
                    {
                        if (v != view)
                        {
                            newStack.Push(v);
                        }
                    }

                    _uiStack.Clear();
                    foreach (var v in newStack)
                    {
                        _uiStack.Push(v);
                    }
                }

                _scenePanelCache.Remove(type.Name);
                Addressables.ReleaseInstance(view.gameObject);
                _uiPanels.Remove(type);
            }
        }

        private Transform GetCanvasTransform(string canvasName)
        {
            if (_canvasCache.TryGetValue(canvasName, out var cachedTransform) && cachedTransform != null)
            {
                return cachedTransform;
            }

            var canvasTransform = GameObject.Find(canvasName)?.transform;
            if (canvasTransform != null)
            {
                _canvasCache[canvasName] = canvasTransform;
            }

            return canvasTransform;
        }

        private GameObject FindExistingPanel(string panelName)
        {
            if (_scenePanelCache.TryGetValue(panelName, out var cachedPanel) && cachedPanel != null)
            {
                return cachedPanel;
            }

            CachePanelsUnderRoot(GetCanvasTransform("Canvas"));
            CachePanelsUnderRoot(GetCanvasTransform("PopupCanvas"));

            if (_scenePanelCache.TryGetValue(panelName, out cachedPanel) && cachedPanel != null)
            {
                return cachedPanel;
            }

            return null;
        }

        private void CachePanelsUnderRoot(Transform root)
        {
            if (root == null)
            {
                return;
            }

            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < transforms.Length; i++)
            {
                var current = transforms[i];
                if (!_scenePanelCache.ContainsKey(current.name))
                {
                    _scenePanelCache[current.name] = current.gameObject;
                }
            }
        }

        private void RegisterScenePanel(string panelName, GameObject panel)
        {
            if (panel != null)
            {
                _scenePanelCache[panelName] = panel;
            }
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }
    }
}
