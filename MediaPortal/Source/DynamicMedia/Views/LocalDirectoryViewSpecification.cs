using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicMedia.Views.Base;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.ResourceAccess;
using Okra.Data;

namespace DynamicMedia.Views
{
  public class LocalDirectoryViewSpecification : ViewSpecification
  {
    #region Consts

    public const string INVALID_SHARE_NAME_RESOURCE = "[Media.InvalidShareName]";

    #endregion

    #region Protected fields

    protected string _overrideName;
    protected ResourcePath _viewPath;

    #endregion

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
        IEnumerable<Guid> necessaryMIATypeIds, IEnumerable<Guid> optionalMIATypeIds)
      : base(null, necessaryMIATypeIds, optionalMIATypeIds)
    {
      _overrideName = overrideName;
      _viewPath = viewPath;
      UpdateDisplayName().Start();
    }

    public override IDataListSource<MediaItem> GetMediaItemsDataSource()
    {
      throw new NotImplementedException();
    }

    public override IDataListSource<ViewSpecification> GetSubViewSpecificationsDataSource()
    {
      throw new NotImplementedException();
    }

    protected Task UpdateDisplayName()
    {
      return new Task(() =>
      {
        if (string.IsNullOrEmpty(_overrideName))
        {
          IResourceAccessor resourceAccessor;
          if (!_viewPath.TryCreateLocalResourceAccessor(out resourceAccessor))
            return;

          using (resourceAccessor)
            ViewDisplayName = resourceAccessor.ResourceName;
        }
        else
        {
          ViewDisplayName = _overrideName;
        }
      });
    }
  }
}
