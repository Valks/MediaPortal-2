using System;
using System.Collections.Generic;
using DynamicMedia.Data.Base;
using MediaPortal.Common.MediaManagement;
using Okra.Data;

namespace DynamicMedia.Views.Base
{
  public class View
  {
    private IList<MediaItem> _mediaItems; 
    private IList<View> _subViews; 

    internal View(ViewSpecification viewSpecification)
    {
      Specification = viewSpecification;
      DisplayName = viewSpecification.ViewDisplayName;
    }

    public string DisplayName { get; internal set; }

    public bool IsEmpty { get { return MediaItems.Count == 0 && SubViews.Count == 0; } }

    public ViewSpecification Specification { get; private set; }

    public ICollection<Guid> NecessaryMIATypeIds { get { return Specification.NecessaryMIATypeIds; } }

    public ICollection<Guid> OptionalMIATypeIDs { get { return Specification.OptionalMIATypeIds; } }

    public IList<MediaItem> MediaItems
    {
      get
      {
        if (!IsLoaded)
          LoadItemsAndSubViews();
        return _mediaItems;
      }
    }

    public IList<View> SubViews
    {
      get
      {
        if (!IsLoaded)
          LoadItemsAndSubViews();
        return _subViews;
      }
    }

    public bool IsLoaded { get { return _mediaItems != null && _subViews != null; } }

    private void LoadItemsAndSubViews()
    {
      _mediaItems = new VirtualizingDataList<MediaItem>(Specification.GetMediaItemsDataSource());
      VirtualizingDataList<ViewSpecification> subViewSpecifications = new VirtualizingDataList<ViewSpecification>(Specification.GetSubViewSpecificationsDataSource());

      _subViews = new VirtualizingDataList<View>(new ViewDataProvider(subViewSpecifications));
    }

    private void InvalidateView()
    {
      _mediaItems = null;
      _subViews = null;
    }

    public override string ToString()
    {
      return DisplayName;
    }
  }
}
