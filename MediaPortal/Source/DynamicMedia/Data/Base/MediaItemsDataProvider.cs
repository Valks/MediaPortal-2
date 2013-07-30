using DynamicMedia.Views.Base;
using MediaPortal.Common.MediaManagement;
using Okra.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamicMedia.Data.Base
{
  public class MediaItemsDataProvider : PagedDataListSource<MediaItem>
  {
    protected readonly ViewSpecification _viewSpecification;

    protected int _pageSize = 50;

    public MediaItemsDataProvider(ViewSpecification viewSpecification)
    {
      if (viewSpecification == null)
        throw new ArgumentNullException("viewSpecification", "MediaItemsDataProvider viewSpecification cannot be null.");

      _viewSpecification = viewSpecification;
    }

    public MediaItemsDataProvider(ViewSpecification viewSpecification, int pageSize)
    {
      if (viewSpecification == null)
        throw new ArgumentNullException("viewSpecification", "MediaItemsDataProvider viewSpecification cannot be null.");

      _viewSpecification = viewSpecification;

      if (pageSize <= 0)
        throw new ArgumentOutOfRangeException("pageSize", "MediaItemsDataProvider pageSize must be greater than zero.");

      _pageSize = pageSize;
    }

    protected override Task<DataListPageResult<MediaItem>> FetchCountAsync()
    {
      return new Task<DataListPageResult<MediaItem>>(() => new DataListPageResult<MediaItem>(
                                                             _viewSpecification.GetMediaItemsDataSource().GetCountAsync().Result,
                                                             null,
                                                             null,
                                                             null));
    }

    protected override Task<DataListPageResult<MediaItem>> FetchPageAsync(int pageNumber)
    {
      return new Task<DataListPageResult<MediaItem>>(() =>
        {
          IDataListSource<MediaItem> itemsSource = _viewSpecification.GetMediaItemsDataSource();
          int? totalItemCount = FetchCountAsync().Result.TotalItemCount;
          if (totalItemCount == null)
          {
            return new DataListPageResult<MediaItem>(0, 0, 0, null);
          }

          IList<MediaItem> items = new List<MediaItem>();

          for (int i = _pageSize * (pageNumber - 1); i < _pageSize * _pageSize && i < totalItemCount; i++)
          {
            items.Add(itemsSource.GetItemAsync(i).Result);
          }

          return new DataListPageResult<MediaItem>(items.Count, _pageSize, pageNumber, items);
        });
    }

    protected override Task<DataListPageResult<MediaItem>> FetchPageSizeAsync()
    {
      return new Task<DataListPageResult<MediaItem>>(() => new DataListPageResult<MediaItem>(null,
                                                                                             _pageSize,
                                                                                             null,
                                                                                             null));
    }
  }
}