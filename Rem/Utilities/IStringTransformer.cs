namespace Rem.Utilities
{
    public interface IStringTransformer
    {
        bool TryTransform(string input, out object output);
    }
}