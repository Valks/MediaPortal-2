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

namespace MediaPortal.Common.UPnP
{
  /// <summary>
  /// Constants of the UPnP system.
  /// </summary>
  public class UPnPTypesAndIds
  {
    // Backend
    // Need to verify
    public const string BACKEND_SERVER_DEVICE_TYPE = "schemas-team-mediaportal-com:device:MP2-Server";
    public const int BACKEND_SERVER_DEVICE_TYPE_VERSION = 1;

    public const string CONTENT_DIRECTORY_SERVICE_TYPE = "schemas-upnp-org:service:ContentDirectory";
    public const int CONTENT_DIRECTORY_SERVICE_TYPE_VERSION = 1;
    public const string CONTENT_DIRECTORY_SERVICE_ID = "urn:upnp-org:serviceId:ContentDirectory";
    
    public const string CONNECTION_MANAGER_SERVICE_TYPE = "urn:schemas-upnp-org:service:ConnectionManager:1";
    public const int CONNECTION_MANAGER_SERVICE_TYPE_VERSION = 1;
    public const string CONTENT_MANAGER_SERVICE_ID = "urn:upnp-org:serviceId:ConnectionManager";
  }
}
