using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using DynamicMedia.Data.Base;
using DynamicMedia.Data.RemovableMediaDrives;
using DynamicMedia.General;
using DynamicMedia.Views.Base;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Utilities.FileSystem;
using Okra.Data;

namespace DynamicMedia.Views.RemovableMediaDrives
{
  public class MultimediaDriveViewSpecification : ViewSpecification
  {

    protected IDataListSource<MediaItem> _mediaItems; 

    private MultimediaDriveViewSpecification(string viewDisplayName, IEnumerable<Guid> necessaryMIATypeIds, IEnumerable<Guid> optionalMIATypeIds, IDataListSource<MediaItem> mediaItems) 
      : base(viewDisplayName, necessaryMIATypeIds, optionalMIATypeIds)
    {
      if(mediaItems == null)
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

    public static MultimediaDriveViewSpecification TryCreateMultimediaCDDriveViewSpecification(DriveInfo driveInfo, Guid[] necessaryVideoMias, Guid[] necessaryImageMias, Guid[] necessaryAudioMias)
    {
      string viewDisplayName;

      IDataListSource<MediaItem> mediaItems =
        MultimediaDriveMediaItemsProvider.TryCreateMultimediaDriveMediaItemsProvider(
          driveInfo,
          necessaryVideoMias,
          necessaryImageMias, necessaryAudioMias);
      if (mediaItems == null)
        return null;

      try
      {
        viewDisplayName = string.Format("{0} ({1})", driveInfo.VolumeLabel,
          DriveUtils.GetDriveNameWithoutRootDirectory(driveInfo));
      }
      catch (Exception)
      {
        viewDisplayName = "Multimedia Disc";
      }

      try
      {
        var necessaryMias = new List<Guid>();
        if (necessaryVideoMias != null) necessaryMias.AddRange(necessaryVideoMias);
        if (necessaryImageMias != null) necessaryMias.AddRange(necessaryImageMias);
        if (necessaryAudioMias != null) necessaryMias.AddRange(necessaryAudioMias);

        return new MultimediaDriveViewSpecification(viewDisplayName, necessaryMias, null, mediaItems);
      }
      catch (Exception ex)
      {
        ServiceRegistration.Get<ILogger>().Error("MultimediaDriveViewSpecification: Error accessing multimedia disc {0}", ex, driveInfo.Name);
      }

      return null;
    }
  }
}
