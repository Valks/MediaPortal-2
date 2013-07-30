using DynamicMedia.General;
using DynamicMedia.Views.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MediaPortal.Common.MediaManagement;
using Okra.Data;
using DynamicMedia.Views.RemovableMediaDrives;

namespace DynamicMedia.Views
{
  public class RemovableDriveViewSpecification : ViewSpecification
  {

    protected DriveInfo _driveInfo;
    protected ViewSpecification _removableDriveViewSpecification;

    public RemovableDriveViewSpecification(string drive)
      : base(string.Empty, new Guid[] {}, new Guid[] {})
    {
      _driveInfo = new DriveInfo(drive);
      UpdateRemovableDriveHandler();
    }


    public static IList<RemovableDriveViewSpecification> CreateViewSpecificationsForRemovableDrives(
      IEnumerable<Guid> necessaryMIATypeIds, IEnumerable<Guid> optionalMIATypeIds)
    {
      return DriveInfo.GetDrives().Where(
        driveInfo => driveInfo.DriveType == DriveType.CDRom || driveInfo.DriveType == DriveType.Removable).Select(
          driveInfo => new RemovableDriveViewSpecification(driveInfo.ToString())).ToList();
    }

    public override IDataListSource<MediaItem> GetMediaItemsDataSource()
    {
      return _removableDriveViewSpecification.GetMediaItemsDataSource();
    }

    public override IDataListSource<ViewSpecification> GetSubViewSpecificationsDataSource()
    {
      return _removableDriveViewSpecification.GetSubViewSpecificationsDataSource();
    }

    public override string ViewDisplayName
    {
      get
      {
        string volumeLabel = _removableDriveViewSpecification.ToString();
        return _driveInfo.RootDirectory.Name + (string.IsNullOrEmpty(volumeLabel) ? string.Empty : String.Format(" ({0})", volumeLabel));
      }
    }

    protected void UpdateRemovableDriveHandler()
    {
      _removableDriveViewSpecification =
        VideoDriveViewSpecification.TryCreateVideoDriveViewSpecification(_driveInfo, Consts.NECESSARY_VIDEO_MIAS) ??
        AudioCDDriveViewSpecification.TryCreateAudioCDDriveViewSpecification(_driveInfo) ??
        MultimediaDriveViewSpecification.TryCreateMultimediaCDDriveViewSpecification(_driveInfo,
                                                                                     Consts.NECESSARY_VIDEO_MIAS,
                                                                                     Consts.NECESSARY_IMAGE_MIAS,
                                                                                     Consts.NECESSARY_AUDIO_MIAS) ??
        (ViewSpecification) new UnknownRemovableDriveViewSpecification(_driveInfo);
    }
  }
}
