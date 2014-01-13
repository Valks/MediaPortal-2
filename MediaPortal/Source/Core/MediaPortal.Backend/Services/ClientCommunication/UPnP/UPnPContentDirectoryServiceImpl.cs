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
using MediaPortal.Common;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.MediaManagement.MLQueries;
using MediaPortal.Common.UPnP;
using MediaPortal.Backend.MediaLibrary;
using MediaPortal.Utilities.UPnP;
using UPnP.Infrastructure.Common;
using UPnP.Infrastructure.Dv;
using UPnP.Infrastructure.Dv.DeviceTree;

namespace MediaPortal.Backend.Services.ClientCommunication.UPnP
{
  /// <summary>
  /// Provides the UPnP service for the MediaPortal 2 content directory.
  /// </summary>
  /// <remarks>
  /// This service works similar to the ContentDirectory service of the UPnP standard MediaServer device, but it uses a bit
  /// different data structure for media items, so it isn't compatible with the standard ContentDirectory service. It also
  /// provides special actions to manage shares and media item aspect metadata schemas.
  /// </remarks>
  public class UPnPContentDirectoryServiceImpl : DvService
  {
    //protected AsynchronousMessageQueue _messageQueue;

    //protected DvStateVariable PlaylistsChangeCounter;
    //protected DvStateVariable MIATypeRegistrationsChangeCounter;
    //protected DvStateVariable CurrentlyImportingSharesChangeCounter;
    //protected DvStateVariable RegisteredSharesChangeCounter;

    //protected UInt32 _playlistsChangeCt = 0;
    //protected UInt32 _miaTypeRegistrationsChangeCt = 0;
    //protected UInt32 _currentlyImportingSharesChangeCt = 0;
    //protected UInt32 _registeredSharesChangeCt = 0;

    public UPnPContentDirectoryServiceImpl() : this(
      UPnPTypesAndIds.CONTENT_DIRECTORY_SERVICE_TYPE, UPnPTypesAndIds.CONTENT_DIRECTORY_SERVICE_TYPE_VERSION,
      UPnPTypesAndIds.CONTENT_DIRECTORY_SERVICE_ID)
    {
    }

