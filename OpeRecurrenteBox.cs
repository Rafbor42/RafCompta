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
    public class OpeRecurrenteBox : Dialog
    {
        public ResponseType rResponse;
        private Datas mdatas;
        [UI] private Entry txtCompteCourant = null;
        [UI] private Entry txtInfo = null;
        [UI] private TreeView trvOperations = null;
        [UI] private Button btnAjouter = null;
        [UI] private Button btnSupprimer = null;
        [UI] private Button btnModifier = null;

        public OpeRecurrenteBox(Window ParentWindow, ref Datas datas) : this(new Builder("OpeRecurrenteBox.glade"))
        {
            this.TransientFor = ParentWindow;
            this.SetPosition(WindowPosition.CenterOnParent);
            this.Modal = true;
            //
            mdatas = datas;
            InitTrvOperations();
            txtCompteCourant.Text = Global.NomCompteCourant;
            //
            btnAjouter.Visible = false;
            trvOperations.GrabFocus();
            UpdateTrvOperations();
        }

        private OpeRecurrenteBox(Builder builder) : base(builder.GetRawOwnedObject("OpeRecurrenteBox"))
        {
            builder.Autoconnect(this);
            DeleteEvent += delegate { this.Dispose(); };
            //
            btnAjouter.Clicked += OnBtnAjouterClicked;
            btnModifier.Clicked += OnBtnModifierClicked;
            btnSupprimer.Clicked += OnBtnSupprimerClicked;
            //
            txtCompteCourant.ModifyBg(StateType.Normal, new Gdk.Color(220,220,220));
            txtInfo.ModifyBg(StateType.Normal, new Gdk.Color(220,220,220));
        }

        // Ajout des lignes sélectionnées au compte courant.
        private void OnBtnAjouterClicked(object sender, EventArgs e)
        {
            TreeIter iter;
            Int16 nLignesAjoutees = 0, nLignesIgnorees = 0;
            Int16 nKey;
            // parcours des lignes cochées
            if (mdatas.lstoreOperationsRecur.GetIterFirst(out iter) == true)
            {
                do
                {
                    // si coché
                    if (Convert.ToBoolean(mdatas.lstoreOperationsRecur.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Select))) == true)
                    {
                        nKey = Convert.ToInt16(mdatas.lstoreOperationsRecur.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Key)));
                        // si opération existe déjà sur le compte courant, on l'ignore
                        if (mdatas.IsOperationExistante(nKey) == true)
                            nLignesIgnorees++;
                        else
                        {
                            mdatas.AjouteOpeRecurSurCompteCourant(iter, nKey);
                            nLignesAjoutees++;
                        }
                    }
                }
                while (mdatas.lstoreOperationsRecur.IterNext(ref iter) == true);
            }
			Global.ShowMessage("Ajout d'opérations sur Compte courant:", nLignesAjoutees + " ajoutée(s)\n" + nLignesIgnorees + " ignorée(s) car déjà existante(s)", this, MessageType.Info);
			UpdateTrvOperations();
        }
        
        private void OnBtnModifierClicked(object sender, EventArgs e)
        {
            txtInfo.Text = string.Empty;
            TreeIter iter;
            TreePath chemin;
            TreePath[] chemins;
            chemins = trvOperations.Selection.GetSelectedRows();
            // si aucune ligne sélectionnée
            if (chemins.Length == 0)
            {
                Global.AfficheInfo(txtInfo, "Vous devez sélectionner une ligne à modifier", new Gdk.Color(255,0,0));
                return;
            }

            chemin = chemins[0];
            if (mdatas.lstoreOperationsRecur.GetIter(out iter, chemin))
            {
                mdatas.ModifieOpeRecurrente(iter);
                UpdateTrvOperations();
                Global.ListeComptesModified = true;
            }
        }

        private void OnBtnSupprimerClicked(object sender, EventArgs e)
        {
            txtInfo.Text = string.Empty;
            TreeIter iter;
            TreePath chemin;
            TreePath[] chemins;
            chemins = trvOperations.Selection.GetSelectedRows();
            // si aucune ligne sélectionnée
            if (chemins.Length == 0)
            {
                Global.AfficheInfo(txtInfo, "Vous devez sélectionner une ligne à supprimer", new Gdk.Color(255,0,0));
                return;
            }
            if (Global.Confirmation("Suppression:", "Voulez-vous vraiment supprimer la ligne sélectionnée ?", this) == false)
				return;

            chemin = chemins[0];
            if (mdatas.lstoreOperationsRecur.GetIter(out iter, chemin))
            {
                mdatas.SupprimeOpeRecurrente(iter);
                UpdateTrvOperations();
                Global.ListeComptesModified = true;
            }
        }

        private void UpdateTrvOperations()
        {
            trvOperations.ShowAll();
            Global.AfficheInfo(txtInfo, "Cocher les lignes à ajouter au compte courant", new Gdk.Color(0,0,255));
        }

        // Initialisation du treeview des opérations.
        private void InitTrvOperations()
        {
            // model
            trvOperations.Model = mdatas.lstoreOperationsRecur;
            //
            TreeViewColumn colOperation = new TreeViewColumn();
            colOperation.Title = "Opération";
            TreeViewColumn colModePaiement = new TreeViewColumn();
            colModePaiement.Title = "Mode paiement";
            TreeViewColumn colDate = new TreeViewColumn();
            colDate.Title = "Jour";
            TreeViewColumn colCredit = new TreeViewColumn();
            colCredit.Title = "Crédit (€)";
            TreeViewColumn colDebit = new TreeViewColumn();
            colDebit.Title = "Débit (€)";
            TreeViewColumn colSelect = new TreeViewColumn();
            colSelect.Title = "";
            //
            trvOperations.AppendColumn(colSelect);
            trvOperations.AppendColumn(colOperation);
            trvOperations.AppendColumn(colModePaiement);
            trvOperations.AppendColumn(colDate);
            trvOperations.AppendColumn(colCredit);
            trvOperations.AppendColumn(colDebit);
            //
            CellRendererToggle cellSelect = new CellRendererToggle();
            cellSelect.Activatable = true;
            cellSelect.Toggled += delegate(object o, ToggledArgs args)
            {
				TreeIter iter;
				if (mdatas.lstoreOperationsRecur.GetIter(out iter, new TreePath (args.Path)))
					mdatas.lstoreOperationsRecur.SetValue(iter, 0, !(bool)mdatas.lstoreOperationsRecur.GetValue(iter, 0));
                UpdateData();
			};
            colSelect.PackStart(cellSelect, true);
            colSelect.AddAttribute(cellSelect, "active", Convert.ToInt16(Global.eTrvOperationsCols.Select));
            CellRendererText cellOperation = new CellRendererText();
            colOperation.PackStart(cellOperation, true);
            colOperation.AddAttribute(cellOperation, "text", Convert.ToInt16(Global.eTrvOperationsCols.Operation));
            CellRendererText cellModePaiement = new CellRendererText();
            colModePaiement.PackStart(cellModePaiement, true);
            colModePaiement.AddAttribute(cellModePaiement, "text", Convert.ToInt16(Global.eTrvOperationsCols.ModePaiement));
            CellRendererText cellDate = new CellRendererText();
            colDate.PackStart(cellDate, true);
            colDate.AddAttribute(cellDate, "text", Convert.ToInt16(Global.eTrvOperationsCols.Date));
            CellRendererText cellCredit = new CellRendererText();
            colCredit.PackStart(cellCredit, true);
            colCredit.AddAttribute(cellCredit, "text", Convert.ToInt16(Global.eTrvOperationsCols.Credit));
            CellRendererText cellDebit = new CellRendererText();
            colDebit.PackStart(cellDebit, true);
            colDebit.AddAttribute(cellDebit, "text", Convert.ToInt16(Global.eTrvOperationsCols.Debit));
        }

        private void UpdateData()
        {
            bool bCoche = false;
            TreeIter iter;
            if (mdatas.lstoreOperationsRecur.GetIterFirst(out iter) == true)
            {
                do
                {
                    // si coché
                    if (Convert.ToBoolean(mdatas.lstoreOperationsRecur.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Select))) == true)
                        bCoche = true;
                }
                while (mdatas.lstoreOperationsRecur.IterNext(ref iter) == true);
            }
            if (bCoche == true)
                btnAjouter.Visible = true;
            else
                btnAjouter.Visible = false;
        }
    }
}