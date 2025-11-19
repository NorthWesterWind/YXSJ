using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using World.View.UI;

namespace Utils
{
    public class UIController : MonoSingleton<UIController>
    {
        [DictionaryDrawerSettings(KeyLabel = "Key", ValueLabel = "Value")]
        public Dictionary<Type, BaseView> _uiPanels = new();

        public List<BaseView> UIPanels = new List<BaseView>();

        private Stack<BaseView> _uiStack = new();


        public override void Awake()
        {
            base.Awake();
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnSceneChanged(Scene oldScene, Scene newScene)
        {
            // 场景切换时清理缓存
            _uiPanels.Clear();
            _uiStack.Clear();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Back();
            }
        }

        /// <summary>
        /// 显示 UI 面板
        /// </summary>
        public async void Show<T>(params object[] args) where T : BaseView
        {
            var type = typeof(T);
            // 检查缓存是否有效
            if (!_uiPanels.TryGetValue(type, out var view) || view == null || view.gameObject == null)
            {
              
                Transform _canvas = GameObject.Find("Canvas")?.transform;
                Transform _popCanvas = GameObject.Find("Canvas")?.transform;
                if (_canvas != null || _popCanvas != null)
                {
                    var go = _canvas.GetComponentsInChildren<Transform>(true) // ✅ true = 包含失活物体
                        .FirstOrDefault(t => t.name == type.Name)?.gameObject;
                    var go_1 = _popCanvas.GetComponentsInChildren<Transform>(true) // ✅ true = 包含失活物体
                        .FirstOrDefault(t => t.name == type.Name)?.gameObject;
                    if (go != null)
                    {
                        var _view = go.GetComponent<T>();
                        if (_view != null)
                        {
                            view = _view;
                            _uiPanels[type] = view;
                        }
                    }else if (go_1 != null)
                    {
                        var _view = go.GetComponent<T>();
                        if (_view != null)
                        {
                            view = _view;
                            _uiPanels[type] = view;
                        }
                    }
                    else
                    {
                        // 动态加载
                        try
                        {
                            var ui = await ResourceLoader.Instance.LoadUIAsync<GameObject>(type.Name);
                            if (ui == null)
                            {
                                Debug.LogError($"加载UI失败：{type.Name}");
                                return;
                            }
                            Transform parent = typeof(T).IsSubclassOf(typeof(PopBaseView)) ? _popCanvas : _canvas;
                            view = ui.GetComponent<T>();
                            ui.transform.SetParent(parent, false);
                            if (view == null)
                            {
                                Debug.LogError($"UI {type.Name} 缺少 {typeof(T).Name} 组件！");
                                ResourceLoader.Instance.ReleaseAsset(type.Name);
                                return;
                            }
                            _uiPanels[type] = view;
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"加载 UI 失败: {type.Name}, {e}");
                            return;
                        }
                    }
                }
               
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
        
        /// <summary>
        /// 隐藏 UI 面板
        /// </summary>
        public void Hide<T>() where T : BaseView
        {
            var type = typeof(T);
            if (_uiPanels.TryGetValue(type, out var view) && view != null && view.gameObject != null)
            {
                Hide(type, view);
            }
            else
            {
                // 如果 UI 已被销毁就从缓存移除
                _uiPanels.Remove(type);
            }
        }

        private void Hide(Type type, BaseView view)
        {
            view.HideImmediate();
            view.gameObject.SetActive(false);
        }

        /// <summary>
        /// 返回（关闭栈顶 UI）
        /// </summary>
        public void Back()
        {
            if (_uiStack.Count > 0)
            {
                var top = _uiStack.Pop();
                if (top != null)
                {
                    top.Hide();
                }
                
            }
        }
        

        /// <summary>
        /// 移除并释放 UI
        /// </summary>
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
                        if (v != view) newStack.Push(v);
                    }

                    _uiStack.Clear();
                    foreach (var v in newStack) _uiStack.Push(v);
                }

                Addressables.ReleaseInstance(view.gameObject);
                _uiPanels.Remove(type);
            }
        }
    }
}