    public UPnPContentDirectoryServiceImpl(string contentDirectoryServiceType, int contentDirectoryServiceTypeVersion, string contentDirectoryServiceId)
      : base (contentDirectoryServiceType, contentDirectoryServiceTypeVersion, contentDirectoryServiceId)
    {
      #region State Variables

      // (Optional) A_ARG_TYPE_TransferIDs,         string(CSV ui4),              2.5.2
      //DvStateVariable A_ARG_TYPE_TransferIDs = new DvStateVariable("A_ARG_TYPE_TransferIDs", new DvStandardDataType(UPnPStandardDataType.String))
      //{
      //  SendEvents = false
      //};
      //AddStateVariable(A_ARG_TYPE_TransferIDs);
      
      // (Required) A_ARG_TYPE_ObjectID,            string,                       2.5.3
      DvStateVariable A_ARG_TYPE_ObjectID = new DvStateVariable("A_ARG_TYPE_ObjectID", new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false
      };
      AddStateVariable(A_ARG_TYPE_ObjectID);

      // Used for several return values
      // (Required) A_ARG_TYPE_Result,              string,                       2.5.4
      DvStateVariable A_ARG_TYPE_Result = new DvStateVariable("A_ARG_TYPE_Result", new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false
      };
      AddStateVariable(A_ARG_TYPE_Result);

      // (Optional) A_ARG_TYPE_SearchCriteria       string,                       2.5.5
      DvStateVariable A_ARG_TYPE_SearchCriteria = new DvStateVariable("A_ARG_TYPE_SearchCriteria", new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false
      };

      // (Required) A_ARG_TYPE_BrowseFlag           string,                       2.5.6
      DvStateVariable A_ARG_TYPE_BrowseFlag = new DvStateVariable("A_ARG_TYPE_BrowseFlag", new DvStandardDataType(UPnPStandardDataType.String))
      {
        AllowedValueList = new string[]
        {
          "BrowseMetadata",
          "BrowseDirectChildren"
        },
        SendEvents = false
      };
      AddStateVariable(A_ARG_TYPE_BrowseFlag);

      // (Required) A_ARG_TYPE_Filter               string (CSV string),          2.5.7
      DvStateVariable A_ARG_TYPE_Filter = new DvStateVariable("A_ARG_TYPE_Filter", new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false
      };
      AddStateVariable(A_ARG_TYPE_Filter);

      // (Required) A_ARG_TYPE_SortCriteria         string (CSV string),          2.5.8
      DvStateVariable A_ARG_TYPE_SortCriteria = new DvStateVariable("A_ARG_TYPE_SortCriteria", new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false
      };
      AddStateVariable(A_ARG_TYPE_SortCriteria);

      // (Required) A_ARG_TYPE_Index                ui4,                          2.5.9
      DvStateVariable A_ARG_TYPE_Index = new DvStateVariable("A_ARG_TYPE_Index", new DvStandardDataType(UPnPStandardDataType.Ui4))
      {
        SendEvents = false
      };
      AddStateVariable(A_ARG_TYPE_Index);

      // (Required) A_ARG_TYPE_Count                ui4,                          2.5.10
      DvStateVariable A_ARG_TYPE_Count = new DvStateVariable("A_ARG_TYPE_Count", new DvStandardDataType(UPnPStandardDataType.Ui4))
          {
            SendEvents = false
          }; // Is int sufficient here?
      AddStateVariable(A_ARG_TYPE_Count);

      // (Required) A_ARG_TYPE_UpdateId             ui4,                          2.5.11
      DvStateVariable A_ARG_TYPE_UpdateID = new DvStateVariable("A_ARG_TYPE_UpdateID", new DvStandardDataType(UPnPStandardDataType.Ui4));
      AddStateVariable(A_ARG_TYPE_UpdateID);

      // (Optional) A_ARG_TYPE_TransferId,          ui4,                          2.5.12
      // (Optional) A_ARG_TYPE_TransferStatus       string,                       2.5.13
      // (Optional) A_ARG_TYPE_TransferLength       string,                       2.5.14
      // (Optional) A_ARG_TYPE_TransferTotal        string                        2.5.15
      // (Optional) A_ARG_TYPE_TagValueList         string (CSV string),          2.5.16

      // (Optional) A_ARG_TYPE_URI                  uri,                          2.5.17
      DvStateVariable A_ARG_TYPE_URI = new DvStateVariable("A_ARG_TYPE_URI", new DvStandardDataType(UPnPStandardDataType.Uri))
      {
        SendEvents = false
      };
      AddStateVariable(A_ARG_TYPE_URI);

      // (Required) SearchCapabilities              string (CSV string),          2.5.18
      DvStateVariable SearchCapabilities = new DvStateVariable("SearchCapabilities", new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false
      };
      AddStateVariable(SearchCapabilities);

      // (Required) SortCapabilities                string (CSV string),          2.5.19
      DvStateVariable SortCapabilities = new DvStateVariable("SortCapabilities", new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false
      };
      AddStateVariable(SortCapabilities);

      // (Required) SystemUpdateID                  string (CSV string),          2.5.20
      // Evented, Moderated Event, Max Event Rate = 2
      DvStateVariable SystemUpdateID = new DvStateVariable("SystemUpdateID", new DvStandardDataType(UPnPStandardDataType.Ui4))
      {
        SendEvents = true,
        ModeratedMaximumRate = TimeSpan.FromSeconds(2)
      };
      AddStateVariable(SystemUpdateID);

      // (Optional) ContainerUpdateIDs              string (CSV {string, ui4}),   2.5.21
      // Evented, Moderated Event, Max Event Rate = 2

      #endregion

      #region Register Actions
      // UPnP 1.0 - 2.7.1 GetSearchCapabilities - Required
      DvAction getSearchCapabiltiesAction = new DvAction("GetSearchCapabilities", OnGetSearchCapabilities,
        new DvArgument[] {
        }, 
          new DvArgument[] {
            new DvArgument("SearchCaps", SearchCapabilities, ArgumentDirection.Out, true), 
          });
      AddAction(getSearchCapabiltiesAction);

      // UPnP 1.0 - 2.7.2 GetSortCapabilities - Required
      DvAction getSortCapabilitiesAction = new DvAction("GetSortCapabilities", OnGetSortCapabilities,
        new DvArgument[] {
        },
        new DvArgument[] {
          new DvArgument("SortCapabilities", SortCapabilities, ArgumentDirection.Out, true)
        });
      AddAction(getSortCapabilitiesAction);

      // UPnP 1.0 - 2.7.3 GetSystemUpdateId - Required
      DvAction getSystemUpdateIDAction = new DvAction("GetSystemUpdateID", OnGetSystemUpdateID,
        new DvArgument[]
        {
          
        },
        new DvArgument[]
        {
          new DvArgument("Id", SystemUpdateID, ArgumentDirection.Out), 
        });
      AddAction(getSystemUpdateIDAction);

      // UPnP 1.0 - 2.7.4 Browse - Required
      DvAction browseAction = new DvAction("Browse", OnBrowse,
          new DvArgument[] {
            new DvArgument("ObjectId", A_ARG_TYPE_ObjectID, ArgumentDirection.In),              // ParentDirectory (Note: value of zero = root directory)
            new DvArgument("BrowseFlag", A_ARG_TYPE_BrowseFlag, ArgumentDirection.In),          // Section 2.5.6
            new DvArgument("Filter", A_ARG_TYPE_Filter, ArgumentDirection.In),                  // Section 2.5.7
            new DvArgument("StartingIndex", A_ARG_TYPE_Index, ArgumentDirection.In), 
            new DvArgument("RequestedCount", A_ARG_TYPE_Count, ArgumentDirection.In), 
            new DvArgument("SortCriteria", A_ARG_TYPE_SortCriteria, ArgumentDirection.In),      // Section 2.5.8
          },
          new DvArgument[] {
            new DvArgument("Result", A_ARG_TYPE_Result, ArgumentDirection.Out, true),           // Section 2.5.4
            new DvArgument("NumberReturned", A_ARG_TYPE_Count, ArgumentDirection.Out, true),
            new DvArgument("TotalMatches", A_ARG_TYPE_Count, ArgumentDirection.Out, true),
            new DvArgument("UpdateID", A_ARG_TYPE_UpdateID, ArgumentDirection.Out, true), 
          });
      AddAction(browseAction);

      // UPnP 1.0 2.7.5 Search - Optional
      DvAction searchAction = new DvAction("Search", OnSearch,
          new DvArgument[] {
            new DvArgument("ContainerID", A_ARG_TYPE_ObjectID, ArgumentDirection.In),
            new DvArgument("SearchCriteria", A_ARG_TYPE_SearchCriteria, ArgumentDirection.In),
            new DvArgument("Filter", A_ARG_TYPE_Filter, ArgumentDirection.In), 
            new DvArgument("StartingIndex", A_ARG_TYPE_Index, ArgumentDirection.In), 
            new DvArgument("RequestedCount", A_ARG_TYPE_Count, ArgumentDirection.In), 
            new DvArgument("SortCriteria", A_ARG_TYPE_SortCriteria, ArgumentDirection.In), 
          },
          new DvArgument[] {
            new DvArgument("Result", A_ARG_TYPE_Result, ArgumentDirection.Out, true),
            new DvArgument("NumberReturned", A_ARG_TYPE_Count, ArgumentDirection.Out, true),
            new DvArgument("TotalMatches", A_ARG_TYPE_Count, ArgumentDirection.Out, true),
            new DvArgument("UpdateID", A_ARG_TYPE_UpdateID, ArgumentDirection.Out, true), 
          });
      AddAction(searchAction);

      // UPnP 1.0 2.7.6 CreateObject - Optional (Not Implemented).
      // UPnP 1.0 2.7.7 DestoryObject - Optional (Not Implemented).
      // UPnP 1.0 2.7.8 UpdateObject - Optional (Not Implemented).
      // UPnP 1.0 2.7.9 ImportResource - Optional (Not Implemented).
      // UPnP 1.0 2.7.10 ExportResource - Optional (Not Implemented).
      // UPnP 1.0 2.7.11 StopTransferResource - Optional (Not Implemented).
      // UPnP 1.0 2.7.12 GetTransferProgress - Optional (Not Implemented).
      // UPnP 1.0 2.7.13 DeleteResource - Optional (Not Implemented).
      // UPnP 1.0 2.7.14 CreateReference - Optional (Not Implemented).
      // UPnP 1.0 2.7.15 Non-Standard Actions - Moved to seperate implementations, e.g. MPnP, DLNA.

      #endregion

      //_messageQueue = new AsynchronousMessageQueue(this, new string[]
      //  {
      //      ContentDirectoryMessaging.CHANNEL,
      //      ImporterWorkerMessaging.CHANNEL,
      //  });
      //_messageQueue.MessageReceived += OnMessageReceived;
      //_messageQueue.Start();
    }

