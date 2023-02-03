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
    public class TransfertBox : Dialog
    {
        private Datas mdatas;
        private double dblSoldeCompteActif;
        [UI] private Entry txtCompteActif = null;
        [UI] private ComboBoxText cbListeComptes = null;
        [UI] private Entry txtOperation = null;
        [UI] private Entry txtMontant = null;
        [UI] private Entry txtInfo = null;
        [UI] private Button btnOk = null;
        [UI] private Button btnCancel = null;

        public TransfertBox(Window ParentWindow, ref Datas datas, double dblSolde) : this(new Builder("TransfertBox.glade"))
        {
            this.TransientFor = ParentWindow;
            this.SetPosition(WindowPosition.CenterOnParent);
            this.Modal = true;
            this.Title = "Transfert";
            //
            mdatas = datas;
            dblSoldeCompteActif = dblSolde;
            UpdateCbListeComptes();
            txtCompteActif.Text = Global.NomCompteCourant;
            txtMontant.Text = "0";
        }

        private TransfertBox(Builder builder) : base(builder.GetRawOwnedObject("TransfertBox"))
        {
            builder.Autoconnect(this);
            DeleteEvent += delegate { OnBtnCancelClicked(this, new EventArgs()); };
            btnOk.Clicked += OnBtnOkClicked;
            btnCancel.Clicked += OnBtnCancelClicked;
            txtMontant.Changed += OnTxtMontantChanged;
            txtMontant.FocusOutEvent += OnTxtMontantFocusOut;
            //
            txtCompteActif.ModifyBg(StateType.Normal, new Gdk.Color(220,220,220));
            txtInfo.ModifyBg(StateType.Normal, new Gdk.Color(220,220,220));
        }

        void OnBtnOkClicked(object sender, EventArgs e)
		{
            string strCompteDest;
            double dblVal;

            strCompteDest = cbListeComptes.ActiveText;
            if (strCompteDest == Global.NomCompteCourant)
            {
                Global.AfficheInfo(txtInfo, "Le compte actif ne peut pas être le destinataire", new Gdk.Color(255,0,0));
                return;
            }
            if (cbListeComptes.Active == -1)
            {
                Global.AfficheInfo(txtInfo, "Veuillez sélectionner un compte destinataire", new Gdk.Color(255,0,0));
                return;
            }
            if (txtOperation.Text == string.Empty)
            {
                Global.AfficheInfo(txtInfo, "Veuillez renseigner un libellé d'opération", new Gdk.Color(255,0,0));
                return;
            }
            if (Convert.ToDouble(txtMontant.Text) == 0)
            {
                Global.AfficheInfo(txtInfo, "Le montant doit être > 0", new Gdk.Color(255,0,0));
                return;
            }
            else
            {
                dblVal = Convert.ToDouble(txtMontant.Text);
                if (dblVal > dblSoldeCompteActif)
                {
                    if (Global.Confirmation("Solde négatif:", "Attention, le solde du compte actif sera négatif. Continuer ?", this) == false)
                    {
                        txtInfo.Text = string.Empty;
                        return;
                    }
                }
            }
            //
            mdatas.DoTransfert(txtOperation.Text, Convert.ToDouble(txtMontant.Text), strCompteDest);
            this.Dispose();
		}
        void OnBtnCancelClicked(object sender, EventArgs e)
		{
			this.Dispose();
		}

        // Controle du texte entré dans les zones de textes.
        private void OnTxtMontantChanged(object sender, EventArgs a)
        {
            bool bRes;
			
			txtMontant.Changed -= OnTxtMontantChanged;
			bRes = Global.CheckValeurs(txtInfo, sender);
			txtMontant.Changed += OnTxtMontantChanged;
        }

        // Interception perte de focus afin d'y laisser une valeur ou 0.
        private void OnTxtMontantFocusOut(object sender, EventArgs a)
        {
            txtInfo.Text = string.Empty;
            txtMontant.FocusOutEvent -= OnTxtMontantFocusOut;
			txtMontant.Text = Math.Abs(Global.GetValueOrZero(txtInfo, sender, true)).ToString();
			txtMontant.FocusOutEvent += OnTxtMontantFocusOut;
        }

        private void UpdateCbListeComptes()
        {
            Global.sCompte compte;
			string strItem = string.Empty;

            // clear
            ListStore ClearList = new ListStore(typeof(string));
            cbListeComptes.Model = ClearList;
            // remplissage
            for (int n = 0; n < Global.arComptes.Count; n++)
            {
                compte = (Global.sCompte)Global.arComptes[n];
                cbListeComptes.AppendText(compte.strNomCompte);
            }
        }
    }
}