// https://github.com/h4kbas/nfc-reader

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NFCLoc.UI.View.NFC
{
    public class NfcReader
    {
        public int RetCode, HCard, Protocol;
        int _hContext;
        public bool ConnActive = false;
        public byte[] SendBuff = new byte[263];
        public byte[] RecvBuff = new byte[263];
        public int SendLen, RecvLen;
        internal enum SmartcardState
        {
            None = 0,
            Inserted = 1,
            Ejected = 2
        }

        public delegate void CardEventHandler();
        public event CardEventHandler CardInserted;
        public event CardEventHandler CardEjected;
        public event CardEventHandler DeviceDisconnected;
        private BackgroundWorker _worker;
        private Card.SCardReaderState _rdrState;
        private string _readername;
        private Card.SCardReaderState[] _states;

        private void WaitChangeStatus(object sender, DoWorkEventArgs e)
        {
            while (!e.Cancel)
            {
                int nErrCode = Card.SCardGetStatusChange(_hContext, 1000, ref _states[0], 1);

                if (nErrCode == Card.ScardEServiceStopped)
                {
                    DeviceDisconnected();
                    e.Cancel = true;
                }

                //Check if the state changed from the last time.
                if ((this._states[0].RdrEventState & 2) == 2)
                {
                    //Check what changed.
                    SmartcardState state = SmartcardState.None;
                    if ((this._states[0].RdrEventState & 32) == 32 && (this._states[0].RdrCurrState & 32) != 32)
                    {
                        //The card was inserted. 
                        state = SmartcardState.Inserted;
                    }
                    else if ((this._states[0].RdrEventState & 16) == 16 && (this._states[0].RdrCurrState & 16) != 16)
                    {
                        //The card was ejected.
                        state = SmartcardState.Ejected;
                    }
                    if (state != SmartcardState.None && this._states[0].RdrCurrState != 0)
                    {
                        switch (state)
                        {
                            case SmartcardState.Inserted:
                            {
                                //MessageBox.Show("Card inserted");
                                CardInserted();
                                break;
                            }
                            case SmartcardState.Ejected:
                            {
                                //MessageBox.Show("Card ejected");
                                CardEjected();
                                break;
                            }
                            default:
                            {
                                //MessageBox.Show("Some other state...");
                                break;
                            }
                        }
                    }
                    //Update the current state for the next time they are checked.
                    this._states[0].RdrCurrState = this._states[0].RdrEventState;
                }
            }
        }

        public Card.SCardIoRequest PioSendRequest;

        private int SendApdUandDisplay(int reqType)
        {
            int indx;
            string tmpStr = "";

            PioSendRequest.dwProtocol = Protocol;
            PioSendRequest.cbPciLength = 8;

            //Display Apdu In
            for (indx = 0; indx <= SendLen - 1; indx++)
            {
                tmpStr = tmpStr + " " + string.Format("{0:X2}", SendBuff[indx]);
            }

            RetCode = Card.SCardTransmit(HCard, ref PioSendRequest, ref SendBuff[0],
                SendLen, ref PioSendRequest, ref RecvBuff[0], ref RecvLen);

            if (RetCode != Card.SCardSSuccess)
            {
                return RetCode;
            }

            else
            {
                try
                {
                    tmpStr = "";
                    switch (reqType)
                    {
                        case 0:
                            for (indx = (RecvLen - 2); indx <= (RecvLen - 1); indx++)
                            {
                                tmpStr = tmpStr + " " + string.Format("{0:X2}", RecvBuff[indx]);
                            }

                            if ((tmpStr).Trim() != "90 00")
                            {
                                //MessageBox.Show("Return bytes are not acceptable.");
                                return -202;
                            }

                            break;

                        case 1:

                            for (indx = (RecvLen - 2); indx <= (RecvLen - 1); indx++)
                            {
                                tmpStr = tmpStr + string.Format("{0:X2}", RecvBuff[indx]);
                            }

                            if (tmpStr.Trim() != "90 00")
                            {
                                tmpStr = tmpStr + " " + string.Format("{0:X2}", RecvBuff[indx]);
                            }

                            else
                            {
                                tmpStr = "ATR : ";
                                for (indx = 0; indx <= (RecvLen - 3); indx++)
                                {
                                    tmpStr = tmpStr + " " + string.Format("{0:X2}", RecvBuff[indx]);
                                }
                            }

                            break;

                        case 2:

                            for (indx = 0; indx <= (RecvLen - 1); indx++)
                            {
                                tmpStr = tmpStr + " " + string.Format("{0:X2}", RecvBuff[indx]);
                            }

                            break;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    return -200;
                }
            }
            return RetCode;
        }

        private void ClearBuffers()
        {
            long indx;

            for (indx = 0; indx <= 262; indx++)
            {
                RecvBuff[indx] = 0;
                SendBuff[indx] = 0;
            }
        }

        private bool AuthBlock(String block)
        {
            ClearBuffers();
            SendBuff[0] = 0xFF;                         // CLA
            SendBuff[2] = 0x00;                         // P1: same for all source types 
            SendBuff[1] = 0x86;                         // INS: for stored key input
            SendBuff[3] = 0x00;                         // P2 : Memory location;  P2: for stored key input
            SendBuff[4] = 0x05;                         // P3: for stored key input
            SendBuff[5] = 0x01;                         // Byte 1: version number
            SendBuff[6] = 0x00;                         // Byte 2
            SendBuff[7] = (byte)int.Parse(block);       // Byte 3: sectore no. for stored key input
            SendBuff[8] = 0x61;                         // Byte 4 : Key B for stored key input
            SendBuff[9] = (byte)int.Parse("1");         // Byte 5 : Session key for non-volatile memory

            SendLen = 0x0A;
            RecvLen = 0x02;

            RetCode = SendApdUandDisplay(0);

            if (RetCode != Card.SCardSSuccess)
            {
                //MessageBox.Show("FAIL Authentication! No:" + retCode.ToString());
                return false;
            }

            return true;
        }

        public string GetCardUid()
        {
            string cardUid = "";
            byte[] receivedUid = new byte[256];
            Card.SCardIoRequest request = new Card.SCardIoRequest();
            request.dwProtocol = Card.ScardProtocolT1;
            request.cbPciLength = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Card.SCardIoRequest));
            byte[] sendBytes = new byte[] { 0xFF, 0xCA, 0x00, 0x00, 0x00 };
            int outBytes = receivedUid.Length;
            int status = Card.SCardTransmit(HCard, ref request, ref sendBytes[0], sendBytes.Length, ref request, ref receivedUid[0], ref outBytes);

            if (status != Card.SCardSSuccess)
                cardUid = "";
            else
                cardUid = BitConverter.ToString(receivedUid.Take(4).ToArray()).Replace("-", string.Empty).ToLower();
            return cardUid;
        }

        public List<string> GetReadersList()
        {
            string readerList = "" + Convert.ToChar(0);
            int indx;
            int pcchReaders = 0;
            string rName = "";
            List<string> lstReaders = new List<string>();
            //Establish Context
            RetCode = Card.SCardEstablishContext(Card.SCardScopeUser, 0, 0, ref _hContext);

            if (RetCode != Card.SCardSSuccess)
            {
                throw new Exception("Error SCardEstablishContext");
            }

            // 2. List PC/SC card readers installed in the system

            RetCode = Card.SCardListReaders(this._hContext, null, null, ref pcchReaders);

            if (RetCode != Card.SCardSSuccess)
            {
                throw new Exception("An error occurred while listing smart card readers.");
            }

            byte[] readersList = new byte[pcchReaders];

            // Fill reader list
            RetCode = Card.SCardListReaders(this._hContext, null, readersList, ref pcchReaders);

            if (RetCode != Card.SCardSSuccess)
            {
                throw new Exception("Error SCardListReaders");
            }

            rName = "";
            indx = 0;


            while (readersList[indx] != 0)
            {

                while (readersList[indx] != 0)
                {
                    rName += (char)readersList[indx];
                    indx++;
                }


                lstReaders.Add(rName);
                rName = "";
                indx++;

            }
            return lstReaders;
        }

        public bool WriteBlock(String text, String block)
        {

            char[] tmpStr = text.ToArray();
            int indx;
            if (AuthBlock(block))
            {
                ClearBuffers();
                SendBuff[0] = 0xFF;                             // CLA
                SendBuff[1] = 0xD6;                             // INS
                SendBuff[2] = 0x00;                             // P1
                SendBuff[3] = (byte)int.Parse(block);           // P2 : Starting Block No.
                SendBuff[4] = (byte)int.Parse("16");            // P3 : Data length

                for (indx = 0; indx <= (tmpStr).Length - 1; indx++)
                {
                    SendBuff[indx + 5] = (byte)tmpStr[indx];
                }
                SendLen = SendBuff[4] + 5;
                RecvLen = 0x02;

                RetCode = SendApdUandDisplay(2);

                if (RetCode != Card.SCardSSuccess)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }
        
        public byte[] ReadBlock(String block)
        {
            byte[] tmpStr;
            int indx;

            if (AuthBlock(block))
            {
                ClearBuffers();
                SendBuff[0] = 0xFF; // CLA 
                SendBuff[1] = 0xB0;// INS
                SendBuff[2] = 0x00;// P1
                SendBuff[3] = (byte)int.Parse(block);// P2 : Block No.
                SendBuff[4] = (byte)int.Parse("16");// Le

                SendLen = 5;
                RecvLen = SendBuff[4] + 2;

                RetCode = SendApdUandDisplay(2);

                if (RetCode == -200)
                {
                    return new byte[] { };
                }

                if (RetCode == -202)
                {
                    return new byte[] { };
                }

                if (RetCode != Card.SCardSSuccess)
                {
                    return new byte[] { };
                }

                // Display data in text format
                List<byte> t = new List<byte>();
                for (indx = 0; indx <= RecvLen - 1; indx++)
                {
                    t.Add(RecvBuff[indx]);
                }
                tmpStr = t.ToArray();
                return tmpStr;
            }
            else return new byte[] { };
        }
        public bool Connect()
        {
            string readerName = GetReadersList()[0];
            ConnActive = true;
            RetCode = Card.SCardConnect(_hContext, readerName, Card.SCardShareShared,
                Card.ScardProtocolT0 | Card.ScardProtocolT1, ref HCard, ref Protocol);
            if (RetCode != Card.SCardSSuccess)
            {
                ConnActive = false;
                return false;
            }
            else
                return true;
        }
        public void Disconnect()
        {
            if (ConnActive)
            {
                RetCode = Card.SCardDisconnect(HCard, Card.SCardUnpowerCard);
            }
            //retCode = Card.SCardReleaseContext(hCard);
        }
        public string Transmit(byte[] buff)
        {
            string tmpStr = "";
            int indx;

            ClearBuffers();

            for (int i = 0; i < buff.Length; i++)
            {
                SendBuff[i] = buff[i];
            }
            SendLen = 5;
            RecvLen = SendBuff[SendBuff.Length - 1] + 2;

            RetCode = SendApdUandDisplay(2);


            // Display data in text format
            for (indx = 0; indx <= RecvLen - 1; indx++)
            {
                tmpStr = tmpStr + Convert.ToChar(RecvBuff[indx]);
            }

            return tmpStr;
        }
        public void Watch()
        {
            this._rdrState = new Card.SCardReaderState();
            _readername = GetReadersList()[0];
            this._rdrState.RdrName = _readername;

            _states = new Card.SCardReaderState[1];
            _states[0] = new Card.SCardReaderState();
            _states[0].RdrName = _readername;
            _states[0].UserData = 0;
            _states[0].RdrCurrState = Card.SCardStateEmpty;
            _states[0].RdrEventState = 0;
            _states[0].ATRLength = 0;
            _states[0].ATRValue = null;
            this._worker = new BackgroundWorker();
            this._worker.WorkerSupportsCancellation = true;
            this._worker.DoWork += WaitChangeStatus;
            this._worker.RunWorkerAsync();
        }
        public NfcReader()
        {
        }
    }
}
