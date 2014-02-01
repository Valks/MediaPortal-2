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

using System.Collections.Generic;
using MediaPortal.Common.Services.MPnP;
using UPnP.Infrastructure.Common;
using UPnP.Infrastructure.CP;

namespace MediaPortal.Common.UPnP.MPnP
{
  public class MPnPExtendedDataTypes
  {
    public const string DATATYPES_SCHEMA_URI = "urn:team-mediaportal-com:MP2-UPnP";

    public static readonly UPnPExtendedDataType DtShare = new MPnPDtShare();
    public static readonly UPnPExtendedDataType DtShareEnumeration = new MPnPDtShareEnumeration();
    public static readonly UPnPExtendedDataType DtMediaItemAspectMetadata = new MPnPDtMediaItemAspectMetadata();
    public static readonly UPnPExtendedDataType DtMediaItemQuery = new MPnPDtMediaItemQuery();
    public static readonly UPnPExtendedDataType DtMediaItem = new MPnPDtMediaItem();
    public static readonly UPnPExtendedDataType DtMediaItemEnumeration = new MPnPDtMediaItemEnumeration();
    public static readonly UPnPExtendedDataType DtMediaItemsFilter = new MPnPDtMediaItemsFilter();
    public static readonly UPnPExtendedDataType DtMediaItemAttributeValues = new MPnPDtMediaItemAttributeValues();
    public static readonly UPnPExtendedDataType DtMediaItemAspectEnumeration = new MPnPDtMediaItemAspectEnumeration();
    public static readonly UPnPExtendedDataType DtResourcePathMetadata = new MPnPDtResourcePathMetadata();
    public static readonly UPnPExtendedDataType DtResourcePathMetadataEnumeration = new MPnPDtResourcePathMetadataEnumeration();
    public static readonly UPnPExtendedDataType DtResourceProviderMetadata = new MPnPDtResourceProviderMetadata();
    public static readonly UPnPExtendedDataType DtResourceProviderMetadataEnumeration = new MPnPDtResourceProviderMetadataEnumeration();
    public static readonly UPnPExtendedDataType DtMediaCategoryEnumeration = new MPnPDtMediaCategoryEnumeration();
    public static readonly UPnPExtendedDataType DtMLQueryResultGroupEnumeration = new MPnPDtMLQueryResultGroupEnumeration();
    public static readonly UPnPExtendedDataType DtMPClientMetadataEnumeration = new MPnPDtMPClientMetadataEnumeration();
    public static readonly UPnPExtendedDataType DtPlaylistInformationDataEnumeration = new MPnPDtPlaylistInformationDataEnumeration();
    public static readonly UPnPExtendedDataType DtPlaylistRawData = new MPnPDtPlaylistRawData();
    public static readonly UPnPExtendedDataType DtPlaylistContents = new MPnPDtPlaylistContents();
    public static readonly UPnPExtendedDataType DtUserProfile = new MPnPDtUserProfile();
    public static readonly UPnPExtendedDataType DtUserProfileEnumeration = new MPnPDtUserProfileEnumeration();

    protected static IDictionary<string, UPnPExtendedDataType> _dataTypes = new Dictionary<string, UPnPExtendedDataType>();

    static MPnPExtendedDataTypes()
    {
      AddDataType(DtShare);
      AddDataType(DtShareEnumeration);
      AddDataType(DtMediaItemAspectMetadata);
      AddDataType(DtMediaItemQuery);
      AddDataType(DtMediaItem);
      AddDataType(DtMediaItemEnumeration);
      AddDataType(DtMediaItemsFilter);
      AddDataType(DtMediaItemAttributeValues);
      AddDataType(DtMediaItemAspectEnumeration);
      AddDataType(DtResourcePathMetadata);
      AddDataType(DtResourcePathMetadataEnumeration);
      AddDataType(DtResourceProviderMetadata);
      AddDataType(DtResourceProviderMetadataEnumeration);
      AddDataType(DtMediaCategoryEnumeration);
      AddDataType(DtMLQueryResultGroupEnumeration);
      AddDataType(DtMPClientMetadataEnumeration);
      AddDataType(DtPlaylistInformationDataEnumeration);
      AddDataType(DtPlaylistRawData);
      AddDataType(DtPlaylistContents);
      AddDataType(DtUserProfile);
      AddDataType(DtUserProfileEnumeration);
    }

    public static void AddDataType(UPnPExtendedDataType type)
    {
      _dataTypes.Add(type.SchemaURI + ":" + type.DataTypeName, type);
    }

    /// <summary>
    /// <see cref="DataTypeResolverDlgt"/>
    /// </summary>
    public static bool ResolveDataType(string dataTypeName, out UPnPExtendedDataType dataType)
    {
      return _dataTypes.TryGetValue(dataTypeName, out dataType);
    }
  }
}
