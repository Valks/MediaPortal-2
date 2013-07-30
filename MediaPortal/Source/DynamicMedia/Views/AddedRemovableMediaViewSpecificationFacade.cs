using DynamicMedia.Data.Base;
using DynamicMedia.Views.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Common.MediaManagement;
using Okra.Data;

namespace DynamicMedia.Views
{
  public class AddedRemovableMediaViewSpecificationFacade : ViewSpecification
  {
    protected IDataListSource<MediaItem> _mediaItems;
    protected IDataListSource<ViewSpecification> _viewSpecifications;
    protected ViewSpecification _viewSpecification;

    public AddedRemovableMediaViewSpecificationFacade(ViewSpecification viewSpecification)
      : this(viewSpecification, viewSpecification.ViewDisplayName, viewSpecification.NecessaryMIATypeIds, viewSpecification.OptionalMIATypeIds)
    {
      
    }

    public AddedRemovableMediaViewSpecificationFacade(ViewSpecification viewSpecification, string viewDisplayName, IEnumerable<Guid> necessaryMIATypeIds, IEnumerable<Guid> optionalMIATypeIds)
      : base(viewDisplayName, necessaryMIATypeIds, optionalMIATypeIds)
    {
      _viewSpecification = viewSpecification;
      IList<RemovableDriveViewSpecification> specifications =
        RemovableDriveViewSpecification.CreateViewSpecificationsForRemovableDrives(necessaryMIATypeIds,
                                                                                   optionalMIATypeIds);
      List<IDataListSource<MediaItem>> mediaDataSources = new List<IDataListSource<MediaItem>>();
      mediaDataSources.Add(_viewSpecification.GetMediaItemsDataSource());
      mediaDataSources.AddRange(specifications.Select(specification => specification.GetMediaItemsDataSource()).ToList());

      _mediaItems = new StackedItemsDataProvider<MediaItem>(mediaDataSources);

      List<IDataListSource<ViewSpecification>> specificationDataSources = new List<IDataListSource<ViewSpecification>>();
      specificationDataSources.Add(_viewSpecification.GetSubViewSpecificationsDataSource());
      specificationDataSources.AddRange(specifications.Select(specification => specification.GetSubViewSpecificationsDataSource()));

      _viewSpecifications = new StackedItemsDataProvider<ViewSpecification>(specificationDataSources);
    }

    public override IDataListSource<MediaItem> GetMediaItemsDataSource()
    {
      return _mediaItems;
    }

    public override IDataListSource<ViewSpecification> GetSubViewSpecificationsDataSource()
    {
      return _viewSpecifications;
    }
  }
}
