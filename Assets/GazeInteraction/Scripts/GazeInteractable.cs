#region Includes
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
#endregion



namespace TS.GazeInteraction
{
    /// <summary>
    /// Component applied to GameObjects that can be interacted with using the gaze.
    /// </summary>
    public class GazeInteractable : MonoBehaviour
    {
        #region Variables

        private const string WAIT_TO_EXIT_COROUTINE = "WaitToExit_Coroutine";

        public delegate void OnEnter(GazeInteractable interactable, GazeInteractor interactor, Vector3 point);
        public event OnEnter Enter;

        public delegate void OnStay(GazeInteractable interactable, GazeInteractor interactor, Vector3 point);
        public event OnStay Stay;

        public delegate void OnExit(GazeInteractable interactable, GazeInteractor interactor);
        public event OnExit Exit;

        public delegate void OnActivated(GazeInteractable interactable);
        public event OnActivated Activated;

        [Header("Configuration")]
        [SerializeField] private bool _isActivable;
        [SerializeField] private float _exitDelay;

        [Header("Events")]
        public UnityEvent OnGazeEnter;
        public UnityEvent OnGazeStay;
        public UnityEvent OnGazeExit;
        public UnityEvent OnGazeActivated;
        public UnityEvent<bool> OnGazeToggle;

        public bool IsEnabled
        {
            /// 公共属性（Property），提供了对 _collider 组件的 enabled 属性的访问和设置。enabled 属性用于控制物体是否启用碰撞器组件。
            get { return _collider.enabled; }
            set { _collider.enabled = value; }
        }
        public bool IsActivable
        {
            get { return _isActivable; }
        }
        public bool IsActivated { get; private set; }

        private Collider _collider;

        #endregion

        private void Awake()
        {

            /// <summary>
            /// 获取 可交互对象 的Collider（碰撞体），及早地发现并解决缺失碰撞器的问题，以避免出现潜在的错误和运行时异常。
            /// </summary>

            _collider = GetComponent<Collider>();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if(_collider == null) { throw new System.Exception("Missing Collider"); }
#endif
        }
        private void Start()
        {
            /// enabled 属性是 MonoBehaviour 类的一个属性，用于控制脚本是否启用。
            enabled = false;
        }

        /// <summary>
        ///  Toggles the GameObject. 切换游戏对象。
        /// </summary>
        /// <param name="enable"></param>
        public void Enable(bool enable)
        {
            gameObject.SetActive(enable);
        }

        /// <summary>
        /// Invokes the Activated events. 调用激活事件。
        /// </summary>
        public void Activate()
        {
            IsActivated = true;

            Activated?.Invoke(this); /// 这行代码触发了 Activated 事件，并传递了当前的 GazeInteractable 实例作为参数。
            /// ?.Invoke()，可以确保在事件没有注册任何处理程序时不会引发空引用异常。 相当于，多了一个判断，判断是否有这个事件
            OnGazeActivated?.Invoke();
            /// 这两个事件可以用于通知其他脚本或系统，当前的 GazeInteractable 实例已被激活。
        }

        /// <summary>
        /// Called by the GazeInteractor when the gaze enters this Interactable.
        /// Invokes the Enter events.
        /// </summary>
        /// <param name="interactor"></param> interactor 表示注视交互器
        /// <param name="point"></param> point 表示注视交互发生的点（位置）
        public void GazeEnter(GazeInteractor interactor, Vector3 point) 
        {
            StopCoroutine(WAIT_TO_EXIT_COROUTINE); ///以避免在进入和离开之间发生冲突。

            Enter?.Invoke(this, interactor, point);

            OnGazeEnter?.Invoke();
            OnGazeToggle?.Invoke(true);
        }
        /// <summary>
        /// Called by the GazeInteractor while the gaze stays on top of this Interactable.
        /// Invokes the Stay events.
        /// </summary>
        /// <param name="interactor"></param>
        /// <param name="point"></param>
        public void GazeStay(GazeInteractor interactor, Vector3 point)
        {
            Stay?.Invoke(this, interactor, point);

            OnGazeStay?.Invoke();
        }
        /// <summary>
        /// Called by the GazeInteractor when the gaze exits this Interactable.
        /// Invokes the Exit events.
        /// </summary>
        /// <param name="interactor"></param>
        public void GazeExit(GazeInteractor interactor)
        {
            if(gameObject.activeInHierarchy) /// 用于检查游戏对象是否处于激活状态，检查游戏对象的所有父级是否处于激活状态。
            {
                StartCoroutine(WAIT_TO_EXIT_COROUTINE, interactor); /// 启动一个名为 WAIT_TO_EXIT_COROUTINE 的协程来延迟执行离开操作。这个协程通常用于在离开操作之前等待一段时间，以提供一定的延迟效果。
            }
            else
            {
                InvokeExit(interactor); /// 执行离开操作
            }
        }

        private IEnumerator WaitToExit_Coroutine(GazeInteractor interactor)
        {
            

            yield return new WaitForSeconds(_exitDelay); /// 使用 yield return 语句来暂停协程的执行，并在指定的时间 _exitDelay 后继续执行。
            /// WaitForSeconds 是 Unity 引擎提供的内置类。它用于在协程中暂停一段时间，以实现时间延迟的效果

            InvokeExit(interactor);
        }

        private void InvokeExit(GazeInteractor interactor)
        {
            /// 执行离开操作

            Exit?.Invoke(this, interactor);

            OnGazeExit?.Invoke();
            OnGazeToggle?.Invoke(false);

            IsActivated = false;
        }
    }
}