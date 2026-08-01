using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using RootTask = Cysharp.Threading.Tasks.UniTask;

namespace RootPattern.Tests
{
    public sealed class RootTests
    {
        [Test]
        public async Task InitializeAsync_CallsHookWithProvidedToken()
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            using var root = new TestRoot();

            await root.InitializeAsync(cancellationTokenSource.Token);

            Assert.IsTrue(root.InitializeAsyncCalled);
            Assert.AreEqual(cancellationTokenSource.Token, root.InitializeToken);
            Assert.AreEqual(RootState.Initialized, root.State);
        }

        [Test]
        public async Task InitializeAsync_WhenHookFails_MarksFailureAndCancelsRootToken()
        {
            using var root = new FailingRoot();
            var rootToken = root.CancellationToken;

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await root.InitializeAsync(CancellationToken.None));

            Assert.AreEqual(RootState.InitializationFailed, root.State);
            Assert.IsTrue(rootToken.IsCancellationRequested);
        }

        [Test]
        public async Task InitializeAsync_WhenCalledMoreThanOnce_Throws()
        {
            using var root = new TestRoot();
            await root.InitializeAsync(CancellationToken.None);

            Assert.ThrowsAsync<InvalidOperationException>(
                async () => await root.InitializeAsync(CancellationToken.None));
        }

        private sealed class TestRoot : Root
        {
            public bool InitializeAsyncCalled { get; private set; }
            public CancellationToken InitializeToken { get; private set; }

            protected override RootTask OnInitializeAsync(CancellationToken token)
            {
                InitializeAsyncCalled = true;
                InitializeToken = token;
                return default;
            }
        }

        private sealed class FailingRoot : Root
        {
            protected override RootTask OnInitializeAsync(CancellationToken token)
            {
                throw new InvalidOperationException("Initialization failed.");
            }
        }
    }
}
