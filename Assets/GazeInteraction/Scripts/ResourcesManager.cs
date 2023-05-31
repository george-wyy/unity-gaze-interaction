#region Includes 
using UnityEngine;
#endregion 
/// #region 是 C# 的预处理指令之一，用于逻辑组织和代码分块。

namespace TS.GazeInteraction
{
    /// <summary>
    /// Loads assets from the Resources directory.
    /// </summary>
    /// TS.GazeInteraction 命名空间提供了一种避免命名冲突的机制，使得不同的代码可以使用相同的类名而不会产生冲突。

    public class ResourcesManager : MonoBehaviour
    {
        #region Variables
        /// 在这段代码中，#region Variables 是一个代码区域标记（region），用于将一段相关的代码逻辑进行分组。
        private const string DIRECTORY_PREFABS_FORMAT = "Prefabs/{0}"; /// 一个私有的常量字符串变量，用于格式化资源目录的路径。
        /// 常量（const）是在编程中用来表示固定不变的值的标识符。它们是一种特殊类型的变量，在声明时被赋予一个固定的值，并且在整个程序的执行过程中都不允许修改。
        public const string FILE_PREFAB_RETICLE = "gaze_reticle";

        #endregion

        /// <summary>
        /// Loads and returns the asset specified in file from the Resources directory.
        /// </summary>
        /// <param name="file">Name of the asset to load. Use one of the constants.</param>
        /// <returns>Returns the loaded GameObject.</returns>
        /// 下面是静态方法， 静态方法 是属于类而不是实例的，可以在不创建类的实例的情况下直接访问和调用。
        /// 在方法体内部，它使用 Resources.Load 方法来从资源目录中加载一个 GameObject 对象。Resources.Load 方法接受一个路径作为参数，用于指定要加载的资源的位置。在这个例子中，DIRECTORY_PREFABS_FORMAT 是一个字符串常量，代表了资源目录的格式，而 file 参数是具体的资源文件名。
        /// 通过使用 string.Format 方法，将 DIRECTORY_PREFABS_FORMAT 和 file 进行格式化，生成实际的资源路径。然后，这个路径作为参数传递给 Resources.Load 方法，并指定要加载的资源类型为 GameObject。最终，方法返回加载到的 GameObject 对象。
        public static GameObject GetPrefab(string file)
        {
            return Resources.Load<GameObject>(string.Format(DIRECTORY_PREFABS_FORMAT, file));
            /// Resources.Load 是 Unity 引擎提供的一个方法，用于从资源目录中加载资源。它是 UnityEngine 命名空间下的方法。
            /// string.Format 是 .NET Framework 提供的一个方法，用于格式化字符串。它是 System 命名空间下的方法。【自动导入】
        }
    }
}