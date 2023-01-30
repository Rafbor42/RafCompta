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
using System.Data;
using System.IO;

namespace RafCompta
{
    public class ArchivesBox : Dialog
    {
        // private TreeModelFilter filter; // ne fonctionne pas...
        private Datas mdatas;
        [UI] private TreeView trvOperations = null;
        [UI] private Entry txtInfo = null;
        [UI] private Entry txtOperation = null;
        [UI] private Entry txtModePaiement = null;
        [UI] private Entry txtDate = null;
        [UI] private Entry txtCredit = null;
        [UI] private Entry txtDebit = null;
        [UI] private Button btnResetFiltre = null;

        public ArchivesBox(Window ParentWindow, ref Datas datas, string strFileName) : this(new Builder("ArchivesBox.glade"))
        {
            this.TransientFor = ParentWindow;
            this.SetPosition(WindowPosition.CenterOnParent);
            this.Modal = true;
            this.Title = strFileName.Substring(0, strFileName.Length - 4);
            //
            mdatas = datas;
            InitTrvOperations();
            OuvrirFichier(strFileName);
            UpdateTrvOperations();
        }

        private ArchivesBox(Builder builder) : base(builder.GetRawOwnedObject("ArchivesBox"))
        {
            builder.Autoconnect(this);
            DeleteEvent += delegate { this.Dispose(); };
            //
            txtOperation.Changed += OnTxtOperationChanged;
            txtModePaiement.Changed += OnTxtModePaiementChanged;
            txtDate.Changed += OnTxtDateChanged;
            txtCredit.Changed += OnTxtCreditChanged;
            txtDebit.Changed += OnTxtDebitChanged;
            btnResetFiltre.Clicked += OnBtnResetFiltreClicked;
            //
            txtInfo.ModifyBg(StateType.Normal, new Gdk.Color(220,220,220));
        }

        private void OnBtnResetFiltreClicked(object sender, EventArgs a)
        {
            txtOperation.Changed -= OnTxtOperationChanged;
			txtOperation.Text = string.Empty;
			txtOperation.Changed += OnTxtOperationChanged;
			//
			txtCredit.Changed -= OnTxtCreditChanged;
			txtCredit.Text = string.Empty;
			txtCredit.Changed += OnTxtCreditChanged;
			//
			txtDebit.Changed -= OnTxtDebitChanged;
			txtDebit.Text = string.Empty;
			txtDebit.Changed += OnTxtDebitChanged;
			//
			txtDate.Changed -= OnTxtDateChanged;
			txtDate.Text = string.Empty;
			txtDate.Changed += OnTxtDateChanged;
			//
			txtModePaiement.Changed -= OnTxtModePaiementChanged;
			txtModePaiement.Text = string.Empty;
			txtModePaiement.Changed += OnTxtModePaiementChanged;
			//
			DoFiltreDataTable();
        }

        private void OnTxtOperationChanged(object sender, EventArgs a)
        {
            //filter.Refilter ();
            DoFiltreDataTable();
        }

        private void OnTxtModePaiementChanged(object sender, EventArgs a)
        {
            //filter.Refilter ();
            DoFiltreDataTable();
        }

        private void OnTxtDateChanged(object sender, EventArgs a)
        {
            //filter.Refilter ();
            DoFiltreDataTable();
        }

        private void OnTxtCreditChanged(object sender, EventArgs a)
        {
            //filter.Refilter ();
            DoFiltreDataTable();
        }

        private void OnTxtDebitChanged(object sender, EventArgs a)
        {
            //filter.Refilter ();
            DoFiltreDataTable();
        }

        // Initialisation du treeview des opérations.
        private void InitTrvOperations()
        {
            TreeViewColumn colOperation = new TreeViewColumn();
            colOperation.Title = "Opération";
            TreeViewColumn colModePaiement = new TreeViewColumn();
            colModePaiement.Title = "Mode paiement";
            TreeViewColumn colDate = new TreeViewColumn();
            colDate.Title = "Date";
            TreeViewColumn colCredit = new TreeViewColumn();
            colCredit.Title = "Crédit (€)";
            TreeViewColumn colDebit = new TreeViewColumn();
            colDebit.Title = "Débit (€)";
            TreeViewColumn colSelect = new TreeViewColumn();
            colSelect.Title = "";
            //
            trvOperations.AppendColumn(colOperation);
            trvOperations.AppendColumn(colModePaiement);
            trvOperations.AppendColumn(colDate);
            trvOperations.AppendColumn(colCredit);
            trvOperations.AppendColumn(colDebit);
            //
            CellRendererText cellOperation = new CellRendererText();
            colOperation.PackStart(cellOperation, true);
            colOperation.AddAttribute(cellOperation, "text", 0);
            CellRendererText cellModePaiement = new CellRendererText();
            colModePaiement.PackStart(cellModePaiement, true);
            colModePaiement.AddAttribute(cellModePaiement, "text", 1);
            CellRendererText cellDate = new CellRendererText();
            colDate.PackStart(cellDate, true);
            colDate.AddAttribute(cellDate, "text", 2);
            CellRendererText cellCredit = new CellRendererText();
            colCredit.PackStart(cellCredit, true);
            colCredit.AddAttribute(cellCredit, "text", 3);
            CellRendererText cellDebit = new CellRendererText();
            colDebit.PackStart(cellDebit, true);
            colDebit.AddAttribute(cellDebit, "text", 4);
            //
            // ne fonctionne pas....
                // filtre
                //filter = new TreeModelFilter(mdatas.lstoreOperationsArchivees, null);
                // fonction de filtre
                //filter.VisibleFunc = new TreeModelFilterVisibleFunc(DoFilter);
                // model
                //trvOperations.Model = filter;
            //
            trvOperations.Model = mdatas.lstoreOperationsArchivees;
        }

