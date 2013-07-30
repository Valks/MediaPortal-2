#region Copyright (C) 2007-2013 Team MediaPortal

/*
    Copyright (C) 2007-2013 Team MediaPortal
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
using DynamicMedia.Data.Base;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using Okra.Data;

namespace DynamicMedia.Views.Base
{
  /// <summary>
  /// Holds the building instructions for creating a collection of media items and sub views.
  /// </summary>
  /// <remarks>
  /// <para>
  /// A view specification is an abstract construct which will be implemented in subclasses in a more special way.
  /// It specifies a data source of media items, for example by a database query, or by a hard disc location.
  /// A view specification can be instantiated to a concrete view, which then will reference to its view specification.
  /// This view specification itself doesn't hold any references to its created views.
  /// The view's contents may be ordered or not.<br/>
  /// </para>
  /// <para>
  /// Views are built on demand from a <see cref="ViewSpecification"/> which comes from a media module. Some media
  /// modules might persist their configured <see cref="ViewSpecification"/> structures by their own.
  /// </para>
  /// </remarks>
  public abstract class ViewSpecification
  {
    protected string _viewDisplayName;
    protected ICollection<Guid> _necessaryMIATypeIds;
    protected ICollection<Guid> _optionalMIATypeIds;

    protected ViewSpecification(string viewDisplayName,
                                IEnumerable<Guid> necessaryMIATypeIds, IEnumerable<Guid> optionalMIATypeIds)
    {
      _viewDisplayName = viewDisplayName;
      NecessaryMIATypeIds = necessaryMIATypeIds == null ? new HashSet<Guid>() : new HashSet<Guid>(necessaryMIATypeIds);
      OptionalMIATypeIds = optionalMIATypeIds == null ? new HashSet<Guid>() : new HashSet<Guid>(optionalMIATypeIds);
      if(!NecessaryMIATypeIds.Contains(ProviderResourceAspect.ASPECT_ID))
        NecessaryMIATypeIds.Add(ProviderResourceAspect.ASPECT_ID);
    }

    /// <summary>
    /// Returns the IDs of media item aspects which need to be present in all media items contained in this view.
    /// </summary>
    public ICollection<Guid> NecessaryMIATypeIds
    {
      get { return _necessaryMIATypeIds; }
      protected set { _necessaryMIATypeIds = value; }
    }

    /// <summary>
    /// Returns the IDs of media item aspects which may be present in all media items contained in this view.
    /// </summary>
    public ICollection<Guid> OptionalMIATypeIds
    {
      get { return _optionalMIATypeIds; }
      protected set { _optionalMIATypeIds = value; }
    }

    /// <summary>
    /// Returns the display name of the created view.
    /// </summary>
    public virtual string ViewDisplayName
    {
      get { return _viewDisplayName; }
      protected set { _viewDisplayName = value; }
    }

    /// <summary>
    /// Returns virtualized data list of all media items in this view and all sub views. Can be overridden to provide a more efficient implementation.
    /// </summary>
    /// <returns>Virtualized Data List Source of all media items of this and all sub views.</returns>
    [Obsolete("Use method GetMediaItemsDataSource.")]
    public virtual VirtualizingDataList<MediaItem> GetAllMediaItems()
    {
      return new VirtualizingDataList<MediaItem>(new MediaItemsDataProvider(this));
    }

    /// <summary>
    /// Returns virtualized data list of all media items in this view and all sub views. Can be overridden to provide a more efficient implementation.
    /// </summary>
    /// <returns>Virtualized Data List Source of all media items of this and all sub views.</returns>
    public abstract IDataListSource<MediaItem> GetMediaItemsDataSource();

    /// <summary>
    /// Returns virtualized data list of all View Specifications in this view and all sub views. Can be overridden to provide a more efficient implementation.
    /// </summary>
    /// <returns>Virtualized Data List Source of all View Specifications of this and all sub views.</returns>
    public abstract IDataListSource<ViewSpecification> GetSubViewSpecificationsDataSource();
  }
}
