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
using System.Linq;
using MediaPortal.Backend.ClientCommunication;
using MediaPortal.Common;
using MediaPortal.Common.General;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.MLQueries;
using MediaPortal.Common.Messaging;
using MediaPortal.Common.ResourceAccess;
using MediaPortal.Common.UPnP;
using MediaPortal.Backend.MediaLibrary;
using MediaPortal.Utilities.UPnP;
using UPnP.Infrastructure.Common;
using UPnP.Infrastructure.Dv;
using UPnP.Infrastructure.Dv.DeviceTree;
using RelocationMode=MediaPortal.Backend.MediaLibrary.RelocationMode;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;

namespace MediaPortal.Backend.Services.ClientCommunication
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

    protected DvStateVariable PlaylistsChangeCounter;
    protected DvStateVariable MIATypeRegistrationsChangeCounter;
    protected DvStateVariable CurrentlyImportingSharesChangeCounter;
    protected DvStateVariable RegisteredSharesChangeCounter;

    protected UInt32 _playlistsChangeCt = 0;
    protected UInt32 _miaTypeRegistrationsChangeCt = 0;
    protected UInt32 _currentlyImportingSharesChangeCt = 0;
    protected UInt32 _registeredSharesChangeCt = 0;


    public UPnPContentDirectoryServiceImpl() : this(
      UPnPTypesAndIds.CONTENT_DIRECTORY_SERVICE_TYPE, UPnPTypesAndIds.CONTENT_DIRECTORY_SERVICE_TYPE_VERSION,
      UPnPTypesAndIds.CONTENT_DIRECTORY_SERVICE_ID)
    {
    }

    public UPnPContentDirectoryServiceImpl(string serviceType, int serviceTypeVersion, string serviceId) : base(
      serviceType, serviceTypeVersion, serviceId)
    {
      // Basic Types

      // Used for a boolean value
      DvStateVariable A_ARG_TYPE_Bool = new DvStateVariable("A_ARG_TYPE_Bool", new DvStandardDataType(UPnPStandardDataType.Boolean))
          {
            SendEvents = false
          };
      AddStateVariable(A_ARG_TYPE_Bool);

      // Used for any single GUID value
      DvStateVariable A_ARG_TYPE_Uuid = new DvStateVariable("A_ARG_TYPE_Id", new DvStandardDataType(UPnPStandardDataType.Uuid))
          {
            SendEvents = false
          };
      AddStateVariable(A_ARG_TYPE_Uuid);

      // CSV of GUID strings
      DvStateVariable A_ARG_TYPE_UuidEnumeration = new DvStateVariable("A_ARG_TYPE_UuidEnumeration", new DvStandardDataType(UPnPStandardDataType.String))
          {
            SendEvents = false
          };
      AddStateVariable(A_ARG_TYPE_UuidEnumeration);

      // Used for a system ID string
      DvStateVariable A_ARG_TYPE_SystemId = new DvStateVariable("A_ARG_TYPE_SystemId", new DvStandardDataType(UPnPStandardDataType.String))
          {
            SendEvents = false
          };
      AddStateVariable(A_ARG_TYPE_SystemId);
      
      // UPnP 1.0 State variables
      
      // (Optional) TransferIDs                 string (CSV ui4),             2.5.2
      //  Evented = true (Not Moderated)

      // Used for several parameters
      DvStateVariable A_ARG_TYPE_ObjectId = new DvStateVariable("A_ARG_TYPE_ObjectId", new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false
      };
      AddStateVariable(A_ARG_TYPE_ObjectId);

      // Used for several return values
      DvStateVariable A_ARG_TYPE_Result = new DvStateVariable("A_ARG_TYPE_Result", new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false
      };
      AddStateVariable(A_ARG_TYPE_Result);

      // (Optional) A_ARG_TYPE_SearchCriteria   string,                       2.5.5
      DvStateVariable A_ARG_TYPE_SearchCriteria = new DvStateVariable("A_ARG_TYPE_SearchCriteria", new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false
      };
      AddStateVariable(A_ARG_TYPE_SearchCriteria);

      // Used for several parameters
      DvStateVariable A_ARG_TYPE_BrowseFlag = new DvStateVariable("A_ARG_TYPE_BrowseFlag", new DvStandardDataType(UPnPStandardDataType.String))
      {
        AllowedValueList = new string[] { "BrowseMetadata", "BrowseDirectChildren" },
        SendEvents = false
      };
      AddStateVariable(A_ARG_TYPE_BrowseFlag);

      // Used for several parameters
      DvStateVariable A_ARG_TYPE_Filter = new DvStateVariable("A_ARG_TYPE_Filter", new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false
      };
      AddStateVariable(A_ARG_TYPE_Filter);

      // Used for several parameters
      DvStateVariable A_ARG_TYPE_SortCriteria = new DvStateVariable("A_ARG_TYPE_SortCriteria", new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false
      };
      AddStateVariable(A_ARG_TYPE_SortCriteria);

      // Used for several parameters
      DvStateVariable A_ARG_TYPE_Index = new DvStateVariable("A_ARG_TYPE_Index", new DvStandardDataType(UPnPStandardDataType.Ui4))
      {
        SendEvents = false
      }; // Is int sufficent here?
      AddStateVariable(A_ARG_TYPE_Index);

      // Used for several parameters and result values
      DvStateVariable A_ARG_TYPE_Count = new DvStateVariable("A_ARG_TYPE_Count", new DvStandardDataType(UPnPStandardDataType.Ui4))
          {
            SendEvents = false
          }; // Is int sufficient here?
      AddStateVariable(A_ARG_TYPE_Count);

      // Used to indicate a change has occured,
      DvStateVariable A_ARG_TYPE_UpdateID = new DvStateVariable("A_ARG_TYPE_UpdateID", new DvStandardDataType(UPnPStandardDataType.Ui4));
      AddStateVariable(A_ARG_TYPE_UpdateID);

      // (Optional) A_ARG_TYPE_TransferId,      ui4,                          2.5.12
      // (Optional) A_ARG_TYPE_TransferStatus   string,                       2.5.13
      // (Optional) A_ARG_TYPE_TransferLength   string,                       2.5.14
      // (Optional) A_ARG_TYPE_TransferTotal    string                        2.5.15
      // (Optional) A_ARG_TYPE_TagValueList     string (CSV string),          2.5.16

      // (Optional)
      DvStateVariable A_ARG_TYPE_URI = new DvStateVariable("A_ARG_TYPE_URI", new DvStandardDataType(UPnPStandardDataType.Uri))
      {
        SendEvents = false
      };
      AddStateVariable(A_ARG_TYPE_URI);

      // TODO: Define
      DvStateVariable SearchCapabilities = new DvStateVariable("SearchCapabilities", new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false
      };
      AddStateVariable(SearchCapabilities);

      // TODO: Define
      DvStateVariable SortCapabilities = new DvStateVariable("SortCapabilities", new DvStandardDataType(UPnPStandardDataType.String))
      {
        SendEvents = false
      };
      AddStateVariable(SortCapabilities);

      // TODO: Define
      // Evented, Moderated Event, Max Event Rate = 2
      DvStateVariable SystemUpdateID = new DvStateVariable("SystemUpdateID", new DvStandardDataType(UPnPStandardDataType.Ui4))
      {
        SendEvents = true,
        ModeratedMaximumRate = TimeSpan.FromSeconds(2)
      };
      AddStateVariable(SystemUpdateID);

      // (Optional) ContainerUpdateIDs          string (CSV {string, ui4}),   2.5.21
      // Evented, Moderated Event, Max Event Rate = 2

      // Capabilities

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

      // Polling Functions

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

      // More actions go here

      // UPnP 1.0 - 2.7.4 Browse - Required
      DvAction browseAction = new DvAction("Browse", OnBrowse,
          new DvArgument[] {
            new DvArgument("ObjectId", A_ARG_TYPE_Uuid, ArgumentDirection.In),               // ParentDirectory
            new DvArgument("BrowseFlag", A_ARG_TYPE_UuidEnumeration, ArgumentDirection.In),  // NecessaryMIATypes
            new DvArgument("Filter", A_ARG_TYPE_UuidEnumeration, ArgumentDirection.In),      // OptionalMIATypes
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
      AddAction(browseAction);

      // UPnP 1.0 - 2.7.5 Search - Optional
      DvAction searchAction = new DvAction("Search", OnSearch,
          new DvArgument[] {
            new DvArgument("ContainerID", A_ARG_TYPE_ObjectId, ArgumentDirection.In),
            new DvArgument("SearchCriteria", A_ARG_TYPE_SearchCriteria, ArgumentDirection.In),
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

      // UPnP 1.0 - 2.7.6 CreateObject - Optional
      // UPnP 1.0 - 2.7.7 DestoryObject - Optional
      // UPnP 1.0 - 2.7.8 UpdateObject - Optional
      // UPnP 1.0 - 2.7.9 ImportResource - Optional
      // UPnP 1.0 - 2.7.10 ExportResource - Optional
      // UPnP 1.0 - 2.7.11 StopTransferResource - Optional
      // UPnP 1.0 - 2.7.12 GetTransferProgress - Optional
      // UPnP 1.0 - 2.7.13 DeleteResource - Optional
      // UPnP 1.0 - 2.7.14 CreateReference - Optional
      
      // UPnP 1.0 - 2.7.15 Non-Stanard Actions Implementations - Don't add the UPnP, extend it and implement in your own library
      // use MPnP as an example!

      //_messageQueue = new AsynchronousMessageQueue(this, new string[]
      //  {
      //      ContentDirectoryMessaging.CHANNEL,
      //      ImporterWorkerMessaging.CHANNEL,
      //  });
      //_messageQueue.MessageReceived += OnMessageReceived;
      //_messageQueue.Start();
    }

    public override void Dispose()
    {
      base.Dispose();
      //_messageQueue.Shutdown();
    }

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

    static UPnPError ParseOnlineState(string argumentName, string onlineStateStr, out bool all)
    {
      switch (onlineStateStr)
      {
        case "All":
          all = true;
          break;
        case "OnlyOnline":
          all = false;
          break;
        default:
          all = true;
          return new UPnPError(600, string.Format("Argument '{0}' must be of value 'All' or 'OnlyOnline'", argumentName));
      }
      return null;
    }

    static UPnPError ParseSearchMode(string argumentName, string searchModeStr, out bool excludeCLOBs)
    {
      switch (searchModeStr)
      {
        case "Normal":
          excludeCLOBs = false;
          break;
        case "ExcludeCLOBs":
          excludeCLOBs = true;
          break;
        default:
          excludeCLOBs = true;
          return new UPnPError(600, string.Format("Argument '{0}' must be of value 'Normal' or 'ExcludeCLOBs'", argumentName));
      }
      return null;
    }

    static UPnPError ParseCapitalizationMode(string argumentName, string searchModeStr, out bool caseSensitive)
    {
      switch (searchModeStr)
      {
        case "CaseSensitive":
          caseSensitive = true;
          break;
        case "CaseInsensitive":
          caseSensitive = false;
          break;
        default:
          caseSensitive = true;
          return new UPnPError(600, string.Format("Argument '{0}' must be of value 'CaseSensitive' or 'CaseInsensitive'", argumentName));
      }
      return null;
    }

    static UPnPError ParseProjectionFunction(string argumentName, string projectionFunctionStr, out ProjectionFunction projectionFunction)
    {
      switch (projectionFunctionStr)
      {
        case "None":
          projectionFunction = ProjectionFunction.None;
          break;
        case "DateToYear":
          projectionFunction = ProjectionFunction.DateToYear;
          break;
        default:
          projectionFunction = ProjectionFunction.None;
          return new UPnPError(600, string.Format("Argument '{0}' must be of value 'DateToYear' or 'None'", argumentName));
      }
      return null;
    }

    static UPnPError ParseGroupingFunction(string argumentName, string groupingFunctionStr, out GroupingFunction groupingFunction)
    {
      switch (groupingFunctionStr)
      {
        case "FirstCharacter":
          groupingFunction = GroupingFunction.FirstCharacter;
          break;
        default:
          groupingFunction = GroupingFunction.FirstCharacter;
          return new UPnPError(600, string.Format("Argument '{0}' must be of value 'FirstCharacter'", argumentName));
      }
      return null;
    }

    static UPnPError ParseRelocationMode(string argumentName, string relocateMediaItemsStr, out RelocationMode relocationMode)
    {
      switch (relocateMediaItemsStr)
      {
        case "None":
          relocationMode = RelocationMode.None;
          break;
        case "Relocate":
          relocationMode = RelocationMode.Relocate;
          break;
        case "ClearAndReImport":
          relocationMode = RelocationMode.Remove;
          break;
        default:
          relocationMode = RelocationMode.Remove;
          return new UPnPError(600, string.Format("Argument '{0}' must be of value 'Relocate' or 'ClearAndReImport'", argumentName));
      }
      return null;
    }

     static UPnPError OnRegisterShare(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      Share share = (Share) inParams[0];
      ServiceRegistration.Get<IMediaLibrary>().RegisterShare(share);
      outParams = null;
      return null;
    }

    static UPnPError OnRemoveShare(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      Guid shareId = MarshallingHelper.DeserializeGuid((string) inParams[0]);
      ServiceRegistration.Get<IMediaLibrary>().RemoveShare(shareId);
      outParams = null;
      return null;
    }

    static UPnPError OnUpdateShare(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      Guid shareId = MarshallingHelper.DeserializeGuid((string) inParams[0]);
      ResourcePath baseResourcePath = ResourcePath.Deserialize((string) inParams[1]);
      string shareName = (string) inParams[2];
      string[] mediaCategories = ((string) inParams[3]).Split(',');
      string relocateMediaItemsStr = (string) inParams[4];
      RelocationMode relocationMode;
      UPnPError error = ParseRelocationMode("RelocateMediaItems", relocateMediaItemsStr, out relocationMode);
      if (error != null)
      {
        outParams = null;
        return error;
      }
      IMediaLibrary mediaLibrary = ServiceRegistration.Get<IMediaLibrary>();
      int numAffected = mediaLibrary.UpdateShare(shareId, baseResourcePath, shareName, mediaCategories, relocationMode);
      outParams = new List<object> {numAffected};
      return null;
    }

    static UPnPError OnGetShares(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      string systemId = (string) inParams[0];
      string sharesFilterStr = (string) inParams[1];
      bool all;
      UPnPError error = ParseOnlineState("SharesFilter", sharesFilterStr, out all);
      if (error != null)
      {
        outParams = null;
        return error;
      }
      IDictionary<Guid, Share> shares = ServiceRegistration.Get<IMediaLibrary>().GetShares(systemId);
      ICollection<Share> result;
      if (all)
        result = shares.Values;
      else
      {
        ICollection<string> connectedClientsIds = ServiceRegistration.Get<IClientManager>().ConnectedClients.Select(
            connection => connection.Descriptor.MPFrontendServerUUID).ToList();
        result = new List<Share>();
        foreach (Share share in shares.Values)
          if (connectedClientsIds.Contains(share.SystemId))
            result.Add(share);
      }
      outParams = new List<object> {result};
      return null;
    }

    static UPnPError OnGetShare(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      Guid shareId = MarshallingHelper.DeserializeGuid((string) inParams[0]);
      Share result = ServiceRegistration.Get<IMediaLibrary>().GetShare(shareId);
      outParams = new List<object> {result};
      return null;
    }

    static UPnPError OnReImportShare(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      Guid shareId = MarshallingHelper.DeserializeGuid((string) inParams[0]);
      Share share = ServiceRegistration.Get<IMediaLibrary>().GetShare(shareId);
      ServiceRegistration.Get<IImporterWorker>().ScheduleRefresh(share.BaseResourcePath, share.MediaCategories, true);
      outParams = null;
      return null;
    }

    static UPnPError OnSetupDefaultServerShares(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      ServiceRegistration.Get<IMediaLibrary>().SetupDefaultLocalShares();
      outParams = null;
      return null;
    }

    static UPnPError OnAddMediaItemAspectStorage(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      MediaItemAspectMetadata miam = (MediaItemAspectMetadata) inParams[0];
      ServiceRegistration.Get<IMediaLibrary>().AddMediaItemAspectStorage(miam);
      outParams = null;
      return null;
    }

    static UPnPError OnRemoveMediaItemAspectStorage(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      Guid aspectId = MarshallingHelper.DeserializeGuid((string) inParams[0]);
      ServiceRegistration.Get<IMediaLibrary>().RemoveMediaItemAspectStorage(aspectId);
      outParams = null;
      return null;
    }

    static UPnPError OnGetAllManagedMediaItemAspectTypes(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      ICollection<Guid> result = ServiceRegistration.Get<IMediaLibrary>().GetManagedMediaItemAspectMetadata().Keys;
      outParams = new List<object> {MarshallingHelper.SerializeGuidEnumerationToCsv(result)};
      return null;
    }

    static UPnPError OnGetMediaItemAspectMetadata(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      Guid aspectId = MarshallingHelper.DeserializeGuid((string) inParams[0]);
      MediaItemAspectMetadata miam = ServiceRegistration.Get<IMediaLibrary>().GetManagedMediaItemAspectMetadata(aspectId);
      outParams = new List<object> {miam};
      return null;
    }

    static UPnPError OnLoadItem(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      string systemId = (string) inParams[0];
      ResourcePath path = ResourcePath.Deserialize((string) inParams[1]);
      IEnumerable<Guid> necessaryMIATypes = MarshallingHelper.ParseCsvGuidCollection((string) inParams[2]);
      IEnumerable<Guid> optionalMIATypes = MarshallingHelper.ParseCsvGuidCollection((string) inParams[3]);
      MediaItem mediaItem = ServiceRegistration.Get<IMediaLibrary>().LoadItem(systemId, path,
          necessaryMIATypes, optionalMIATypes);
      outParams = new List<object> {mediaItem};
      return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="action"></param>
    /// <param name="inParams"></param>
    /// <param name="outParams"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    /// <example>
    ///  new DvArgument[] {
    ///  new DvArgument("ObjectId", A_ARG_TYPE_Uuid, ArgumentDirection.In),               // ParentDirectory
    ///  new DvArgument("BrowseFlag", A_ARG_TYPE_UuidEnumeration, ArgumentDirection.In),  // NecessaryMIATypes
    ///  new DvArgument("Filter", A_ARG_TYPE_UuidEnumeration, ArgumentDirection.In),      // OptionalMIATypes
    ///  new DvArgument("StartingIndex", A_ARG_TYPE_Index, ArgumentDirection.In), 
    ///  new DvArgument("RequestedCount", A_ARG_TYPE_Count, ArgumentDirection.In), 
    ///  new DvArgument("SortCriteria", A_ARG_TYPE_SortCriteria, ArgumentDirection.In), 
    ///},
    ///new DvArgument[] {
    ///  new DvArgument("Result", A_ARG_TYPE_Result, ArgumentDirection.Out, true),
    ///  new DvArgument("NumberReturned", A_ARG_TYPE_Count, ArgumentDirection.Out, true),
    ///  new DvArgument("TotalMatches", A_ARG_TYPE_Count, ArgumentDirection.Out, true),
    ///  new DvArgument("UpdateID", A_ARG_TYPE_UpdateID, ArgumentDirection.Out, true), 
    ///});
    /// </example>
    static UPnPError OnBrowse(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      Guid parentDirectoryId = (string)inParams[0] == "0" ? Guid.Empty : MarshallingHelper.DeserializeGuid((string)inParams[0]);
      string browseFlag = inParams[1].ToString();
      string filter = inParams[2].ToString();
      int startIndex = Convert.ToInt32(inParams[3]);
      int requestedCount = Convert.ToInt32(inParams[4]);
      string sortCriteria = (string)inParams[5];

      var necessaryMIATypes = new List<Guid> { DirectoryAspect.ASPECT_ID };

      MediaItemQuery query = new MediaItemQuery(necessaryMIATypes, null);

      if (requestedCount != 0)
        query.Filter = new BooleanCombinationFilter(BooleanOperator.And, new IFilter[]
        {
          new TakeFilter(requestedCount),
          new SkipFilter(startIndex),
        });

      IList<MediaItem> result = ServiceRegistration.Get<IMediaLibrary>().Search(query, false);

      outParams = new List<object>
      {
        result,       // Result
        null,         // NumberReturned
        result.Count, // TotalMatches
        null          // UpdateID
      };
      return null;
    }

    static UPnPError OnSearch(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      MediaItemQuery query = (MediaItemQuery) inParams[0];
      string onlineStateStr = (string) inParams[1];
      if (inParams.Count >= 5)
      {
        if (query.Filter == null)
          query.Filter = new BooleanCombinationFilter(BooleanOperator.And, new IFilter[]
          {
            new TakeFilter(inParams[3]),
            new SkipFilter(inParams[4]), 
          });
        else
        {
          query.Filter = new BooleanCombinationFilter(BooleanOperator.And, new IFilter[]
          {
            query.Filter,
            new TakeFilter(inParams[3]),
            new SkipFilter(inParams[4]), 
          });
        }
      }
      bool all;
      UPnPError error = ParseOnlineState("OnlineState", onlineStateStr, out all);
      if (error != null)
      {
        outParams = null;
        return error;
      }
      IList<MediaItem> mediaItems = ServiceRegistration.Get<IMediaLibrary>().Search(query, !all);
      outParams = new List<object>
      {
        mediaItems,       // Result
        null,             // NumberReturned
        mediaItems.Count, // TotalMatches
        null              // UpdateID
      };
      return null;
    }

    static UPnPError OnTextSearch(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      string searchText = (string) inParams[0];
      IEnumerable<Guid> necessaryMIATypes = MarshallingHelper.ParseCsvGuidCollection((string) inParams[1]);
      IEnumerable<Guid> optionalMIATypes = MarshallingHelper.ParseCsvGuidCollection((string) inParams[2]);
      IFilter filter = (IFilter) inParams[3];
      string searchModeStr = (string) inParams[4];
      string onlineStateStr = (string) inParams[5];
      string capitalizationMode = (string) inParams[6];
      bool excludeCLOBs;
      bool all = false;
      bool caseSensitive = true;
      UPnPError error = ParseSearchMode("SearchMode", searchModeStr, out excludeCLOBs) ?? 
        ParseOnlineState("OnlineState", onlineStateStr, out all) ??
        ParseCapitalizationMode("CapitalizationMode", capitalizationMode, out caseSensitive);
      if (error != null)
      {
        outParams = null;
        return error;
      }
      IMediaLibrary mediaLibrary = ServiceRegistration.Get<IMediaLibrary>();
      MediaItemQuery query = mediaLibrary.BuildSimpleTextSearchQuery(searchText, necessaryMIATypes, optionalMIATypes,
          filter, !excludeCLOBs, caseSensitive);
      IList<MediaItem> mediaItems = mediaLibrary.Search(query, !all);
      outParams = new List<object> {mediaItems};
      return null;
    }

    static UPnPError OnGetValueGroups(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      Guid aspectId = MarshallingHelper.DeserializeGuid((string) inParams[0]);
      string attributeName = (string) inParams[1];
      IFilter selectAttributeFilter = (IFilter) inParams[2];
      string projectionFunctionStr = (string) inParams[3];
      IEnumerable<Guid> necessaryMIATypes = MarshallingHelper.ParseCsvGuidCollection((string) inParams[4]);
      IFilter filter = (IFilter) inParams[5];
      string onlineStateStr = (string) inParams[6];
      IMediaItemAspectTypeRegistration miatr = ServiceRegistration.Get<IMediaItemAspectTypeRegistration>();
      MediaItemAspectMetadata miam;
      outParams = null;
      ProjectionFunction projectionFunction;
      bool all = true;
      UPnPError error = ParseProjectionFunction("ProjectionFunction", projectionFunctionStr, out projectionFunction) ??
          ParseOnlineState("OnlineState", onlineStateStr, out all);
      if (error != null)
        return error;
      if (!miatr.LocallyKnownMediaItemAspectTypes.TryGetValue(aspectId, out miam))
        return new UPnPError(600, string.Format("Media item aspect type '{0}' is unknown", aspectId));
      MediaItemAspectMetadata.AttributeSpecification attributeType;
      if (!miam.AttributeSpecifications.TryGetValue(attributeName, out attributeType))
        return new UPnPError(600, string.Format("Media item aspect type '{0}' doesn't contain an attribute of name '{1}'",
            aspectId, attributeName));
      HomogenousMap values = ServiceRegistration.Get<IMediaLibrary>().GetValueGroups(attributeType, selectAttributeFilter,
          projectionFunction, necessaryMIATypes, filter, !all);
      outParams = new List<object> {values};
      return null;
    }

    static UPnPError OnGroupValueGroups(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      Guid aspectId = MarshallingHelper.DeserializeGuid((string) inParams[0]);
      string attributeName = (string) inParams[1];
      IFilter selectAttributeFilter = (IFilter) inParams[2];
      string projectionFunctionStr = (string) inParams[3];
      IEnumerable<Guid> necessaryMIATypes = MarshallingHelper.ParseCsvGuidCollection((string) inParams[4]);
      IFilter filter = (IFilter) inParams[5];
      string onlineStateStr = (string) inParams[6];
      string groupingFunctionStr = (string) inParams[7];
      outParams = null;
      ProjectionFunction projectionFunction;
      bool all = true;
      GroupingFunction groupingFunction = GroupingFunction.FirstCharacter;
      UPnPError error = ParseProjectionFunction("ProjectionFunction", projectionFunctionStr, out projectionFunction) ??
          ParseOnlineState("OnlineState", onlineStateStr, out all) ??
          ParseGroupingFunction("GroupingFunction", groupingFunctionStr, out groupingFunction);
      if (error != null)
        return error;
      IMediaItemAspectTypeRegistration miatr = ServiceRegistration.Get<IMediaItemAspectTypeRegistration>();
      MediaItemAspectMetadata miam;
      if (!miatr.LocallyKnownMediaItemAspectTypes.TryGetValue(aspectId, out miam))
        return new UPnPError(600, string.Format("Media item aspect type '{0}' is unknown", aspectId));
      MediaItemAspectMetadata.AttributeSpecification attributeType;
      if (!miam.AttributeSpecifications.TryGetValue(attributeName, out attributeType))
        return new UPnPError(600, string.Format("Media item aspect type '{0}' doesn't contain an attribute of name '{1}'",
            aspectId, attributeName));
      IList<MLQueryResultGroup> values = ServiceRegistration.Get<IMediaLibrary>().GroupValueGroups(attributeType,
          selectAttributeFilter, projectionFunction, necessaryMIATypes, filter, !all, groupingFunction);
      outParams = new List<object> {values};
      return null;
    }

    static UPnPError OnCountMediaItems(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      IEnumerable<Guid> necessaryMIATypes = MarshallingHelper.ParseCsvGuidCollection((string) inParams[0]);
      IFilter filter = (IFilter) inParams[1];
      string onlineStateStr = (string) inParams[2];
      outParams = null;
      bool all;
      UPnPError error = ParseOnlineState("OnlineState", onlineStateStr, out all);
      if (error != null)
        return error;
      int numMediaItems = ServiceRegistration.Get<IMediaLibrary>().CountMediaItems(necessaryMIATypes, filter, !all);
      outParams = new List<object> {numMediaItems};
      return null;
    }

    static UPnPError OnGetPlaylists(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      ICollection<PlaylistInformationData> result = ServiceRegistration.Get<IMediaLibrary>().GetPlaylists();
      outParams = new List<object> {result};
      return null;
    }

    static UPnPError OnSavePlaylist(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      PlaylistRawData playlistData = (PlaylistRawData) inParams[0];
      ServiceRegistration.Get<IMediaLibrary>().SavePlaylist(playlistData);
      outParams = null;
      return null;
    }

    static UPnPError OnDeletePlaylist(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      Guid playlistId = MarshallingHelper.DeserializeGuid((string) inParams[0]);
      bool result = ServiceRegistration.Get<IMediaLibrary>().DeletePlaylist(playlistId);
      outParams = new List<object> {result};
      return null;
    }

    static UPnPError OnExportPlaylist(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      Guid playlistId = MarshallingHelper.DeserializeGuid((string) inParams[0]);
      PlaylistRawData result = ServiceRegistration.Get<IMediaLibrary>().ExportPlaylist(playlistId);
      outParams = new List<object> {result};
      return null;
    }

    static UPnPError OnLoadCustomPlaylist(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      IList<Guid> mediaItemIds = MarshallingHelper.ParseCsvGuidCollection((string) inParams[0]);
      IEnumerable<Guid> necessaryMIATypes = MarshallingHelper.ParseCsvGuidCollection((string) inParams[1]);
      IEnumerable<Guid> optionalMIATypes = MarshallingHelper.ParseCsvGuidCollection((string) inParams[2]);
      IList<MediaItem> result = ServiceRegistration.Get<IMediaLibrary>().LoadCustomPlaylist(
          mediaItemIds, necessaryMIATypes, optionalMIATypes);
      outParams = new List<object> {result};
      return null;
    }

    static UPnPError OnAddOrUpdateMediaItem(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      Guid parentDirectoryId = MarshallingHelper.DeserializeGuid((string) inParams[0]);
      string systemId = (string) inParams[1];
      ResourcePath path = ResourcePath.Deserialize((string) inParams[2]);
      IEnumerable<MediaItemAspect> mediaItemAspects = (IEnumerable<MediaItemAspect>) inParams[3];
      Guid mediaItemId = ServiceRegistration.Get<IMediaLibrary>().AddOrUpdateMediaItem(parentDirectoryId, systemId, path, mediaItemAspects);
      outParams = new List<object> {MarshallingHelper.SerializeGuid(mediaItemId)};
      return null;
    }

    static UPnPError OnDeleteMediaItemOrPath(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      string systemId = (string) inParams[0];
      ResourcePath path = ResourcePath.Deserialize((string) inParams[1]);
      bool inclusive = (bool) inParams[2];
      ServiceRegistration.Get<IMediaLibrary>().DeleteMediaItemOrPath(systemId, path, inclusive);
      outParams = null;
      return null;
    }

    static UPnPError OnClientStartedShareImport(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      Guid shareId = MarshallingHelper.DeserializeGuid((string) inParams[0]);
      ServiceRegistration.Get<IMediaLibrary>().ClientStartedShareImport(shareId);
      outParams = null;
      return null;
    }

    static UPnPError OnClientCompletedShareImport(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      Guid shareId = MarshallingHelper.DeserializeGuid((string) inParams[0]);
      ServiceRegistration.Get<IMediaLibrary>().ClientCompletedShareImport(shareId);
      outParams = null;
      return null;
    }

    static UPnPError OnGetCurrentlyImportingShares(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      IMediaLibrary mediaLibrary = ServiceRegistration.Get<IMediaLibrary>();
      outParams = new List<object> {MarshallingHelper.SerializeGuidEnumerationToCsv(mediaLibrary.GetCurrentlyImportingShareIds())};
      return null;
    }

    static UPnPError OnNotifyPlayback(DvAction action, IList<object> inParams, out IList<object> outParams,
        CallContext context)
    {
      IMediaLibrary mediaLibrary = ServiceRegistration.Get<IMediaLibrary>();
      Guid mediaItemId = MarshallingHelper.DeserializeGuid((string) inParams[0]);
      mediaLibrary.NotifyPlayback(mediaItemId);
      outParams = null;
      return null;
    }
  }
}
