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
    public class AjoutCompteBox : Dialog
    {
        public ResponseType rResponse;

        [UI] public Entry txtSoldeInitial = null;
        [UI] public Entry txtNomCompte = null;
        [UI] private Button btnOk = null;
        [UI] private Button btnCancel = null;
        [UI] private Entry txtInfo = null;

        public AjoutCompteBox(Window ParentWindow, string strTitre) : this(new Builder("AjoutCompteBox.glade"))
        {
            this.TransientFor = ParentWindow;
            this.SetPosition(WindowPosition.CenterOnParent);
            this.Modal = true;
            this.Title = strTitre;
        }

        private AjoutCompteBox(Builder builder) : base(builder.GetRawOwnedObject("AjoutCompteBox"))
        {
            builder.Autoconnect(this);
            DeleteEvent += delegate { OnBtnCancelClicked(this, new EventArgs()); };//Application.Quit();};
            //
            btnOk.Clicked += OnBtnOkClicked;
            btnCancel.Clicked += OnBtnCancelClicked;
            txtSoldeInitial.Changed += OnTxtSoldeInitialChanged;
            txtSoldeInitial.FocusOutEvent += OnTxtSoldeInitialFocusOut;
            //
            txtInfo.ModifyBg(StateType.Normal, new Gdk.Color(220,220,220));
        }

        void OnBtnOkClicked(object sender, EventArgs e)
		{
			if (txtNomCompte.Text == string.Empty)
			{
				Global.ShowMessage("Information manquante:", "Vous devez indiquer le nom du compte", this);
				return;
			}
            else
            {
                // on vérifie que le compte n'existe pas déjà
                string strNom = txtNomCompte.Text.Trim().Replace(' ','_');
                if (Global.IsCompteExistant(strNom) == true)
                {
                    Global.ShowMessage("Doublon:", "Ce nom de compte est déjà utilisé", this);
				    return;
                }
            }
			txtSoldeInitial.Text = Global.GetValueOrZero(txtInfo, txtSoldeInitial, false).ToString();
			rResponse = ResponseType.Ok;
			this.Dispose();
		}
        void OnBtnCancelClicked(object sender, EventArgs e)
		{
			rResponse = ResponseType.Cancel;
			this.Dispose();
		}

        public ResponseType ShowD()
        {
            this.Run();
            return rResponse;
        }

        // Controle du texte entré dans les zones de textes.
        private void OnTxtSoldeInitialChanged(object sender, EventArgs a)
        {
            bool bRes;
			
			txtSoldeInitial.Changed -= OnTxtSoldeInitialChanged;
			bRes = Global.CheckValeurs(txtInfo, sender);
			txtSoldeInitial.Changed += OnTxtSoldeInitialChanged;
        }
        // Interception perte de focus afin d'y laisser une valeur ou 0.
        private void OnTxtSoldeInitialFocusOut(object sender, EventArgs a)
        {
            txtSoldeInitial.FocusOutEvent -= OnTxtSoldeInitialFocusOut;
			txtSoldeInitial.Text = Global.GetValueOrZero(txtInfo, sender, true).ToString();
			txtSoldeInitial.FocusOutEvent += OnTxtSoldeInitialFocusOut;
        }
    }
}