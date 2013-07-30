using DynamicMedia.Data.Base;
using DynamicMedia.Data.RemovableMediaDrives;
using DynamicMedia.Views.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MediaPortal.Common;
using MediaPortal.Common.MediaManagement;
using Okra.Data;
using System.IO;
using MediaPortal.Utilities.FileSystem;
using MediaPortal.Common.Logging;

namespace DynamicMedia.Views.RemovableMediaDrives
{
  public class AudioCDDriveViewSpecification : ViewSpecification
  {
    protected IDataListSource<MediaItem> _mediaItems;

    private AudioCDDriveViewSpecification(string viewDisplayName, IEnumerable<Guid> necessaryMIATypeIds,
                                         IEnumerable<Guid> optionalMIATypeIds, IDataListSource<MediaItem> mediaItemSource)
      : base(viewDisplayName, necessaryMIATypeIds, optionalMIATypeIds)
    {
      _mediaItems = mediaItemSource;
    }

    public static AudioCDDriveViewSpecification TryCreateAudioCDDriveViewSpecification(DriveInfo driveInfo)
    {
      IDataListSource<MediaItem> mediaItems;
      ICollection<Guid> extractedMIATypeIds;
      string viewDisplayName;

      if (!AudioCDMediaItemsProvider.TryCreateAudioCDMediaItemsProvider(driveInfo, out mediaItems, out extractedMIATypeIds))
        return null;

      try
      {
        viewDisplayName = driveInfo.VolumeLabel;
      }
      catch (Exception)
      {
        viewDisplayName = "Audio CD";
      }

      try
      {
        return new AudioCDDriveViewSpecification(viewDisplayName, extractedMIATypeIds, null, mediaItems);
      }
      catch (Exception ex)
      {
        ServiceRegistration.Get<ILogger>().Error("RemovableMediaManager: Error accessing video disc {0}", ex, driveInfo.Name);
      }

      return null;
    }

    public override IDataListSource<MediaItem> GetMediaItemsDataSource()
    {
      return _mediaItems;
    }

    public override IDataListSource<ViewSpecification> GetSubViewSpecificationsDataSource()
    {
      return new EmptyDataProvider<ViewSpecification>();
    }
  }
}
