using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Okra.Data;

namespace MediaPortal.Common.Services.ServerCommunication
{
  public class UPnPContentDirectoryPagedDataSource<T> : PagedDataListSource<T>
  {
    private const int PAGE_SIZE = 20;
    private readonly Func<int, int, UPnPContentDirectoryRequestResult<T>> _requestFunc;
    private readonly Func<int> _countFunc; 

    private uint _updateId = 0;

    public UPnPContentDirectoryPagedDataSource(Func<int, int, UPnPContentDirectoryRequestResult<T>> requestFunc, Func<int> countFunc)
    {
      _requestFunc = requestFunc;
      if (countFunc == null)
      {
        countFunc = () => requestFunc(0, 1).Count;
      }

      _countFunc = countFunc;
    }

    protected override Task<DataListPageResult<T>> FetchCountAsync()
    {
      return new Task<DataListPageResult<T>>(() => new DataListPageResult<T>(_countFunc(), null, null, null));
    }

    protected override Task<DataListPageResult<T>> FetchPageAsync(int pageNumber)
    {
      return new Task<DataListPageResult<T>>(() =>
      {
        UPnPContentDirectoryRequestResult<T> result = _requestFunc(pageNumber, PAGE_SIZE);
        // Check if backend changed, if so refresh local cache.
        if (_updateId != result.UpdateId)
        {
          _updateId = result.UpdateId;
          Refresh();
        }

        return new DataListPageResult<T>(result.Count, PAGE_SIZE, pageNumber, result.Items);
      });
    }

    protected override Task<DataListPageResult<T>> FetchPageSizeAsync()
    {
      return new Task<DataListPageResult<T>>(() => new DataListPageResult<T>(null,PAGE_SIZE,null,null));
    }
  }

  public class UPnPContentDirectoryRequestResult<T>
  {
    public UPnPContentDirectoryRequestResult(IList<T> items, int count, int returnCount, uint updateId)
    {
      Items = items;
      Count = count;
      ReturnCount = returnCount;
      UpdateId = updateId;
    }

    public IList<T> Items { get; set; }
    public int Count { get; set; }
    public int ReturnCount { get; set; }
    public uint UpdateId { get; set; }
  }
}