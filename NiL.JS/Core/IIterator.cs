namespace NiL.JS.Core
{
    public interface IIterator
    {
#pragma warning disable IDE1006

        IIteratorResult next(Arguments arguments = null);
        IIteratorResult @return();
        IIteratorResult @throw(Arguments arguments = null);

#pragma warning restore IDE1006
    }
}
