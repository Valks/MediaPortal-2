using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DynamicMedia.Views.Base;
using Okra.Data;

namespace DynamicMedia.Data.Base
{
  public class ViewDataProvider : PagedDataListSource<View>
  {
    protected readonly IList<ViewSpecification> _viewSpecification;
    protected int _pageSize = 50;

    public ViewDataProvider(IList<ViewSpecification> viewSpecifications)
    {
      if (viewSpecifications == null)
        throw new ArgumentNullException("viewSpecifications", "MediaItemsDataProvider viewSpecification cannot be null.");

      _viewSpecification = viewSpecifications;
    }

    public ViewDataProvider(IList<ViewSpecification> viewSpecifications, int pageSize)
    {
      if (viewSpecifications == null)
        throw new ArgumentNullException("viewSpecifications", "MediaItemsDataProvider viewSpecification cannot be null.");

      _viewSpecification = viewSpecifications;

      if (pageSize <= 0)
        throw new ArgumentOutOfRangeException("pageSize", "MediaItemsDataProvider pageSize must be greater than zero.");

      _pageSize = pageSize;
    }

    protected override Task<DataListPageResult<View>> FetchCountAsync()
    {
      return new Task<DataListPageResult<View>>(() => new DataListPageResult<View>(_viewSpecification.Count, null, null, null));
    }

    protected override Task<DataListPageResult<View>> FetchPageAsync(int pageNumber)
    {
      return new Task<DataListPageResult<View>>(() => new DataListPageResult<View>(_viewSpecification.Count,
                                                                                   _viewSpecification.Count < _pageSize * pageNumber ? _viewSpecification.Count : _pageSize,
                                                                                   pageNumber,
                                                                                   _viewSpecification
                                                                                     .Skip(_pageSize * (pageNumber - 1))
                                                                                     .Take(_pageSize)
                                                                                     .Select(
                                                                                       (view) => new View(view) {DisplayName = view.ViewDisplayName}).ToList()));
    }

    protected override Task<DataListPageResult<View>> FetchPageSizeAsync()
    {
      return new Task<DataListPageResult<View>>(() => new DataListPageResult<View>(_viewSpecification.Count,
                                                                                   _viewSpecification.Count < 50 ? _viewSpecification.Count : 50,
                                                                                   1,
                                                                                   null));
    }
  }
}
