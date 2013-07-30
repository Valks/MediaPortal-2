using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicMedia.General;
using MediaPortal.Common.MediaManagement;
using Okra.Data;
using System.IO;

namespace DynamicMedia.Data.RemovableMediaDrives
{
  public class MultimediaDriveMediaItemsProvider : PagedDataListSource<MediaItem>
  {
    protected int _pageSize = 50;
    protected IDataListSource<MediaItem> _mediaItems;

    private MultimediaDriveMediaItemsProvider(DriveInfo driveInfo, ICollection<MediaItem> mediaItems, MultiMediaType mediaType)
    {
      throw new NotImplementedException("Lots of work required still.");
    }

    public static MultimediaDriveMediaItemsProvider TryCreateMultimediaDriveMediaItemsProvider(DriveInfo driveInfo,
                                                                                               IEnumerable<Guid>
                                                                                                 videoMIATypeIds,
                                                                                               IEnumerable<Guid>
                                                                                                 imageMIATypeIds,
                                                                                               IEnumerable<Guid>
                                                                                                 audioMIATypeIds)
    {
      string drive = driveInfo.Name;
      if (string.IsNullOrEmpty(drive) || drive.Length < 2)
        return null;
      drive = drive.Substring(0, 2); // Clip potential '\\' at the end
      string directory = drive + "\\";

      ICollection<MediaItem> mediaItems;
      MultiMediaType mediaType;
      return
        (mediaType =
          MultimediaDirectory.DetectMultimedia(directory, videoMIATypeIds, imageMIATypeIds, audioMIATypeIds,
            out mediaItems)) == MultiMediaType.None
          ? null
          : new MultimediaDriveMediaItemsProvider(driveInfo, mediaItems, mediaType);
    }

    protected override Task<DataListPageResult<MediaItem>> FetchCountAsync()
    {
      return new Task<DataListPageResult<MediaItem>>(() => _mediaItems == null ? 
        new DataListPageResult<MediaItem>(0, null, null, null) : new DataListPageResult<MediaItem>(_mediaItems.GetCountAsync().Result, null, null, null));
    }

    protected override Task<DataListPageResult<MediaItem>> FetchPageAsync(int pageNumber)
    {
      return new Task<DataListPageResult<MediaItem>>(() =>
      {
        if (_mediaItems == null)
          return new DataListPageResult<MediaItem>(0, null, null, null);

        IList<MediaItem> page = new List<MediaItem>();
        for (int i = _pageSize*pageNumber; i < _pageSize; i++)
        {
          MediaItem item = _mediaItems.GetItemAsync(i).Result;
          if (item == null)
            break;
          page.Add(item);
        }

        return new DataListPageResult<MediaItem>(page.Count, _pageSize, pageNumber, page);
      });
    }

    protected override Task<DataListPageResult<MediaItem>> FetchPageSizeAsync()
    {
      return new Task<DataListPageResult<MediaItem>>(() => new DataListPageResult<MediaItem>(null,
                                                                                             _pageSize,
                                                                                             null,
                                                                                             null));
    }
  }
}
