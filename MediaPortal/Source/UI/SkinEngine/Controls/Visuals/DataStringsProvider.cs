using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using MediaPortal.Data.Collections.Generic;
using MediaPortal.UI.SkinEngine.Controls.Visuals.Templates;

namespace MediaPortal.UI.SkinEngine.Controls.Visuals
{
  public class DataStringsProvider : IItemsProvider<string>
  {
    private readonly IList _objects;
    private readonly DataStringProvider _dataStringProvider;

    public DataStringsProvider(IList objects, DataStringProvider dataStringProvider)
    {
      _objects = objects;
      _dataStringProvider = dataStringProvider;
    }

    #region IItemsProvider<string> Members

    public int Count
    {
      get { return _objects.Count; }
    }

    public IList<string> FetchRange(int startIndex, int pageCount, out int overallCount)
    {
      overallCount = Count;
      List<string> dataStrings = new List<string>();
      
      int loopCount = Count < startIndex + pageCount ? Count : startIndex + pageCount;
      for (int i = startIndex; i < loopCount; i++)
      {
        dataStrings.Add(_dataStringProvider.GenerateDataString(_objects[i]).ToLowerInvariant());
      }

      return dataStrings;
    }

    #endregion

    #region INotifyPropertyChanged Members

    public event PropertyChangedEventHandler PropertyChanged;

    protected void FirePropertyChanged(string property)
    {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(property));
    }
    #endregion
  }
}
