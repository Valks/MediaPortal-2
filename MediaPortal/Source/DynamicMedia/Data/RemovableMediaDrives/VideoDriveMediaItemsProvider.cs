using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicMedia.Views.RemovableMediaDrives;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using Okra.Data;
using MediaPortal.Common.ResourceAccess;
using MediaPortal.Common;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Utilities.FileSystem;

namespace DynamicMedia.Data.RemovableMediaDrives
{
  public class VideoDriveMediaItemsProvider : PagedDataListSource<MediaItem>
  {
    protected MediaItem _mediaItem;

    public static VideoDriveMediaItemsProvider TryCreateVideoDriveMediaItemsProvider(DriveInfo driveInfo, IEnumerable<Guid> extractedMIATypeIds)
    {
      VideoMediaType mediaType;

      string drive = driveInfo.Name;
      if (string.IsNullOrEmpty(drive) || drive.Length < 2)
        return null;
      drive = drive.Substring(0, 2); // Clip potential '\\' at the end

      if (!DetectVideoMedia(drive, out mediaType))
        return null;

      try
      {
        return new VideoDriveMediaItemsProvider(driveInfo, extractedMIATypeIds);
      }
      catch (Exception ex)
      {
        ServiceRegistration.Get<ILogger>().Error("RemovableMediaManager: Error accessing video disc {0}", ex, driveInfo.Name);
      }

      return null;
    }

    private static bool DetectVideoMedia(string drive, out VideoMediaType videoMediaType)
    {
      videoMediaType = VideoMediaType.Unknown;
      if (string.IsNullOrEmpty(drive) || drive.Length < 2)
        return false;
      drive = drive.Substring(0, 2); // Clip potential '\\' at the end

      try
      {
        if (Directory.Exists(drive + "\\BDMV"))
        {
          ServiceRegistration.Get<ILogger>().Info("RemovableMediaManager: BD inserted into drive {0}", drive);
          videoMediaType = VideoMediaType.VideoBD;
          return true;
        }

        if (Directory.Exists(drive + "\\VIDEO_TS"))
        {
          ServiceRegistration.Get<ILogger>().Info("RemovableMediaManager: DVD inserted into drive {0}", drive);
          videoMediaType = VideoMediaType.VideoDVD;
          return true;
        }

        if (Directory.Exists(drive + "\\MPEGAV"))
        {
          ServiceRegistration.Get<ILogger>().Info("RemovableMediaManager: Video CD inserted into drive {0}", drive);
          videoMediaType = VideoMediaType.VideoCD;
          return true;
        }
      }
      catch (IOException)
      {
        ServiceRegistration.Get<ILogger>().Warn("VideoDriveHandler: Error checking for video CD in drive {0}", drive);
        return false;
      }

      return false;
    }

    private VideoDriveMediaItemsProvider(DriveInfo driveInfo, IEnumerable<Guid> extractedMIATypeIds)
    {
      IMediaAccessor mediaAccessor = ServiceRegistration.Get<IMediaAccessor>();
      ResourcePath resourcePath = LocalFsResourceProviderBase.ToResourcePath(driveInfo.Name);
      IResourceAccessor resourceAccessor;
      if(!resourcePath.TryCreateLocalResourceAccessor(out resourceAccessor))
        throw new ArgumentException(string.Format("Unable to access drive '{0}'", driveInfo.Name), "driveInfo");

      using (resourceAccessor)
      {
        _mediaItem = mediaAccessor.CreateLocalMediaItem(resourceAccessor,
                                                        mediaAccessor.GetMetadataExtractorsForMIATypes(
                                                          extractedMIATypeIds));
        if(_mediaItem == null)
          throw new Exception(string.Format("Could not create media item for drive '{0}", driveInfo.Name));

        MediaItemAspect itemAspect = _mediaItem.Aspects[MediaAspect.ASPECT_ID];
        itemAspect.SetAttribute(MediaAspect.ATTR_TITLE, itemAspect.GetAttributeValue(MediaAspect.ATTR_TITLE) + 
          " (" + DriveUtils.GetDriveNameWithoutRootDirectory(driveInfo) + ")");
      }
    }

    protected override Task<DataListPageResult<MediaItem>> FetchCountAsync()
    {
      return new Task<DataListPageResult<MediaItem>>(() => _mediaItem == null ? new DataListPageResult<MediaItem>(0, null, null, null) : new DataListPageResult<MediaItem>(1, null, null, null));
    }

    protected override Task<DataListPageResult<MediaItem>> FetchPageAsync(int pageNumber)
    {
      return new Task<DataListPageResult<MediaItem>>(() => _mediaItem == null ? new DataListPageResult<MediaItem>(0,null,null,null) :
        new DataListPageResult<MediaItem>(1,1,1, new List<MediaItem>() { _mediaItem }) );
    }

    protected override Task<DataListPageResult<MediaItem>> FetchPageSizeAsync()
    {
      return new Task<DataListPageResult<MediaItem>>(() => new DataListPageResult<MediaItem>(null, 1, null, null));
    }
  }
}
