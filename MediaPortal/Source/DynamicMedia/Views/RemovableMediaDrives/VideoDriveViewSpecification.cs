using System.IO;
using DynamicMedia.Data.Base;
using DynamicMedia.Data.RemovableMediaDrives;
using DynamicMedia.Views.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Okra.Data;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common;
using MediaPortal.Common.Logging;

namespace DynamicMedia.Views.RemovableMediaDrives
{
  public enum VideoMediaType
  {
    Unknown,
    VideoBD,
    VideoDVD,
    VideoCD
  }

  public class VideoDriveViewSpecification : ViewSpecification
  {
    protected IDataListSource<MediaItem> _mediaItems;

    private VideoDriveViewSpecification(string viewDisplayName, IEnumerable<Guid> necessaryMIATypeIds,
                                       IEnumerable<Guid> optionalMIATypeIds, IDataListSource<MediaItem> mediaItems)
      : base(viewDisplayName, necessaryMIATypeIds, optionalMIATypeIds)
    {
      if (mediaItems == null)
        throw new ArgumentNullException("mediaItems", "mediaItems cannot be null.");
      
      _mediaItems = mediaItems;
    }

    public override IDataListSource<MediaItem> GetMediaItemsDataSource()
    {
      return _mediaItems;
    }

    public override IDataListSource<ViewSpecification> GetSubViewSpecificationsDataSource()
    {
      return new EmptyDataProvider<ViewSpecification>();
    }

    public static ViewSpecification TryCreateVideoDriveViewSpecification(DriveInfo driveInfo, Guid[] necessaryVideoMias)
    {
      string viewDisplayName;

      IDataListSource<MediaItem> mediaItems = VideoDriveMediaItemsProvider.TryCreateVideoDriveMediaItemsProvider(driveInfo, necessaryVideoMias);
      if (mediaItems == null)
        return null;

      try
      {
        viewDisplayName = driveInfo.VolumeLabel;
      }
      catch (Exception)
      {
        viewDisplayName = "Video CD";
      }

      try
      {
        return new VideoDriveViewSpecification(viewDisplayName, necessaryVideoMias, null, mediaItems);
      }
      catch (Exception ex)
      {
        ServiceRegistration.Get<ILogger>().Error("RemovableMediaManager: Error accessing video disc {0}", ex, driveInfo.Name);
      }

      return null;
    }
  }
}
