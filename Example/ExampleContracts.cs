using System;
using UnityEngine;

namespace RootPattern.Example
{
    public interface IExampleLog
    {
        void Write(string message);
    }

    [Serializable]
    public struct ExampleRootContext : IRootContext
    {
        [SerializeField] private ExampleView _view;
        [SerializeField] private string _rootName;
        [NonSerialized] private IExampleLog _log;

        public ExampleRootContext(ExampleView view, string rootName, IExampleLog log)
        {
            _view = view;
            _rootName = rootName;
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public ExampleView View => _view;
        public string RootName => _rootName;
        public IExampleLog Log => _log ?? throw new InvalidOperationException("A runtime log must be supplied before creating the root.");

        public ExampleRootContext WithLog(IExampleLog log)
        {
            return new ExampleRootContext(_view, _rootName, log);
        }
    }
}
