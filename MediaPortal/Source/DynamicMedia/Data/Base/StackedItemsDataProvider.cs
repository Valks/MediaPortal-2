using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaPortal.Common.MediaManagement;
using Okra.Data;

namespace DynamicMedia.Data.Base
{
  public class StackedItemsDataProvider<T> : PagedDataListSource<T>
  {
    protected IList<IDataListSource<T>> _dataListSources;

    protected int _pageSize = 50;

    public StackedItemsDataProvider(IList<IDataListSource<T>> dataListSources)
    {
      if (dataListSources == null)
        throw new ArgumentNullException("dataListSources", "StackedMediaItemsDataProvider dataListSources cannot be null.");
        
      _dataListSources = dataListSources;
    }

    protected override Task<DataListPageResult<T>> FetchCountAsync()
    {
      return new Task<DataListPageResult<T>>(() =>
        {
          int count = _dataListSources.Aggregate(0, (current, dataListSource) => current + dataListSource.GetCountAsync().Result);

          return new DataListPageResult<T>(count,null,null,null);
        });
    }

    protected override Task<DataListPageResult<T>> FetchPageAsync(int pageNumber)
    {
      return new Task<DataListPageResult<T>>(() =>
        {
          if (FetchCountAsync().Result.TotalItemCount == 0)
            return new DataListPageResult<T>(0, 0, 0, null);

          IList<T> items = new List<T>();

          foreach (IDataListSource<T> dataListSource in _dataListSources)
          {
            int count = dataListSource.GetCountAsync().Result;

            if (count > pageNumber*_pageSize - _pageSize)
            {
              for (int i = (pageNumber * _pageSize - _pageSize); i < count - (pageNumber * _pageSize - _pageSize); i++)
              {
                items.Add(dataListSource.GetItemAsync(i).Result);
              }
            }
          }

          return new DataListPageResult<T>(items.Count, _pageSize, pageNumber, items);
        });
    }

    protected override Task<DataListPageResult<T>> FetchPageSizeAsync()
    {
      return new Task<DataListPageResult<T>>(() => new DataListPageResult<T>(null, _pageSize, null, null));
    }

    public void Add(IDataListSource<T> items)
    {
      _dataListSources.Add(items);
    }
  }
}