    //public override void Dispose()
    //{
    //  base.Dispose();
    //  //_messageQueue.Shutdown();
    //}

    //private void OnMessageReceived(AsynchronousMessageQueue queue, SystemMessage message)
    //{
    //  if (message.ChannelName == ContentDirectoryMessaging.CHANNEL)
    //  {
    //    ContentDirectoryMessaging.MessageType messageType = (ContentDirectoryMessaging.MessageType) message.MessageType;
    //    switch (messageType)
    //    {
    //      case ContentDirectoryMessaging.MessageType.PlaylistsChanged:
    //        PlaylistsChangeCounter.Value = ++_playlistsChangeCt;
    //        break;
    //      case ContentDirectoryMessaging.MessageType.MIATypesChanged:
    //        MIATypeRegistrationsChangeCounter.Value = ++_miaTypeRegistrationsChangeCt;
    //        break;
    //      case ContentDirectoryMessaging.MessageType.RegisteredSharesChanged:
    //        RegisteredSharesChangeCounter.Value = ++_registeredSharesChangeCt;
    //        break;
    //      case ContentDirectoryMessaging.MessageType.ShareImportStarted:
    //      case ContentDirectoryMessaging.MessageType.ShareImportCompleted:
    //        CurrentlyImportingSharesChangeCounter.Value = ++_currentlyImportingSharesChangeCt;
    //        break;
    //    }
    //  }
    //  else if (message.ChannelName == ImporterWorkerMessaging.CHANNEL)
    //  {
    //    ImporterWorkerMessaging.MessageType messageType = (ImporterWorkerMessaging.MessageType) message.MessageType;
    //    switch (messageType)
    //    {
    //      case ImporterWorkerMessaging.MessageType.ImportStarted:
    //      case ImporterWorkerMessaging.MessageType.ImportCompleted:
    //        CurrentlyImportingSharesChangeCounter.Value = ++_currentlyImportingSharesChangeCt;
    //        break;
    //    }
    //  }
    //}

