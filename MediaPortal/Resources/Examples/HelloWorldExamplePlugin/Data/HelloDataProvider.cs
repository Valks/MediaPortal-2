using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaPortal.Data.Collections.Generic;

namespace HelloWorld.Data
{
  class HelloDataProvider: IItemsProvider<HelloData>
  {
    private int _count;

    public HelloDataProvider(int count)
    {
      _count = count;
    }

    #region IItemsProvider<Customer> Members

    public int Count
    {
      get { return _count; }
    }

    public IList<HelloData> FetchRange(int startIndex, int pageCount, out int overallCount)
    {
      overallCount = Count;
      List<HelloData> customers = new List<HelloData>();
      
      int loopCount = Count < startIndex + pageCount ? Count : startIndex + pageCount;
      for (int i = startIndex; i < loopCount; i++)
      {
        customers.Add(new HelloData()
        {
          Id = i + 1,
          Name = String.Format("Customer {0}", i + 1)
        });
      }

      return customers;
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
