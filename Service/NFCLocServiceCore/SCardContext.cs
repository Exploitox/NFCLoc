using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NFCLoc.Libraries
{
    public class SCardContext
    {
        [DllImport("Winscard.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern Hresult SCardEstablishContext(ScardScope scope, IntPtr reserved1, IntPtr reserved2, [Out] out IntPtr hContext);

        [DllImport("Winscard.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern Hresult SCardListReadersW(IntPtr hContext, IntPtr group, IntPtr readers, [Out] out uint len);

        [DllImport("Winscard.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern Hresult SCardConnectW(IntPtr hContext, string reader, ScardShareMode shareMode, ScardProtocol preferredProtocolFlags, [Out] out IntPtr hCard, [Out] out ScardProtocol activeProtocol);

        [DllImport("Winscard.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern Hresult SCardTransmit(IntPtr hCard, IntPtr sendPci, IntPtr buffer, uint bufferLen, IntPtr recvPci, IntPtr receiveBuffer, [Out] out uint receiveBufferLen);

        [DllImport("Winscard.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern Hresult SCardDisconnect(IntPtr hCard, ScardDisconnect options);

        [DllImport("Winscard.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern Hresult SCardFreeMemory(IntPtr hContext, string reader);

        [DllImport("Winscard.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern Hresult SCardReleaseContext(IntPtr hContext);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr LoadLibraryW(string fileName);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr FreeLibrary(IntPtr hModule);

        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, CharSet = CharSet.Ansi)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string objName);

        [DllImport("Kernel32.dll", EntryPoint = "CopyMemory")]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        private IntPtr _context = IntPtr.Zero;
        private IntPtr _sCardT0Pci;
        private IntPtr _sCardT1Pci;
        private IntPtr _requestIdCommand;

        public SCardContext()
        {
            // get some structs
            IntPtr dll = LoadLibraryW("Winscard.dll");
            if (dll != IntPtr.Zero)
            {
                _sCardT0Pci = Marshal.AllocHGlobal(8);
                CopyMemory(_sCardT0Pci, GetProcAddress(dll, "g_rgSCardT0Pci"), 8);
                _sCardT1Pci = Marshal.AllocHGlobal(8);
                CopyMemory(_sCardT1Pci, GetProcAddress(dll, "g_rgSCardT1Pci"), 8);
                FreeLibrary(dll);
            }
            _requestIdCommand = Marshal.AllocHGlobal(5);
            Marshal.StructureToPtr(new ScardCommand() { BCla = 0xFF, BIns = 0xCA, BP1 = 0x00, BP2 = 0x00, BP3 = 0x00 }, _requestIdCommand, false);
        }

        ~SCardContext()
        {
            Marshal.FreeHGlobal(_sCardT0Pci);
            Marshal.FreeHGlobal(_sCardT1Pci);
            Marshal.FreeHGlobal(_requestIdCommand);
            if(_context != IntPtr.Zero)
            {
                SCardReleaseContext(_context);
            }
        }

        public List<string> GetReaders()
        {
            Hresult hr = Hresult.ScardFUnknownError;
            List<string> readers = new List<string>();
            if (_context == IntPtr.Zero)
            {
                hr = SCardEstablishContext(ScardScope.System, IntPtr.Zero, IntPtr.Zero, out _context);
                if (hr != Hresult.ScardSSuccess)
                {
                    _context = IntPtr.Zero;
                    return readers;
                }
            }
            IntPtr reader = IntPtr.Zero;
            uint len = 0;
            hr = SCardListReadersW(_context, IntPtr.Zero, reader, out len);
            if (hr != Hresult.ScardSSuccess)
            {
                try
                {
                    SCardReleaseContext(_context);
                }
                catch
                {

                }
                finally
                {
                    _context = IntPtr.Zero;
                }
                return readers;
            }
            reader = Marshal.AllocHGlobal((int)len * sizeof(char) * 2);
            hr = SCardListReadersW(_context, IntPtr.Zero, reader, out len);
            int total = 0;
            while (true)
            {
                string s = Marshal.PtrToStringUni(reader + (total * 2));
                if (s == "")
                    break;
                readers.Add(s);
                total += s.Length + 1;
            }
            Marshal.FreeHGlobal(reader);
            return readers;
        }

        public List<string> GetIds()
        {
            List<string> results = new List<string>();
            Hresult hr = Hresult.ScardFUnknownError;
            if (_context == IntPtr.Zero)
            {
                hr = SCardEstablishContext(ScardScope.System, IntPtr.Zero, IntPtr.Zero, out _context);
                if (hr != Hresult.ScardSSuccess)
                {
                    _context = IntPtr.Zero;
                    return results;
                }
            }
            List<string> readers = GetReaders();
            IntPtr card;
            ScardProtocol p;
            foreach(string readerString in readers)
            { 
                hr = SCardConnectW(_context, readerString, ScardShareMode.Shared, ScardProtocol.Any, out card, out p);
                if (hr == Hresult.ScardEInvalidHandle)
                {
                    try
                    {
                        SCardReleaseContext(_context);
                    }
                    catch
                    {

                    }
                    finally
                    {
                        _context = IntPtr.Zero;
                    }
                }
                if(card != IntPtr.Zero)
                {
                    IntPtr id = Marshal.AllocHGlobal(50);
                    uint resLen = 50;
                    IntPtr requestPci;
                    if (p == ScardProtocol.T0)
                        requestPci = _sCardT0Pci;
                    else
                        requestPci = _sCardT1Pci;
                    hr = SCardTransmit(card, requestPci, _requestIdCommand, 5, IntPtr.Zero, id, out resLen);
                    if(hr != Hresult.ScardSSuccess)
                    {
                        // bad things happened!
                    }
                    byte[] bid = new byte[resLen];
                    Marshal.Copy(id, bid, 0, (int)resLen);
                    Marshal.FreeHGlobal(id);
                    string sid = BitConverter.ToString(bid, 0, (int)resLen).ToUpper().Replace("-", "");
                    SCardDisconnect(card, ScardDisconnect.Leave);
                    results.Add(sid);
                }
                //                SCardReleaseContext(context);
            }
            return results;
        }
    }

    public struct ScardCommand
    {
        public byte BCla;   // the instruction class
        public byte BIns;   // the instruction code 
        public byte BP1;    // parameter to the instruction
        public byte BP2;    // parameter to the instruction
        public byte BP3;    // size of I/O transfer
    }

    public struct ScardIoRequest
    {
        public uint DwProtocol;
        public uint CbPciLength;
    }

    public enum Hresult : uint
    {
        ScardSSuccess = 0x0, //	No error was encountered.
        ScardFInternalError = 0x80100001, // An internal consistency check failed
        ScardECancelled = 0x80100002, // The action was cancelled by an SCardCancel request
        ScardEInvalidHandle = 0x80100003, // The supplied handle was invalid
        ScardEInvalidParameter = 0x80100004, // One or more of the supplied parameters could not be properly interpreted
        ScardEInvalidTarget = 0x80100005, // Registry startup information is missing or invalid
        ScardENoMemory = 0x80100006, // Not enough memory available to complete this command. 
        ScardFWaitedTooLong = 0x80100007, // An internal consistency timer has expired
        ScardEInsufficientBuffer = 0x80100008, //  The data buffer to receive returned data is too small for the returned data.
        ScardEUnknownReader = 0x80100009, // The specified reader name is not recognized. 
        ScardETimeout=0x8010000A, // The user-specified timeout value has expired.
        ScardESharingViolation=0x8010000B, // The smart card cannot be accessed because of other connections outstanding.
        ScardENoSmartcard=0x8010000C, // The operation requires a Smart Card, but no Smart Card is currently in the device. 
        ScardEUnknownCard = 0x8010000D, // The specified smart card name is not recognized. 
        ScardECantDispose=0x8010000E, // The system could not dispose of the media in the requested manner.
        ScardEProtoMismatch=0x8010000F, // The requested protocols are incompatible with the protocol currently in use with the smart card.
        ScardENotReady=0x80100010, // The reader or smart card is not ready to accept commands.
        ScardEInvalidValue=0x80100011, // One or more of the supplied parameters values could not be properly interpreted.
        ScardESystemCancelled=0x80100012, // The action was cancelled by the system, presumably to log off or shut down.
        ScardFCommError=0x80100013, // An internal communications error has been detected.
        ScardFUnknownError=0x80100014, // An internal error has been detected, but the source is unknown.
        ScardEInvalidAtr=0x80100015, // An ATR obtained from the registry is not a valid ATR string. 
        ScardENotTransacted=0x80100016, // An attempt was made to end a non-existent transaction.
        ScardEReaderUnavailable=0x80100017, // The specified reader is not currently available for use.
        ScardPShutdown=0x80100018, // The operation has been aborted to allow the server application to exit. 
        ScardEPciTooSmall=0x80100019, // The PCI Receive buffer was too small.
        ScardEReaderUnsupported=0x8010001A, // The reader driver does not meet minimal requirements for support.
        ScardEDuplicateReader=0x8010001B, // The reader driver did not produce a unique reader name. 
        ScardECardUnsupported=0x8010001C, // The smart card does not meet minimal requirements for support.
        ScardENoService=0x8010001D, // The Smart card resource manager is not running. 
        ScardEServiceStopped=0x8010001E, // The Smart card resource manager has shut down. 
        ScardEUnexpected=0x8010001F, // An unexpected card error has occurred. 
        ScardEUnsupportedFeature=0x8010001F, // This smart card does not support the requested feature.
        ScardEIccInstallation=0x80100020, // No primary provider can be found for the smart card.
        ScardEIccCreateorder=0x80100021, // The requested order of object creation is not supported. 
        ScardEDirNotFound=0x80100023, // The identified directory does not exist in the smart card.
        ScardEFileNotFound=0x80100024, // The identified file does not exist in the smart card.
        ScardENoDir=0x80100025, // The supplied path does not represent a smart card directory. 
        ScardENoFile=0x80100026, // The supplied path does not represent a smart card file. 
        ScardENoAccess=0x80100027, // Access is denied to this file.
        ScardEWriteTooMany=0x80100028, // The smart card does not have enough memory to store the information. 
        ScardEBadSeek=0x80100029, // There was an error trying to set the smart card file object pointer. 
        ScardEInvalidChv=0x8010002A, // The supplied PIN is incorrect.
        ScardEUnknownResMng=0x8010002B, // An unrecognized error code was returned from a layered component. 
        ScardENoSuchCertificate=0x8010002C, // The requested certificate does not exist. 
        ScardECertificateUnavailable=0x8010002D, // The requested certificate could not be obtained.
        ScardENoReadersAvailable=0x8010002E, // Cannot find a smart card reader. 
        ScardECommDataLost=0x8010002F, // A communications error with the smart card has been detected. 
        ScardENoKeyContainer=0x80100030, // The requested key container does not exist on the smart card.
        ScardEServerTooBusy=0x80100031, // The Smart Card Resource Manager is too busy to complete this operation.
        ScardWUnsupportedCard=0x80100065, // The reader cannot communicate with the card, due to ATR string configuration conflicts.
        ScardWUnresponsiveCard=0x80100066, // The smart card is not responding to a reset.
        ScardWUnpoweredCard=0x80100067, // Power has been removed from the smart card, so that further communication is not possible. 
        ScardWResetCard=0x80100068, // The smart card has been reset, so any shared state information is invalid.
        ScardWRemovedCard=0x80100069, // The smart card has been removed, so further communication is not possible. 
        ScardWSecurityViolation=0x8010006A, // Access was denied because of a security violation. 
        ScardWWrongChv=0x8010006B, // The card cannot be accessed because the wrong PIN was presented.
        ScardWChvBlocked=0x8010006C, // The card cannot be accessed because the maximum number of PIN entry attempts has been reached. 
        ScardWEof=0x8010006D, // The end of the smart card file has been reached. 
        ScardWCancelledByUser=0x8010006E, // The user pressed "Cancel" on a Smart Card Selection Dialog. 
        ScardWCardNotAuthenticated=0x8010006F, // No PIN was presented to the smart card
    }

    public enum ScardDisconnect : uint
    {
        Leave = 0,
        Reset = 1,
        Unpower = 2,
        Eject = 3
    }

    public enum ScardScope : uint
    {
        User = 0,
        Terminal = 1,
        System = 2,
        Global = 3
    }

    public enum ScardShareMode : uint
    {
        Exclusive = 1,
        Shared = 2,
        Direct = 3
    }

    [Flags]
    public enum ScardProtocol : uint
    {
        T0 = 1,
        T1 = 2,
        Raw = 4,
        T15 = 8,
        Any = 3
    }


}
