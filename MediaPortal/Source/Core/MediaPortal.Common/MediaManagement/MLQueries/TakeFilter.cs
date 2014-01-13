using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MediaPortal.Common.MediaManagement.MLQueries
{
  /// <summary>
  /// Specifies an expression which skips a given number of media items.
  /// </summary>
  public class TakeFilter : IFilter
  {
    protected object _value1;

    public TakeFilter(object value1)
    {
      _value1 = value1;
    }

    [XmlIgnore]
    public object Value1
    {
      get { return _value1; }
      set { _value1 = value; }
    }

    public override string ToString()
    {
      return " LIMIT " + _value1;
    }

    #region Additional members for the XML serialization

    internal TakeFilter() { }

    /// <summary>
    /// For internal use of the XML serialization system only.
    /// </summary>
    [XmlElement("Value1")]
    public object XML_Value1
    {
      get { return _value1; }
      set { _value1 = value; }
    }

    #endregion
  }
}
