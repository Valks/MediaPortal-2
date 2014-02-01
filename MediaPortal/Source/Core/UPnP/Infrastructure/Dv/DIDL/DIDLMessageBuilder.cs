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
using System.Text;
using System.Xml;
using UPnP.Infrastructure.Utils;

namespace UPnP.Infrastructure.Dv.DIDL
{
  public class DIDLMessageBuilder
  {
    public static string BuildMessage()
    {
      StringBuilder result = new StringBuilder(1000);
      using (StringWriterWithEncoding stringWriter = new StringWriterWithEncoding(result, UPnPConsts.UTF8_NO_BOM))
      using (XmlWriter writer = XmlWriter.Create(stringWriter, UPnPConfiguration.DEFAULT_XML_WRITER_SETTINGS))
      {
        writer.WriteStartDocument();
        writer.WriteStartElement(string.Empty, "DIDL-Lite", UPnPConsts.NS_DIDL_LITE_ELEMENT);
        writer.WriteAttributeString("xmlns", "dc", null, UPnPConsts.NS_DIDL_DC_ELEMENT);
        writer.WriteAttributeString("xmlns", "upnp", null, UPnPConsts.NS_UPNP_METADATA);

        // Check if container or item

        writer.WriteEndElement(); // DIDL-Lite
        writer.WriteEndDocument();
        writer.Close();
      }

      return result.ToString();
    }
  }
}
