using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Extensions.ResourceProviders.AudioCDResourceProvider;
using Okra.Data;
using System.IO;
using MediaPortal.Extensions.BassLibraries;
using MediaPortal.Common;
using MediaPortal.Common.SystemResolver;

namespace DynamicMedia.Data.RemovableMediaDrives
{
  public class AudioCDMediaItemsProvider : PagedDataListSource<MediaItem>
  {

    protected string _drive;
    protected Task _readingMediaTask;
    protected IList<MediaItem> _tracks;
    protected int _pageSize = 50;

    public AudioCDMediaItemsProvider(DriveInfo driveInfo)
    {
      if(driveInfo == null)
        throw new ArgumentNullException("driveInfo cannot be null");
      
      _drive = driveInfo.Name.Substring(0, 2); // Clip potential '\\' at the end
      _readingMediaTask = LoadInBackground();
      _readingMediaTask.Start();
    }

    public static bool TryCreateAudioCDMediaItemsProvider(DriveInfo driveInfo, out IDataListSource<MediaItem> mediaSource, out ICollection<Guid> extractedMIATypeIds)
    {
      mediaSource = null;
      extractedMIATypeIds = new List<Guid>()
      {
        ProviderResourceAspect.ASPECT_ID,
        MediaAspect.ASPECT_ID,
        AudioAspect.ASPECT_ID
      };

      if (driveInfo == null)
        return false;

      if (BassUtils.GetNumAudioTracks(driveInfo.Name.Substring(0, 2)) == 0)
        return false;

      mediaSource = new AudioCDMediaItemsProvider(driveInfo);

      return true;
    }

    protected override Task<DataListPageResult<MediaItem>> FetchCountAsync()
    {
      return new Task<DataListPageResult<MediaItem>>(() =>
        {
          _readingMediaTask.Wait();
          return new DataListPageResult<MediaItem>(_tracks.Count, null, null, null);
        });

    }

    protected override Task<DataListPageResult<MediaItem>> FetchPageAsync(int pageNumber)
    {
      return new Task<DataListPageResult<MediaItem>>(() =>
        {
          _readingMediaTask.Wait();
          IList<MediaItem> tracks = _tracks.Skip(_pageSize*pageNumber).Take(_pageSize).ToList();
          return new DataListPageResult<MediaItem>(tracks.Count, _pageSize, pageNumber, tracks);
        });
    }

    protected override Task<DataListPageResult<MediaItem>> FetchPageSizeAsync()
    {
      return new Task<DataListPageResult<MediaItem>>(() =>
        {
          _readingMediaTask.Wait();
          return new DataListPageResult<MediaItem>(null, _pageSize, null, null);
        });
    }

    protected Task LoadInBackground()
    {
      return new Task(() =>
        {
          try
          {
            IList<BassUtils.AudioTrack> audioTracks = BassUtils.GetAudioTracks(_drive);
            if (audioTracks == null || audioTracks.Count == 0)
            {
              _tracks = null;
              return;
            }

            ISystemResolver systemResolver = ServiceRegistration.Get<ISystemResolver>();
            string systemId = systemResolver.LocalSystemId;
            _tracks = new List<MediaItem>(audioTracks.Count);
            char driveChar = _drive[0];
            foreach (BassUtils.AudioTrack audioTrack in audioTracks)
            {
              _tracks.Add(CreateMediaItem(audioTrack, driveChar, audioTracks.Count, systemId));
            }
          }
          catch (IOException)
          {
            ServiceRegistration.Get<ILogger>().Warn("Error enumerating tracks of audio CD in drive {0}", _drive);
            _tracks = new List<MediaItem>();
          }
        });
    }

    protected static MediaItem CreateMediaItem(BassUtils.AudioTrack track, char drive, int numTracks, string systemId)
    {
      IDictionary<Guid, MediaItemAspect> aspects = new Dictionary<Guid, MediaItemAspect>();
      MediaItemAspect providerResourceAspect = MediaItemAspect.GetOrCreateAspect(aspects, ProviderResourceAspect.Metadata);
      MediaItemAspect mediaAspect = MediaItemAspect.GetOrCreateAspect(aspects, MediaAspect.Metadata);
      MediaItemAspect audioAspect = MediaItemAspect.GetOrCreateAspect(aspects, AudioAspect.Metadata);

      // TODO: Collect data from internet for the current audio CD
      providerResourceAspect.SetAttribute(ProviderResourceAspect.ATTR_RESOURCE_ACCESSOR_PATH,
          AudioCDResourceProvider.ToResourcePath(drive, track.TrackNo).Serialize());
      providerResourceAspect.SetAttribute(ProviderResourceAspect.ATTR_SYSTEM_ID, systemId);
      mediaAspect.SetAttribute(MediaAspect.ATTR_TITLE, "Track " + track.TrackNo);
      audioAspect.SetAttribute(AudioAspect.ATTR_TRACK, (int)track.TrackNo);
      audioAspect.SetAttribute(AudioAspect.ATTR_DURATION, (long)track.Duration);
      audioAspect.SetAttribute(AudioAspect.ATTR_ENCODING, "PCM");
      audioAspect.SetAttribute(AudioAspect.ATTR_BITRATE, 1411200); // 44.1 kHz * 16 bit * 2 channel
      audioAspect.SetAttribute(AudioAspect.ATTR_NUMTRACKS, numTracks);

      return new MediaItem(Guid.Empty, aspects);
    }
  }
}
