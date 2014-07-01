#region Copyright (C) 2007-2014 Team MediaPortal

/*
    Copyright (C) 2007-2014 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.ResourceAccess;
using Okra.Data;

namespace MediaPortal.UiComponents.Media.Views
{
  /// <summary>
  /// View implementation which is based on a local provider path.
  /// </summary>
  public class LocalDirectoryViewSpecification : ViewSpecification
  {
    #region Consts

    public const string INVALID_SHARE_NAME_RESOURCE = "[Media.InvalidShareName]";

    #endregion

    #region Protected fields

    protected string _overrideName;
    protected ResourcePath _viewPath;

    #endregion

    #region Ctor

    /// <summary>
    /// Creates a new <see cref="LocalDirectoryViewSpecification"/> instance.
    /// </summary>
    /// <param name="overrideName">Overridden name for the view. If not set, the resource name of the specified
    /// <paramref name="viewPath"/> will be used as <see cref="ViewDisplayName"/>.</param>
    /// <param name="viewPath">Path of a directory in a local filesystem provider.</param>
    /// <param name="necessaryMIATypeIds">Ids of the media item aspect types which should be extracted for all items and
    /// sub views of this view.</param>
    /// <param name="optionalMIATypeIds">Ids of the media item aspect types which may be extracted for items and
    /// sub views of this view.</param>
    public LocalDirectoryViewSpecification(string overrideName, ResourcePath viewPath,
        IEnumerable<Guid> necessaryMIATypeIds, IEnumerable<Guid> optionalMIATypeIds) :
        base(null, necessaryMIATypeIds, optionalMIATypeIds)
    {
      _overrideName = overrideName;
      _viewPath = viewPath;
      UpdateDisplayName();
    }

    #endregion

    /// <summary>
    /// Returns the resource path of the directory of this view.
    /// </summary>
    public ResourcePath ViewPath
    {
      get { return _viewPath; }
    }

    /// <summary>
    /// Returns the display name which overrides the default (created) display name. This can be
    /// useful for shares root directories.
    /// </summary>
    public string OverrideName
    {
      get { return _overrideName; }
      set
      {
        _overrideName = value;
        UpdateDisplayName();
      }
    }

    #region Base overrides

    public override string ViewDisplayName
    {
      get
      {
        if (string.IsNullOrEmpty(_viewDisplayName))
          UpdateDisplayName();
        return _viewDisplayName;
      }
    }

    public override bool CanBeBuilt
    {
      get { return _viewPath.IsValidLocalPath; }
    }

    public override IList<MediaItem> GetAllMediaItems()
    {
      return GetItemsRecursive(BuildView());
    }

    protected IList<MediaItem> GetItemsRecursive(View view)
    {
      return new IncrementalLoadingDataList<MediaItem>(new SimplePagedDataListSource<MediaItem>((pageNumber, pageSize) =>
      {
        var result = new List<MediaItem>();
        int startPos = pageNumber * pageSize;
        result.AddRange(GetItemsRecursive(0, startPos, startPos + pageSize, view));

        return new DataListPageResult<MediaItem>(result.Count, pageSize, pageNumber, result);
      }));
    }

    protected IList<MediaItem> GetItemsRecursive(int position, int startPosition, int endPosition, View currentView)
    {
      List<MediaItem> items = new List<MediaItem>();

      int activeObjectItemCount = currentView.MediaItems.Count;

      if (items.Count + activeObjectItemCount >= startPosition)
      {
        for (int i = position; i < ((position + activeObjectItemCount) <= endPosition ? (position + activeObjectItemCount) : endPosition); i++)
        {
          items.Add(currentView.MediaItems[i]);
        }
      }

      if (items.Count >= endPosition)
        return items;

      foreach (View subView in currentView.SubViews)
      {
        items.AddRange(GetItemsRecursive(items.Count, startPosition, endPosition, subView));
        if (items.Count >= endPosition)
          break;
      }

      return items;
    }

    protected internal override void ReLoadItemsAndSubViewSpecifications(out IList<MediaItem> mediaItems, out IList<ViewSpecification> subViewSpecifications)
    {
      mediaItems = new List<MediaItem>();
      subViewSpecifications = new List<ViewSpecification>();
      IMediaAccessor mediaAccessor = ServiceRegistration.Get<IMediaAccessor>();
      IEnumerable<Guid> metadataExtractorIds = mediaAccessor.GetMetadataExtractorsForMIATypes(_necessaryMIATypeIds.Union(_optionalMIATypeIds));
      IResourceAccessor baseResourceAccessor;
      if (!_viewPath.TryCreateLocalResourceAccessor(out baseResourceAccessor))
      {
        ServiceRegistration.Get<ILogger>().Warn("LocalDirectoryViewSpecification.ReLoadItemsAndSubViewSpecifications: Cannot access local view path '{0}'", _viewPath);
        return;
      }
      using (baseResourceAccessor)
      {
        IFileSystemResourceAccessor fsra = baseResourceAccessor as IFileSystemResourceAccessor;
        if (fsra == null)
        {
          ServiceRegistration.Get<ILogger>().Warn("LocalDirectoryViewSpecification.ReLoadItemsAndSubViewSpecifications: Cannot access local view path '{0}' - no local file system resource", _viewPath);
          return;
        }
        // Add all items at the specified path
        ICollection<IFileSystemResourceAccessor> files = FileSystemResourceNavigator.GetFiles(fsra, false);
        if (files != null)
          foreach (IFileSystemResourceAccessor childAccessor in files)
            using (childAccessor)
              try
              {
                MediaItem result = mediaAccessor.CreateLocalMediaItem(childAccessor, metadataExtractorIds);
                if (result != null)
                  mediaItems.Add(result);
              }
              catch (Exception e)
              {
                ServiceRegistration.Get<ILogger>().Warn("LocalDirectoryViewSpecification: Error creating local media item for '{0}'", e, childAccessor);
              }
        ICollection<IFileSystemResourceAccessor> directories = FileSystemResourceNavigator.GetChildDirectories(fsra, false);
        if (directories != null)
          foreach (IFileSystemResourceAccessor childAccessor in directories)
            using (childAccessor)
              try
              {
                MediaItem result = mediaAccessor.CreateLocalMediaItem(childAccessor, metadataExtractorIds);
                if (result == null)
                  subViewSpecifications.Add(new LocalDirectoryViewSpecification(null, childAccessor.CanonicalLocalResourcePath,
                    _necessaryMIATypeIds, _optionalMIATypeIds));
                else
                  mediaItems.Add(result);
              }
              catch (Exception e)
              {
                ServiceRegistration.Get<ILogger>().Warn("LocalDirectoryViewSpecification: Error creating media item or view specification for '{0}'", e, childAccessor);
              }
      }
    }

    #endregion

    protected void UpdateDisplayName()
    {
      if (string.IsNullOrEmpty(_overrideName))
      {
        IResourceAccessor ra;
        if (!_viewPath.TryCreateLocalResourceAccessor(out ra))
          return;
        using (ra)
          _viewDisplayName = ra.ResourceName;
      }
      else
        _viewDisplayName = _overrideName;
    }
  }
}
