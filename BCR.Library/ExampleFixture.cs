using Docnet.Core;

namespace BCR.Library
{
    public class ExampleFixture : IDisposable
    {
        public IDocLib DocNet { get; }

        public ExampleFixture()
        {
            DocNet = DocLib.Instance;
        }

        public void Dispose()
        {
            DocNet.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}