        // Filtre les données du datatable source et les réinjecte dans lstoreOperationsArchivees.
        private void DoFiltreDataTable()
        {
            Int16 nNbParam = 0;
            string strFilter = string.Empty;
			bool bOperation = false;
			string strOperation = string.Empty;
			if (txtOperation.Text != string.Empty)
			{
				strOperation = "strOperation LIKE '%" + txtOperation.Text + "%'";
				bOperation = true;
			}
			//
			bool bModePaiement = false;
			string strModePaiement = string.Empty;
			if (txtModePaiement.Text != string.Empty)
			{
				strModePaiement = "strModePaiement LIKE '%" + txtModePaiement.Text + "%'";
				bModePaiement = true;
			}
			//
			string strDate = string.Empty;
			bool bDate = false;
			if (txtDate.Text != string.Empty && txtDate.Text.Length == 10)
			{
				strDate = "dtDate = '" + txtDate.Text + " 00:00:00'";
				bDate = true;
			}
			//
			bool bCredit = false;
			string strCredit = string.Empty;
			if (txtCredit.Text != string.Empty)
			{
				strCredit = "dblCredit = " + txtCredit.Text;
				bCredit = true;
			}
			//
			string strDebit = string.Empty;
			if (txtDebit.Text != string.Empty)
			{
				strDebit = "dblDebit = " + txtDebit.Text;
			}
			//
			if (strOperation != string.Empty)
			    strFilter = strOperation;
			if (strModePaiement != string.Empty)
			{
				if (bOperation == true)
					strFilter += " AND ";
				strFilter += strModePaiement;
			}
			if (strDate != string.Empty)
			{
				if (bOperation == true || bModePaiement == true)
					strFilter += " AND ";
				strFilter += strDate;
			}
			if (strCredit != string.Empty)
			{
				if (bOperation == true || bModePaiement == true || bDate == true)
					strFilter += " AND ";
				strFilter += strCredit;
			}
			if (strDebit != string.Empty)
			{
				if (bOperation == true || bModePaiement == true || bDate == true || bCredit == true)
					strFilter += " AND ";
				strFilter += strDebit;
			}
			if (strFilter == string.Empty)
				strFilter = "1=1";

            mdatas.lstoreOperationsArchivees.Clear();
            foreach(DataRow row in mdatas.dtTableOperationsArchivees.Select(strFilter, "dtDate ASC"))
			{
                mdatas.lstoreOperationsArchivees.AppendValues
                (
                    row["strOperation"].ToString(),
                    row["strModePaiement"].ToString(),
                    Convert.ToDateTime(row["dtDate"]).ToShortDateString(),
                    row["dblCredit"].ToString(),
                    row["dblDebit"].ToString(),
                    Convert.ToInt16(row["nKey"])
                );
                nNbParam++;
            }
            Global.AfficheInfo(txtInfo, "Nombre d'opérations trouvées: " + nNbParam.ToString(), new Gdk.Color(0,0,255));
        }

        // ne fonctionne pas...
        // Filtre appelé par TreeModelFilterVisibleFunc:
        // private bool DoFilter(ITreeModel model, TreeIter iter)
        // {
        //     string strOperation = model.GetValue(iter, 0).ToString();
        //     string strModePaiement = model.GetValue(iter, 1).ToString();
        //     string strDate = model.GetValue(iter, 2).ToString();
        //     string strCredit = model.GetValue(iter, 3).ToString();
        //     string strDebit = model.GetValue(iter, 4).ToString();

        //     if (txtOperation.Text == "" && txtModePaiement.Text == "" && txtDate.Text == "" && txtCredit.Text == "" && txtDebit.Text == "")
        //         return true;
    
        //     if (    strOperation.IndexOf(txtOperation.Text) > -1
        //         ||  strModePaiement.IndexOf(txtModePaiement.Text) > -1
        //         ||  strDate.IndexOf(txtDate.Text) > -1
        //         ||  strCredit.IndexOf(txtCredit.Text) > -1
        //         ||  strDebit.IndexOf(txtDebit.Text) > -1)
        //         return true;
        //     else
        //         return false;
        // }

        // Mise à jour de la treeview des opérations.
        private void UpdateTrvOperations()
        {
            trvOperations.ShowAll();
        }

        // Ouverture du fichier d'archives du compte actif.
		void OuvrirFichier(string strFileName)
		{
			string strMsg = string.Empty;
			short nNbParam;
			
			// si fichier n'existe pas
			if (System.IO.File.Exists(System.IO.Path.Combine(Global.DossierFichiers, strFileName)) == false)
			{
				UpdateTrvOperations();
				Global.AfficheInfo(txtInfo, "Fichier d'archives inexistant. Aucune opération chargée", new Gdk.Color(0,0,255));
			}
			else if ((nNbParam = mdatas.ChargeOperationsArchivees(strFileName, ref strMsg)) > 0)
			{
				DoFiltreDataTable();
			}
			if (strMsg != string.Empty)
				Global.ShowMessage("Erreur archives:", strMsg, this);
		}
    }
}