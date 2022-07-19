// https://github.com/h4kbas/nfc-reader

using System;
using System.Runtime.InteropServices;

namespace ZeroKey.UI.View.NFC
{
    public static class Card
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct SCardIoRequest
        {
            public int dwProtocol;
            public int cbPciLength;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct SCardReaderState
        {
            public string RdrName;
            public int UserData;
            public int RdrCurrState;
            public int RdrEventState;
            public int ATRLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 37)]
            public byte[] ATRValue;
        }

        public const int SCardSSuccess = 0;


        /* ===========================================================
        '  Memory Card type constants
        '===========================================================*/
        public const int CtMcu = 0x00;                   // MCU
        public const int CtIicAuto = 0x01;               // IIC (Auto Detect Memory Size)
        public const int CtIic1K = 0x02;                 // IIC (1K)
        public const int CtIic2K = 0x03;                 // IIC (2K)
        public const int CtIic4K = 0x04;                 // IIC (4K)
        public const int CtIic8K = 0x05;                 // IIC (8K)
        public const int CtIic16K = 0x06;                // IIC (16K)
        public const int CtIic32K = 0x07;                // IIC (32K)
        public const int CtIic64K = 0x08;                // IIC (64K)
        public const int CtIic128K = 0x09;               // IIC (128K)
        public const int CtIic256K = 0x0A;               // IIC (256K)
        public const int CtIic512K = 0x0B;               // IIC (512K)
        public const int CtIic1024K = 0x0C;              // IIC (1024K)
        public const int CtAt88Sc153 = 0x0D;              // AT88SC153
        public const int CtAt88Sc1608 = 0x0E;             // AT88SC1608
        public const int CtSle4418 = 0x0F;                // SLE4418
        public const int CtSle4428 = 0x10;                // SLE4428
        public const int CtSle4432 = 0x11;                // SLE4432
        public const int CtSle4442 = 0x12;                // SLE4442
        public const int CtSle4406 = 0x13;                // SLE4406
        public const int CtSle4436 = 0x14;                // SLE4436
        public const int CtSle5536 = 0x15;                // SLE5536
        public const int CtMcut0 = 0x16;                  // MCU T=0
        public const int CtMcut1 = 0x17;                  // MCU T=1
        public const int CtMcuAuto = 0x18;               // MCU Autodetect

        /*===============================================================
        ' CONTEXT SCOPE
        ===============================================================	*/
        public const int SCardScopeUser = 0;
        /*===============================================================
        ' The context is a user context, and any database operations 
        '  are performed within the domain of the user.
        '===============================================================*/
        public const int SCardScopeTerminal = 1;
        /*===============================================================
        ' The context is that of the current terminal, and any database 
        'operations are performed within the domain of that terminal.  
        '(The calling application must have appropriate access permissions 
        'for any database actions.)
        '===============================================================*/

        public const int SCardScopeSystem = 2;
        /*===============================================================
        ' The context is the system context, and any database operations 
        ' are performed within the domain of the system.  (The calling
        ' application must have appropriate access permissions for any 
        ' database actions.)
        '===============================================================*/
        /*=============================================================== 
        ' Context Scope
        '===============================================================*/
        public const int SCardStateUnaware = 0x00;
        /*===============================================================
        ' The application is unaware of the current state, and would like 
        ' to know. The use of this value results in an immediate return
        ' from state transition monitoring services. This is represented
        ' by all bits set to zero.
        '===============================================================*/
        public const int SCardStateIgnore = 0x01;
        /*===============================================================
        ' The application requested that this reader be ignored. No other
        ' bits will be set.
        '===============================================================*/

        public const int SCardStateChanged = 0x02;
        /*===============================================================
        ' This implies that there is a difference between the state 
        ' believed by the application, and the state known by the Service
        ' Manager.When this bit is set, the application may assume a
        ' significant state change has occurred on this reader.
        '===============================================================*/

        public const int SCardStateUnknown = 0x04;
        /*===============================================================
        ' This implies that the given reader name is not recognized by
        ' the Service Manager. If this bit is set, then SCARD_STATE_CHANGED
        ' and SCARD_STATE_IGNORE will also be set.
        '===============================================================*/
        public const int SCardStateUnavailable = 0x08;
        /*===============================================================
        ' This implies that the actual state of this reader is not
        ' available. If this bit is set, then all the following bits are
        ' clear.
        '===============================================================*/
        public const int SCardStateEmpty = 0x10;
        /*===============================================================
        '  This implies that there is not card in the reader.  If this bit
        '  is set, all the following bits will be clear.
        '===============================================================*/
        public const int SCardStatePresent = 0x20;
        /*===============================================================
        '  This implies that there is a card in the reader.
        '===============================================================*/
        public const int SCardStateAtrMatch = 0x40;
        /*===============================================================
        '  This implies that there is a card in the reader with an ATR
        '  matching one of the target cards. If this bit is set,
        '  SCARD_STATE_PRESENT will also be set.  This bit is only returned
        '  on the SCardLocateCard() service.
        '===============================================================*/
        public const int SCardStateExclusive = 0x80;
        /*===============================================================
        '  This implies that the card in the reader is allocated for 
        '  exclusive use by another application. If this bit is set,
        '  SCARD_STATE_PRESENT will also be set.
        '===============================================================*/
        public const int SCardStateInUse = 0x100;
        /*===============================================================
        '  This implies that the card in the reader is in use by one or 
        '  more other applications, but may be connected to in shared mode. 
        '  If this bit is set, SCARD_STATE_PRESENT will also be set.
        '===============================================================*/
        public const int SCardStateMute = 0x200;
        /*===============================================================
        '  This implies that the card in the reader is unresponsive or not
        '  supported by the reader or software.
        ' ===============================================================*/
        public const int SCardStateUnPowered = 0x400;
        /*===============================================================
        '  This implies that the card in the reader has not been powered up.
        ' ===============================================================*/

        public const int SCardShareExclusive = 1;
        /*===============================================================
        ' This application is not willing to share this card with other 
        'applications.
        '===============================================================*/
        public const int SCardShareShared = 2;
        /*===============================================================
        ' This application is willing to share this card with other 
        'applications.
        '===============================================================*/
        public const int SCardShareDirect = 3;
        /*===============================================================
        ' This application demands direct control of the reader, so it 
        ' is not available to other applications.
        '===============================================================*/

        /*===========================================================
        '   Disposition
        '===========================================================*/
        public const int SCardLeaveCard = 0;   // Don't do anything special on close
        public const int SCardResetCard = 1;   // Reset the card on close
        public const int SCardUnpowerCard = 2;   // Power down the card on close
        public const int SCardEjectCard = 3;   // Eject the card on close


        /* ===========================================================
    ' ACS IOCTL class
    '===========================================================*/
        public const long FileDeviceSmartcard = 0x310000; // Reader action IOCTLs

        public const long IoctlSmartcardDirect = FileDeviceSmartcard + 2050 * 4;
        public const long IoctlSmartcardSelectSlot = FileDeviceSmartcard + 2051 * 4;
        public const long IoctlSmartcardDrawLcdbmp = FileDeviceSmartcard + 2052 * 4;
        public const long IoctlSmartcardDisplayLcd = FileDeviceSmartcard + 2053 * 4;
        public const long IoctlSmartcardClrLcd = FileDeviceSmartcard + 2054 * 4;
        public const long IoctlSmartcardReadKeypad = FileDeviceSmartcard + 2055 * 4;
        public const long IoctlSmartcardReadRtc = FileDeviceSmartcard + 2057 * 4;
        public const long IoctlSmartcardSetRtc = FileDeviceSmartcard + 2058 * 4;
        public const long IoctlSmartcardSetOption = FileDeviceSmartcard + 2059 * 4;
        public const long IoctlSmartcardSetLed = FileDeviceSmartcard + 2060 * 4;
        public const long IoctlSmartcardLoadKey = FileDeviceSmartcard + 2062 * 4;
        public const long IoctlSmartcardReadEeprom = FileDeviceSmartcard + 2065 * 4;
        public const long IoctlSmartcardWriteEeprom = FileDeviceSmartcard + 2066 * 4;
        public const long IoctlSmartcardGetVersion = FileDeviceSmartcard + 2067 * 4;
        public const long IoctlSmartcardGetReaderInfo = FileDeviceSmartcard + 2051 * 4;
        public const long IoctlSmartcardSetCardType = FileDeviceSmartcard + 2060 * 4;
        public const long IoctlSmartcardAcr128EscapeCommand = FileDeviceSmartcard + 2079 * 4;

        /*===========================================================
    '   Error Codes
    '===========================================================*/
        public const int ScardFInternalError = -2146435071;
        public const int ScardECancelled = -2146435070;
        public const int ScardEInvalidHandle = -2146435069;
        public const int ScardEInvalidParameter = -2146435068;
        public const int ScardEInvalidTarget = -2146435067;
        public const int ScardENoMemory = -2146435066;
        public const int ScardFWaitedTooLong = -2146435065;
        public const int ScardEInsufficientBuffer = -2146435064;
        public const int ScardEUnknownReader = -2146435063;


        public const int ScardETimeout = -2146435062;
        public const int ScardESharingViolation = -2146435061;
        public const int ScardENoSmartcard = -2146435060;
        public const int ScardEUnknownCard = -2146435059;
        public const int ScardECantDispose = -2146435058;
        public const int ScardEProtoMismatch = -2146435057;


        public const int ScardENotReady = -2146435056;
        public const int ScardEInvalidValue = -2146435055;
        public const int ScardESystemCancelled = -2146435054;
        public const int ScardFCommError = -2146435053;
        public const int ScardFUnknownError = -2146435052;
        public const int ScardEInvalidAtr = -2146435051;
        public const int ScardENotTransacted = -2146435050;
        public const int ScardEReaderUnavailable = -2146435049;
        public const int ScardPShutdown = -2146435048;
        public const int ScardEPciTooSmall = -2146435047;

        public const int ScardEReaderUnsupported = -2146435046;
        public const int ScardEDuplicateReader = -2146435045;
        public const int ScardECardUnsupported = -2146435044;
        public const int ScardENoService = -2146435043;
        public const int ScardEServiceStopped = -2146435042;

        public const int ScardWUnsupportedCard = -2146435041;
        public const int ScardWUnresponsiveCard = -2146435040;
        public const int ScardWUnpoweredCard = -2146435039;
        public const int ScardWResetCard = -2146435038;
        public const int ScardWRemovedCard = -2146435037;

        /*===========================================================
    '   PROTOCOL
    '===========================================================*/
        public const int ScardProtocolUndefined = 0x00;          // There is no active protocol.
        public const int ScardProtocolT0 = 0x01;                 // T=0 is the active protocol.
        public const int ScardProtocolT1 = 0x02;                 // T=1 is the active protocol.
        public const int ScardProtocolRaw = 0x10000;             // Raw is the active protocol.
        //public const int SCARD_PROTOCOL_DEFAULT = 0x80000000;      // Use implicit PTS.
        /*===========================================================
    '   READER STATE
    '===========================================================*/
        public const int ScardUnknown = 0;
        /*===============================================================
    ' This value implies the driver is unaware of the current 
    ' state of the reader.
    '===============================================================*/
        public const int ScardAbsent = 1;
        /*===============================================================
    ' This value implies there is no card in the reader.
    '===============================================================*/
        public const int ScardPresent = 2;
        /*===============================================================
    ' This value implies there is a card is present in the reader, 
    'but that it has not been moved into position for use.
    '===============================================================*/
        public const int ScardSwallowed = 3;
        /*===============================================================
    ' This value implies there is a card in the reader in position 
    ' for use.  The card is not powered.
    '===============================================================*/
        public const int ScardPowered = 4;
        /*===============================================================
    ' This value implies there is power is being provided to the card, 
    ' but the Reader Driver is unaware of the mode of the card.
    '===============================================================*/
        public const int ScardNegotiable = 5;
        /*===============================================================
    ' This value implies the card has been reset and is awaiting 
    ' PTS negotiation.
    '===============================================================*/
        public const int ScardSpecific = 6;
        /*===============================================================
    ' This value implies the card has been reset and specific 
    ' communication protocols have been established.
    '===============================================================*/

        /*==========================================================================
    ' Prototypes
    '==========================================================================*/


        [DllImport("winscard.dll")]
        public static extern int SCardEstablishContext(int dwScope, int pvReserved1, int pvReserved2, ref int phContext);

        [DllImport("winscard.dll")]
        public static extern int SCardReleaseContext(int phContext);

        [DllImport("winscard.dll")]
        public static extern int SCardConnect(int hContext, string szReaderName, int dwShareMode, int dwPrefProtocol, ref int phCard, ref int activeProtocol);

        [DllImport("winscard.dll")]
        public static extern int SCardBeginTransaction(int hCard);

        [DllImport("winscard.dll")]
        public static extern int SCardDisconnect(int hCard, int disposition);

        [DllImport("winscard.dll")]
        public static extern int SCardListReaderGroups(int hContext, ref string mzGroups, ref int pcchGroups);

        [DllImport("winscard.DLL", EntryPoint = "SCardListReadersA", CharSet = CharSet.Ansi)]
        public static extern int SCardListReaders(
            int hContext,
            byte[] groups,
            byte[] readers,
            ref int pcchReaders
        );


        [DllImport("winscard.dll")]
        public static extern int SCardStatus(int hCard, string szReaderName, ref int pcchReaderLen, ref int state, ref int protocol, ref byte atr, ref int atrLen);

        [DllImport("winscard.dll")]
        public static extern int SCardEndTransaction(int hCard, int disposition);

        [DllImport("winscard.dll")]
        public static extern int SCardState(int hCard, ref uint state, ref uint protocol, ref byte atr, ref uint atrLen);

        [DllImport("WinScard.dll")]
        public static extern int SCardTransmit(IntPtr hCard,
            ref SCardIoRequest pioSendPci,
            ref byte[] pbSendBuffer,
            int cbSendLength,
            ref SCardIoRequest pioRecvPci,
            ref byte[] pbRecvBuffer,
            ref int pcbRecvLength);

        [DllImport("winscard.dll")]
        public static extern int SCardTransmit(int hCard, ref SCardIoRequest pioSendRequest, ref byte sendBuff, int sendBuffLen, ref SCardIoRequest pioRecvRequest, ref byte recvBuff, ref int recvBuffLen);

        [DllImport("winscard.dll")]
        public static extern int SCardTransmit(int hCard, ref SCardIoRequest pioSendRequest, ref byte[] sendBuff, int sendBuffLen, ref SCardIoRequest pioRecvRequest, ref byte[] recvBuff, ref int recvBuffLen);

        [DllImport("winscard.dll")]
        public static extern int SCardControl(int hCard, uint dwControlCode, ref byte sendBuff, int sendBuffLen, ref byte recvBuff, int recvBuffLen, ref int pcbBytesReturned);

        [DllImport("winscard.dll")]
        public static extern int SCardGetStatusChange(int hContext, int timeOut, ref SCardReaderState readerState, int readerCount);

        public static string GetScardErrMsg(int returnCode)
        {
            switch (returnCode)
            {
                case ScardECancelled:
                    return ("The action was canceled by an SCardCancel request.");
                case ScardECantDispose:
                    return ("The system could not dispose of the media in the requested manner.");
                case ScardECardUnsupported:
                    return ("The smart card does not meet minimal requirements for support.");
                case ScardEDuplicateReader:
                    return ("The reader driver didn't produce a unique reader name.");
                case ScardEInsufficientBuffer:
                    return ("The data buffer for returned data is too small for the returned data.");
                case ScardEInvalidAtr:
                    return ("An ATR string obtained from the registry is not a valid ATR string.");
                case ScardEInvalidHandle:
                    return ("The supplied handle was invalid.");
                case ScardEInvalidParameter:
                    return ("One or more of the supplied parameters could not be properly interpreted.");
                case ScardEInvalidTarget:
                    return ("Registry startup information is missing or invalid.");
                case ScardEInvalidValue:
                    return ("One or more of the supplied parameter values could not be properly interpreted.");
                case ScardENotReady:
                    return ("The reader or card is not ready to accept commands.");
                case ScardENotTransacted:
                    return ("An attempt was made to end a non-existent transaction.");
                case ScardENoMemory:
                    return ("Not enough memory available to complete this command.");
                case ScardENoService:
                    return ("The smart card resource manager is not running.");
                case ScardENoSmartcard:
                    return ("The operation requires a smart card, but no smart card is currently in the device.");
                case ScardEPciTooSmall:
                    return ("The PCI receive buffer was too small.");
                case ScardEProtoMismatch:
                    return ("The requested protocols are incompatible with the protocol currently in use with the card.");
                case ScardEReaderUnavailable:
                    return ("The specified reader is not currently available for use.");
                case ScardEReaderUnsupported:
                    return ("The reader driver does not meet minimal requirements for support.");
                case ScardEServiceStopped:
                    return ("The smart card resource manager has shut down.");
                case ScardESharingViolation:
                    return ("The smart card cannot be accessed because of other outstanding connections.");
                case ScardESystemCancelled:
                    return ("The action was canceled by the system, presumably to log off or shut down.");
                case ScardETimeout:
                    return ("The user-specified timeout value has expired.");
                case ScardEUnknownCard:
                    return ("The specified smart card name is not recognized.");
                case ScardEUnknownReader:
                    return ("The specified reader name is not recognized.");
                case ScardFCommError:
                    return ("An internal communications error has been detected.");
                case ScardFInternalError:
                    return ("An internal consistency check failed.");
                case ScardFUnknownError:
                    return ("An internal error has been detected, but the source is unknown.");
                case ScardFWaitedTooLong:
                    return ("An internal consistency timer has expired.");
                case SCardSSuccess:
                    return ("No error was encountered.");
                case ScardWRemovedCard:
                    return ("The smart card has been removed, so that further communication is not possible.");
                case ScardWResetCard:
                    return ("The smart card has been reset, so any shared state information is invalid.");
                case ScardWUnpoweredCard:
                    return ("Power has been removed from the smart card, so that further communication is not possible.");
                case ScardWUnresponsiveCard:
                    return ("The smart card is not responding to a reset.");
                case ScardWUnsupportedCard:
                    return ("The reader cannot communicate with the card, due to ATR string configuration conflicts.");
                default:
                    return ("?");
            }
        }
    }
}