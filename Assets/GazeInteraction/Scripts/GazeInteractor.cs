#region Includes
using UnityEngine;
#endregion

namespace TS.GazeInteraction
{
    /// <summary>
    /// Component responsible for managing the gaze interaction.
    /// </summary>
    public class GazeInteractor : MonoBehaviour
    {
        #region Variables

        [Header("Configuration")] /// [Header("Configuration")] 配置，用于在 Inspector 面板中给字段或属性添加一个标题头。它的作用是对字段或属性进行分组，提供更好的可读性和可视化。
        [SerializeField] private float _maxDetectionDistance;
        [SerializeField] private float _minDetectionDistance;
        [SerializeField] private float _timeToActivate = 1.0f;
        [SerializeField] private LayerMask _layerMask; /// Unity 引擎中的一个数据类型，用于表示层级关系和标记物体所属的图层。
        /// [SerializeField] 用于将私有字段或私有属性暴露给 Unity 的序列化系统，使它们可以在 Inspector 面板中显示和编辑。通常，私有字段或私有属性无法在 Inspector 面板中直接访问，但是使用 [SerializeField] 特性可以解决这个问题。
        /// 在给定的代码中，字段名前面的下划线 _ 是一种常见的命名约定，用于表示私有字段。这是一种命名惯例，帮助区分私有字段和公共属性或方法。

        private Ray _ray;
        private RaycastHit _hit;

        private GazeReticle _reticle;
        private GazeInteractable _interactable;

        private float _enterStartTime;

        #endregion

        private void Start()
        {
            /// void 是一种返回类型（return type）的标识符，表示一个方法不返回任何值。当方法被标记为 void 时，它表示该方法执行一些操作或功能，但不会返回任何结果。

            /// <summary>
            /// 在 Start() 方法中，通过 ResourcesManager.GetPrefab 方法加载 gaze_reticle 的预制体，并获取其中的 GazeReticle 组件。然后实例化该预制体，并将 GazeReticle 组件赋值给 _reticle 变量。
            /// </summary>

            var instance = ResourcesManager.GetPrefab(ResourcesManager.FILE_PREFAB_RETICLE);
            var reticle = instance.GetComponent<GazeReticle>(); /// 获取到 注视点光标对象的 GazeReticle（光标组件）

            #if UNITY_EDITOR || DEVELOPMENT_BUILD
                        if(reticle == null) { throw new System.Exception("Missing GazeReticle"); }
            #endif
            /// 在给定的代码中，#if 和 #endif 是条件编译指令（Conditional Compilation Directives），它们用于在编译时根据指定的条件选择性地包含或排除一段代码。

            _reticle = Instantiate(reticle); /// Instantiate 方法用于创建一个对象的实例，它接受一个参数作为要实例化的对象，并返回该对象的克隆。
            /// 通过实例化，可以确保每个 GazeReticle 对象都是独立的，并且可以在场景中进行独立的交互操作。
            _reticle.SetInteractor(this); /// SetInteractor 方法是 GazeReticle 组件的一个方法，用于设置与之关联的交互器（interactor）。在这里，将当前的 GazeInteractor 对象作为交互器传递给 _reticle，以建立它们之间的联系。
        }
        private void Update()
        {

            /// <summary>
            /// 在 Update() 方法中，通过发射射线来检测是否有目标物体与射线相交。如果相交，则进行一系列交互操作，包括处理目标物体的进入、停留和激活。
            /// </summary>
            
            _ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(_ray, out _hit, _maxDetectionDistance, _layerMask))
            {
                var distance = Vector3.Distance(transform.position, _hit.transform.position);
                /// 计算游戏对象与相交物体之间的距离
                if (distance < _minDetectionDistance)
                {
                    _reticle.Enable(false);
                    Reset();
                    return;
                }

                _reticle.SetTarget(_hit); /// 设置 GazeReticle 的目标为相交的物体（通过 _reticle.SetTarget(_hit)），即把 光标放到交互对象上
                _reticle.Enable(true); /// _reticle.Enable(true) 是调用 GazeReticle 组件中的 Enable 方法
                /// TODO： GazeReticle 组件中的 Enable 方法 是做什么的

                var interactable = _hit.collider.transform.GetComponent<GazeInteractable>();
                /// var 关键字用于隐式类型推断，它允许编译器根据赋值的表达式推断变量的类型。
                /// 从Hit 获取到 碰撞的交互对象
                if(interactable == null)
                {
                    Reset(); /// Reset() 是一个自定义的方法调用，它用于重置一些状态或执行一些清理操作
                    return; /// return 语句用于提前结束当前方法的执行，并将控制权返回到调用方。
                }

                /// 如果当前相交的物体与之前相交的物体不同，说明进入了一个新的交互目标。
                if (interactable != _interactable)
                {
                    Reset();

                    _enterStartTime = Time.time; /// 记录当前时间作为进入时间
                    /// DONE：为什么要获取到 这个时间？这个时间是私有的呀，因为在 _reticle.SetProgress(progress); 这里 向_reticle 传递了进度（progress）

                    _interactable = interactable; /// 相交物体赋值给 _interactable 变量，更新 
                    _interactable.GazeEnter(this, _hit.point); /// 表示游戏对象进入了该物体的视线。
                }

                _interactable.GazeStay(this, _hit.point); /// 游戏对象持续注视该物体。

                /// 可激活的且尚未激活
                if (_interactable.IsActivable && !_interactable.IsActivated) 
                {
                    /// 计算激活进度和时间
                    var timeToActivate = (_enterStartTime + _timeToActivate) - Time.time; /// 是在当前作用域内创建了一个名为 timeToActivate 的新变量
                    /// 作用域限定在 Update() 方法内部
                    var progress = 1 - (timeToActivate / _timeToActivate); /// 通过百分比来计算 进度
                    progress = Mathf.Clamp(progress, 0, 1);  /// 将 progress 的值限制在 0 到 1 之间。如果 progress 小于 0，它将被设置为 0；如果 progress 大于 1，它将被设置为 1。这样可以确保 progress 始终在指定的范围内。

                    _reticle.SetProgress(progress); /// 计算激活进度和时间，并更新 GazeReticle 的进度显示

                    if (progress == 1)
                    {
                        _reticle.Enable(false); /// 禁用 GazeReticle
                        _interactable.Activate(); /// 这里是触发 交互对象 的 激活状态
                    }
                }

                return;
            }

            _reticle.Enable(false);
            Reset();
        }

        private void Reset()
        {

            /// <summary>
            /// Reset() 方法用于重置状态，将 _reticle 的进度设置为0，并调用目标物体的 GazeExit 方法。
            /// </summary>

            _reticle.SetProgress(0);

            if(_interactable == null) { return; } /// 如果是 null，说明当前没有与视线交互的目标物体，就直接返回（不执行后续的代码）。 优化性能和避免不必要的操作
            _interactable.GazeExit(this); /// 用于处理当目标物体离开视线时的逻辑。可以实现一些清理操作、状态重置或其他自定义逻辑，以响应目标物体离开交互的事件。
            _interactable = null; /// 当前没有与视线交互的目标物体。
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            /// <summary>
            /// OnDrawGizmosSelected() 方法是在编辑器中绘制调试用的图形，用黄色射线表示射线检测的最大距离。
            /// </summary>

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.forward * _maxDetectionDistance);
        }
#endif
    }
}