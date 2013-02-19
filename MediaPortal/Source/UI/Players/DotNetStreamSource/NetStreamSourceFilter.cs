using System;
using System.IO;
using System.Runtime.InteropServices;
using DirectShow;
using DirectShow.BaseClasses;
using DirectShow.Helper;

namespace MediaPortal.UI.Players
{
  [ComVisible(true)]
  [Guid("BD32F0D8-1E0D-46BE-8DE6-E3C4C5746633")]
  public interface IDotNetStreamSourceFilter
  {
    int SetSourceStream(Stream sourceStream);
  }

  /// <summary>
  /// A simple DirectShow source filter implementation that implements the <see cref="IDotNetStreamSourceFilter"/> interface,
  /// supports the pull model and has one output pin named "Output" after <see cref="SetSourceStream"/> was called with a valid <see cref="Stream"/>.
  /// </summary>
  [ComVisible(true)]
  [Guid("19651B59-6AD6-4FD7-882A-914ED5592BFA")]
  [ClassInterface(ClassInterfaceType.None)]
  public class DotNetStreamSourceFilter : BaseFilter, IDotNetStreamSourceFilter
  {
    protected Stream sourceStream = null;
    protected DotNetStreamOutputPin outputPin = null;

    public DotNetStreamSourceFilter()
      : base(".Net Stream Source Filter")
    {
    }

    ~DotNetStreamSourceFilter()
    {
      sourceStream = null;
    }

    protected override int OnInitializePins()
    {
      outputPin = new DotNetStreamOutputPin("Output", this, sourceStream);
      AddPin(outputPin);
      return NOERROR;
    }

    public int SetSourceStream(Stream sourceStream)
    {
      if (this.sourceStream != null)
        return E_UNEXPECTED;
      else
      {
        this.sourceStream = sourceStream;
        return NOERROR;
      }
    }
  }

  /// <summary>
  /// Output <see cref="IPin"/> that implements <see cref="IAsyncReader"/> as it perform asynchronous read operations, 
  /// delivers data in the form of a byte stream (<see cref="MediaType.Stream"/>) and supports the pull model.
  /// </summary>
  [ComVisible(true)]
  [Guid("8CF6F982-E2A4-4DC4-A437-8E9F8533EA1D")]
  public class DotNetStreamOutputPin : BasePin, IAsyncReader
  {
    protected Stream sourceStream = null;

    public DotNetStreamOutputPin(string _name, BaseFilter _filter, Stream sourceStream)
      : base(_name, _filter, _filter.FilterLock, PinDirection.Output)
    {
      if (sourceStream == null)
      {
        throw new ArgumentException("Paramater cannot be null!", "sourceStream");
      }
      else
      {
        this.sourceStream = sourceStream;
      }
    }

    /// <summary>
    /// Called by the downstream filter (splitter), to flush data from the graph (e.g. after a seek).
    /// Should discard any pending read requests.
    /// </summary>
    /// <returns></returns>
    public override int BeginFlush()
    {
      return E_UNEXPECTED;
    }

    /// <summary>
    /// Ends the flushing operation.
    /// </summary>
    /// <returns></returns>
    public override int EndFlush()
    {
      return E_UNEXPECTED;
    }

    public override int GetMediaType(int iPosition, ref AMMediaType pMediaType)
    {
      lock (m_Filter.FilterLock)
      {
        if (iPosition < 0)
        {
          return E_INVALIDARG;
        }
        if (iPosition > 0)
        {
          return VFW_S_NO_MORE_ITEMS;
        }
        return GetMediaType(ref pMediaType);
      }
    }

    public override int CheckMediaType(AMMediaType pmt)
    {
      lock (m_Filter.FilterLock)
      {
        AMMediaType mt = null;
        AMMediaType.Init(ref mt);
        try
        {
          GetMediaType(ref mt);
          if (AMMediaType.AreEquals(mt, pmt))
          {
            return NOERROR;
          }
        }
        finally
        {
          AMMediaType.Free(ref mt);
          mt = null;
        }
      }
      return E_FAIL;
    }