    #region Action Implementation

    static UPnPError OnGetSearchCapabilities(DvAction action, IList<object> inParams, out IList<object> outParams, CallContext context)
    {
      // Not Supported.
      outParams = new List<object>
      {
        ""
      };

      return null;
    }

    static UPnPError OnGetSortCapabilities(DvAction action, IList<object> inParams, out IList<object> outParams, CallContext context)
    {
      // Not Supported
      outParams = new List<object>
      {
        ""
      };

      return null;
    }

    static UPnPError OnGetSystemUpdateID(DvAction action, IList<object> inParams, out IList<object> outParams, CallContext context)
    {
      // Polling for changes not supported, restart your media server.
      outParams = new List<object>
      {
        0
      };

      return null;
    }

    // TODO: update to UPnP Version
    static UPnPError OnBrowse(DvAction action, IList<object> inParams, out IList<object> outParams, CallContext context)
    {
      Guid parentDirectoryId = MarshallingHelper.DeserializeGuid((string) inParams[0]);
      IEnumerable<Guid> necessaryMIATypes = MarshallingHelper.ParseCsvGuidCollection((string) inParams[1]);
      IEnumerable<Guid> optionalMIATypes = MarshallingHelper.ParseCsvGuidCollection((string) inParams[2]);
      IList<MediaItem> result = ServiceRegistration.Get<IMediaLibrary>().Browse(parentDirectoryId, necessaryMIATypes, optionalMIATypes, (int)inParams[3], (int)inParams[4]);

      outParams = new List<object>
      {
        result,       // Result
        null,         // NumberReturned
        result.Count, // TotalMatches
        null          // UpdateID
      };
      return null;
    }

    static UPnPError OnSearch(DvAction action, IList<object> inParams, out IList<object> outParams, CallContext context)
    {
      // In parameters
      string containerId = (string)inParams[0];
      string searchCriteria = inParams[1].ToString();
      string filter = inParams[2].ToString();
      int startIndex = Convert.ToInt32(inParams[3]);
      int requestedCount = Convert.ToInt32(inParams[4]);
      string sortCriteria = (string)inParams[5];

      var parentDirectoryId = containerId == "0" ? Guid.Empty : MarshallingHelper.DeserializeGuid(containerId);
      var necessaryMIATypes = new List<Guid> { DirectoryAspect.ASPECT_ID };

      MediaItemQuery query = new MediaItemQuery(necessaryMIATypes, null);
      
      // Doc
      if(requestedCount != 0)
        query.Filter = new BooleanCombinationFilter(BooleanOperator.And, new IFilter[]
        {
          new TakeFilter(requestedCount), 
          new SkipFilter(startIndex), 
        });

      IList<MediaItem> mediaItems = ServiceRegistration.Get<IMediaLibrary>().Search(query, false);



      outParams = new List<object>
      {
        mediaItems,       // Result
        null,             // NumberReturned
        mediaItems.Count, // TotalMatches
        null              // UpdateID
      };

      return null;
    }

    #endregion
  }
}
