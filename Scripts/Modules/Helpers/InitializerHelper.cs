using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrannPack.Helpers.Initializers
{
    class InitializerHelper
    {

    }

    public interface IIndexable
    {
        public int Index { get; } 
        public abstract string CodeName { get; }
    }

    public class Registry<T> where T : IIndexable
    {
        private readonly Dictionary<int, T> _byIndex = new();
        private readonly Dictionary<string, T> _byCodeName = new();
        private readonly T _errorObject=default(T);

        public Registry()
        {
            _byIndex = new Dictionary<int, T>();
            _byCodeName = new Dictionary<string, T>();
        }

        public Registry(T ErrorObject):base()
        {
            _errorObject = ErrorObject;
        }

        public void Register(T obj)
        {
            _byIndex[obj.Index] = obj;
            _byCodeName[obj.CodeName] = obj;
        }


        public T Get(int index)
        {
            return _byIndex.TryGetValue(index, out var result) ? result : default;
        }

        public T Get(string codeName)
        {
            return _byCodeName.TryGetValue(codeName, out var result) ? result : default;
        }

        public IEnumerable<T> All => _byIndex.Values;
    }
}
