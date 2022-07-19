﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ZeroKey.Service.Common;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;

namespace CredentialRegistration
{
    public partial class FrmEventRegistration : Form
    {
        List<PluginInfo> _plugins = new List<PluginInfo>();
        TcpClient _client;
        Dictionary<string, string> _tokens = new Dictionary<string, string>();
        System.Threading.Timer _t; // progress bar timer
        string _actualPassword = "";

        public FrmEventRegistration(ref TcpClient client, Dictionary<string, string> tokens, List<PluginInfo> plugins)
        {
            InitializeComponent();
            this._plugins = plugins;
            this._client = client;
            foreach(KeyValuePair<string, string> token in tokens)
            {
                cboTokens.Items.Add(token.Value + " - " + token.Key);
            }
            if(tokens.Count == 1)
            {
                cboTokens.SelectedIndex = 0;
            }
            foreach(PluginInfo pi in plugins)
            {
                cboPlugins.Items.Add(pi.Name);
            }
            if(plugins.Count == 1)
            {
                cboPlugins.SelectedItem = 0;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cboPlugins_SelectedValueChanged(object sender, EventArgs e)
        {
            dgvParameters.Rows.Clear();
            foreach (PluginInfo pi in _plugins)
            {
                if (pi.Name == (string)cboPlugins.SelectedItem)
                {
                    foreach (Parameter p in pi.Parameters)
                    {
                        int row = dgvParameters.Rows.Add(new object[] { p.Name, p.IsOptional, p.Default });
                        dgvParameters[0, row].ReadOnly = true;
                        dgvParameters[1, row].ReadOnly = true;
                    }
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            NetworkMessage nm = new NetworkMessage(MessageType.AssociatePluginToToken);
            if (cboTokens.SelectedItem.ToString() == "")
            {
                MessageBox.Show("Select a token to register");
                return;
            }
            else
            {
                nm.Token = cboTokens.SelectedItem.ToString().Split('-')[1].Trim();
            }
            if (cboPlugins.SelectedItem.ToString() == "")
            {
                MessageBox.Show("Select a plugin to register an event for.");
                return;
            }
            else
            {
                nm.PluginName = cboPlugins.SelectedItem.ToString();
            }
            foreach (DataGridViewRow dgvr in dgvParameters.Rows)
            {
                if((bool)dgvr.Cells["dgcIsOptional"].Value == false)
                {
                    if(dgvr.Cells["dgcValue"].Value.ToString() == "")
                    {
                        MessageBox.Show("Fill in the required paramters or go home");
                        return;
                    }
                    else
                    {
                        if(dgvr.Cells["dgcName"].Value.ToString() == "Username")
                        {
                            nm.Username = dgvr.Cells["dgcValue"].Value.ToString(); // this should be a list instead of specific properties
                        }
                        else if (dgvr.Cells["dgcName"].Value.ToString() == "Password")
                        {
                            nm.Password = _actualPassword;//dgvr.Cells["dgcValue"].Value.ToString(); // make this a single-hashed version of the token
                        }
                    }
                }
            }
            if (!string.IsNullOrEmpty(nm.Password))
            {
                lblSwipeEncrypt.Visible = true;
                pgbAwaitingToken.Visible = true;
                // need to do a progress bar and ask for a ring to encrypt credential
                if (ServiceCommunication.SendNetworkMessage(ref _client, JsonConvert.SerializeObject(new NetworkMessage(MessageType.GetToken))) > 0)
                {
                    // swipe ring to encrypt
                    int value = 0;
                    var task = Task<string>.Factory.StartNew(() => { return ServiceCommunication.ReadNetworkMessage(ref _client); });
                    _t = new System.Threading.Timer((o) =>
                    {
                        Task<string> tsk = (Task<string>)o;
                        if (tsk.IsCompleted)
                        {
                            _t.Change(Timeout.Infinite, Timeout.Infinite); // stop the timer
                            ClientCommon.SetControlPropertyThreadSafe(pgbAwaitingToken, "Visible", false);
                            ClientCommon.SetControlPropertyThreadSafe(lblSwipeEncrypt, "Visible", false);
                            if (tsk.Result != "")
                            {
                                string rawToken = JsonConvert.DeserializeObject<NetworkMessage>(tsk.Result).Token;
                                nm.Password = ZeroKey.Service.Common.Crypto.Encrypt(nm.Password, rawToken);
                                if (ServiceCommunication.SendNetworkMessage(ref _client, JsonConvert.SerializeObject(nm)) > 0)
                                    Invoke(new Action(Close));
                            }
                            else
                            {
                                // failed to scan a ring. 
                                // do we want to allow unencrypted passwords?
                            }
                        }
                        else
                        {
                            // still waiting for it to be scanned
                            value += 7;
                            ClientCommon.SetControlPropertyThreadSafe(pgbAwaitingToken, "Value", value);
                        }
                    }, task, 1000, 1000);
                    pgbAwaitingToken.Visible = true;
                }
            }
            else
            {
                // This event doesnt have a password field
                if (ServiceCommunication.SendNetworkMessage(ref _client, JsonConvert.SerializeObject(nm)) > 0)
                    Invoke(new Action(Close));
            }
        }

        private void dgvParameters_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (dgvParameters.SelectedCells == null || dgvParameters.SelectedCells.Count == 0)
                return;
            if (dgvParameters.SelectedCells[0].ColumnIndex < 0 || dgvParameters.SelectedCells[0].RowIndex < 0)
                return;
            if (dgvParameters[0, dgvParameters.SelectedCells[0].RowIndex].Value.ToString() == "Password")
            {
                if (e.KeyChar == (char)Keys.Back)
                {
                    if (_actualPassword.Length > 0)
                    {
                        _actualPassword = _actualPassword.Substring(0, _actualPassword.Length - 1);
                    }
                }
                else
                {
                    _actualPassword += e.KeyChar;
                }
                string mask = "";
                for (int i = 0; i < _actualPassword.Length; i++)
                {
                    mask += "*";
                }
                (sender as DataGridViewTextBoxEditingControl).Text = mask;
                dgvParameters.SelectedCells[0].Value = mask;
                (sender as DataGridViewTextBoxEditingControl).SelectionStart = _actualPassword.Length;
                (sender as DataGridViewTextBoxEditingControl).SelectionLength = 0;
                e.Handled = true;
            }
        }

        private void dgvParameters_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            //DataGridViewTextBoxEditingControl tb = (DataGridViewTextBoxEditingControl)e.Control;
            //tb.KeyPress += new KeyPressEventHandler(dgvParameters_KeyPress);
            e.Control.KeyPress -= new KeyPressEventHandler(dgvParameters_KeyPress);
            e.Control.KeyPress += new KeyPressEventHandler(dgvParameters_KeyPress);
        }
    }
}
