/*  Copyright (c) Raphael Borrelli (@Rafbor)

    Ce fichier fait partie de RafCompta.

    RafCompta est un logiciel libre: vous pouvez le redistribuer ou le modifier
    suivant les termes de la GNU General Public License telle que publiée par
    la Free Software Foundation, soit la version 3 de la licence, soit
    (à votre gré) toute version ultérieure.

    RafCompta est distribué dans l'espoir qu'il sera utile,
    mais SANS AUCUNE GARANTIE; sans même la garantie tacite de
    QUALITÉ MARCHANDE ou d'ADÉQUATION à UN BUT PARTICULIER. Consultez la
    GNU General Public License pour plus de détails.

    Vous devez avoir reçu une copie de la GNU General Public License
    en même temps que RafCompta. Si ce n'est pas le cas, consultez <https://www.gnu.org/licenses>.

    -----------------------------------------------------------------

    This file is part of RafCompta.

    RafCompta is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    RafCompta is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with RafCompta.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace RafCompta
{
    public class CalendarBox : Window
    {
        public DateTime dtDatePicked;

        public CalendarBox(Window ParentWindow) : 
                base(Gtk.WindowType.Popup)
        {
            this.TransientFor = ParentWindow;
            this.SetPosition(WindowPosition.CenterOnParent);
            this.Modal = true;
            //
            HBox box = new HBox();
            Gtk.Calendar calendar = new Gtk.Calendar();
            box.PackStart(calendar, true, true, 1);
            this.Add(box);
            //
            calendar.DaySelectedDoubleClick += OnCalendarDaySelectedDoubleClick;

            this.ShowAll();
        }

        protected void OnCalendarDaySelectedDoubleClick (object sender, EventArgs e)
        {
            var datePicker = (Gtk.Calendar)sender;
            dtDatePicked = datePicker.Date;
            this.Dispose();
        }
    }
    public class OperationBox : Dialog
    {
        public ResponseType rResponse;
        private bool bRecurrente;
        private bool bOpeRecurMod;
        private bool bDateRecurMod;
        private Int16 nKeyRecur;
        private CalendarBox datePicker;
        private Window pParentWindow=null;

        [UI] public Entry txtDate = null;
        [UI] public Entry txtOperation = null;
        [UI] public Entry txtCredit = null;
        [UI] public Entry txtDebit = null;
        [UI] public ComboBoxText cbModePaiement = null;
        [UI] private Button btnCalendar = null;
        [UI] private Entry txtInfo = null;
        [UI] private Button btnOk = null;
        [UI] private Button btnCancel = null;
        [UI] private CheckButton chkRecurrente = null;

        public bool Recurrente { get => bRecurrente; set => bRecurrente = value; }
        public Int16 KeyRecur { get => nKeyRecur; set => nKeyRecur = value; }
        public bool OpeRecurMod { get => bOpeRecurMod; set => bOpeRecurMod = value; }
        public bool DateRecurMod { get => bDateRecurMod; set => bDateRecurMod = value; }

        public OperationBox(Window ParentWindow, string strTitre, bool bOpeRecurMod, bool bDateRecurMod) : this(new Builder("OperationBox.glade"))
        {
            pParentWindow = ParentWindow;
            this.TransientFor = pParentWindow;
            this.SetPosition(WindowPosition.CenterOnParent);
            this.Modal = true;
            this.Title = strTitre;
            OpeRecurMod = bOpeRecurMod;
            DateRecurMod = bDateRecurMod;
        }

        private OperationBox(Builder builder) : base(builder.GetRawOwnedObject("OperationBox"))
        {
            builder.Autoconnect(this);
            DeleteEvent += delegate { OnBtnCancelClicked(this, new EventArgs()); };//Application.Quit();};
            //
            btnCalendar.Clicked += OnBtnCalendarClicked;
            btnOk.Clicked += OnBtnOkClicked;
            btnCancel.Clicked += OnBtnCancelClicked;
            txtCredit.Changed += OnTxtCreditChanged;
            txtCredit.FocusOutEvent += OnTxtCreditFocusOut;
            txtDebit.Changed += OnTxtDebitChanged;
            txtDebit.FocusOutEvent += OnTxtDebitFocusOut;
            //
            chkRecurrente.Active = false;
            txtInfo.ModifyBg(StateType.Normal, new Gdk.Color(220,220,220));
        }

        // Controle du texte entré dans les zones de textes.
        private void OnTxtCreditChanged(object sender, EventArgs a)
        {
            bool bRes;
			
			txtCredit.Changed -= OnTxtCreditChanged;
			bRes = Global.CheckValeurs(txtInfo, sender);
			txtCredit.Changed += OnTxtCreditChanged;
        }
        private void OnTxtDebitChanged(object sender, EventArgs a)
        {
            bool bRes;
			
			txtDebit.Changed -= OnTxtDebitChanged;
			bRes = Global.CheckValeurs(txtInfo, sender);
			txtDebit.Changed += OnTxtDebitChanged;
        }
        // Interception perte de focus afin d'y laisser une valeur ou 0.
        private void OnTxtCreditFocusOut(object sender, EventArgs a)
        {
            txtCredit.FocusOutEvent -= OnTxtCreditFocusOut;
			txtCredit.Text = Math.Abs(Global.GetValueOrZero(txtInfo, sender, true)).ToString();
			txtCredit.FocusOutEvent += OnTxtCreditFocusOut;
        }
        private void OnTxtDebitFocusOut(object sender, EventArgs a)
        {
            txtDebit.FocusOutEvent -= OnTxtDebitFocusOut;
			txtDebit.Text = Math.Abs(Global.GetValueOrZero(txtInfo, sender, true)).ToString();
			txtDebit.FocusOutEvent += OnTxtDebitFocusOut;
        }

        private void OnBtnCalendarClicked(object sender, EventArgs a)
        {
            Global.AfficheInfo(txtInfo, "Double-clic pour sélectionner une date", new Gdk.Color(0,0,255));
            datePicker = new CalendarBox(pParentWindow);
            datePicker.Destroyed += OnDatePickerDestroyed;
            datePicker.ShowAll();
        }

        // Fermeture de la boite dialogue affichant le controle Calendar.
        private void OnDatePickerDestroyed(object sender, EventArgs e)
        { 
            txtDate.Text = datePicker.dtDatePicked.ToShortDateString();
            txtInfo.Text = string.Empty;
        }

        void OnBtnOkClicked(object sender, EventArgs e)
		{
            // crédit ou débit mais pas les deux
            if (txtDebit.Text != "0" && txtCredit.Text != "0")
            {
                Global.AfficheInfo(txtInfo, "Impossible de saisir simultanément du crédit et du débit", new Gdk.Color(255,0,0));
                return;
            }
            //
            Recurrente = chkRecurrente.Active;
			rResponse = ResponseType.Ok;
			this.Dispose();
		}
        void OnBtnCancelClicked(object sender, EventArgs e)
		{
			rResponse = ResponseType.Cancel;
			this.Dispose();
		}
        
        public ResponseType ShowD(string strOperation, string strModePaiement, double dblCredit, double dblDebit, DateTime dtmDate, bool bRecurrente)
        {
            txtOperation.Text = strOperation;
            txtDate.Text = dtmDate.ToShortDateString();
            txtCredit.Text = dblCredit.ToString();
            txtDebit.Text = dblDebit.ToString();
            chkRecurrente.Active = bRecurrente;
            chkRecurrente.Sensitive = OpeRecurMod;
            // modif date impossible
            txtDate.Sensitive = DateRecurMod;
            btnCalendar.Sensitive = DateRecurMod;
            for (int i = 0; i < Global.arListItem.Count; i++)
            {
                if (Global.arListItem[i].Equals(strModePaiement))
                {
                    cbModePaiement.Active = i;
                    break;
                }
            }
            this.Run();
            return rResponse;
        }
    }
}