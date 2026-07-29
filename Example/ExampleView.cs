using UnityEngine;

namespace RootPattern.Example
{
    /// <summary>
    /// A scene or prefab dependency registered by <see cref="ExampleRootBehaviour"/>.
    /// </summary>
    public sealed class ExampleView : MonoBehaviour, IExampleView
    {
        [SerializeField] private string _lastMessage;

        public void Show(string message)
        {
            _lastMessage = message;
            Debug.Log(message, this);
        }
    }
}