    int GetMediaType(ref AMMediaType pMediaType)
    {
      if (sourceStream == null) return E_UNEXPECTED;

      pMediaType.majorType = MediaType.Stream;
      pMediaType.subType = MediaSubType.Null;
      pMediaType.formatType = FormatType.None;
      pMediaType.temporalCompression = false;
      pMediaType.sampleSize = 1;

      return NOERROR;
    }

    public override int CheckConnect(ref IPinImpl _pin)
    {
      PinDirection _direction;
      HRESULT hr = (HRESULT)_pin.QueryDirection(out _direction);
      if (hr.Failed) return hr;
      if (_direction == m_Direction)
      {
        return VFW_E_INVALID_DIRECTION;
      }
      return NOERROR;
    }

    public override int CompleteConnect(ref IPinImpl pReceivePin)
    {
      return NOERROR;
    }

    /// <summary>
    /// Requests an allocator during the pin connection.
    /// </summary>
    /// <param name="pPreferred">Pointer to the <see cref="IMemAllocator"/> interface on the input pin's preferred allocator, or NULL.</param>
    /// <param name="pProps">An <see cref="AllocatorProperties"/> structure, allocated by the caller. The caller should fill in any allocator properties that the input pin requires, and set the remaining members to zero.</param>
    /// <param name="ppActual">Pointer that receives an <see cref="IMemAllocator"/>.</param>
    /// <returns></returns>
    public int RequestAllocator(IntPtr pPreferred, AllocatorProperties pProps, out IntPtr ppActual)
    {
      if (pPreferred != null)
      {
        ppActual = pPreferred;
        if (pProps != null)
          new IMemAllocatorImpl(pPreferred).GetProperties(pProps);
        return S_OK;
      }
      ppActual = IntPtr.Zero;
      return E_FAIL;
    }

    /// <summary>
    /// Queues an asynchronous request for data.
    /// </summary>
    /// <param name="pSample">Pointer to an <see cref="IMediaSample"/> provided by the caller.</param>
    /// <param name="dwUser">Specifies an arbitrary value that is returned when the request completes.</param>
    /// <returns></returns>
    public int Request(IntPtr pSample, IntPtr dwUser)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Waits for the next pending read request to complete.
    /// </summary>
    /// <param name="dwTimeout">Specifies a time-out in milliseconds. Use the value INFINITE to wait indefinitely.</param>
    /// <param name="ppSample">Pointer that receives an <see cref="IMediaSample"/>.</param>
    /// <param name="pdwUser">Pointer that receives the value of the dwUser parameter specified in the <see cref="Request"/> method.</param>
    /// <returns></returns>
    public int WaitForNext(int dwTimeout, out IntPtr ppSample, out IntPtr pdwUser)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Performs a synchronous read. The method blocks until the request is completed. 
    /// The file positions and the buffer address must be aligned.
    /// This method performs an unbuffered read, so it might be faster than the <see cref="SyncRead "/> method.
    /// </summary>
    /// <param name="pSample">Pointer to an <see cref="IMediaSample"/> provided by the caller.</param>
    /// <returns></returns>
    public int SyncReadAligned(IntPtr pSample)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Performs a synchronous read. The method blocks until the request is completed. 
    /// The file positions and the buffer address do not have to be aligned.
    /// </summary>
    /// <param name="llPosition">Specifies the byte offset at which to begin reading. The method fails if this value is beyond the end of the file.</param>
    /// <param name="lLength">Specifies the number of bytes to read.</param>
    /// <param name="pBuffer">Pointer to a buffer that receives the data.</param>
    /// <returns></returns>
    public int SyncRead(long llPosition, int lLength, IntPtr pBuffer)
    {
      byte[] array = new byte[lLength];
      if (sourceStream.Position != llPosition)
      {
        sourceStream.Seek(llPosition, SeekOrigin.Begin);
      }
      int read = sourceStream.Read(array, 0, lLength);
      Marshal.Copy(array, 0, pBuffer, read);
      return NOERROR;
    }

    /// <summary>
    /// Retrieves the total length of the stream.
    /// </summary>
    /// <param name="pTotal">Length of the stream (in bytes).</param>
    /// <param name="pAvailable">Portion of the stream that is currently available (in bytes).</param>
    /// <returns></returns>
    public int Length(out long pTotal, out long pAvailable)
    {
      pTotal = pAvailable = sourceStream.Length;
      return NOERROR;
    }
  }
}
