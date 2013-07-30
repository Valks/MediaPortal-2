using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Okra.Data;

namespace DynamicMedia.Data.Base
{
  public class EmptyDataProvider<T> : IDataListSource<T>
  {
    public Task<int> GetCountAsync()
    {
      return new Task<int>(() => 0);
    }

    public Task<T> GetItemAsync(int index)
    {
      return new Task<T>(() => (T) new object());
    }

    public int IndexOf(T item)
    {
      return 0;
    }

    public IDisposable Subscribe(IUpdatableCollection collection)
    {
      return null;
    }
  }
}
