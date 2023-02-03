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
using System.Threading;
using System.Globalization;
using System.IO;

namespace RafCompta
{
    class MainWindow : Window
    {
        Datas datas;

        private AjoutCompteBox ajoutCompteBox;
        private string strNomCompte;
        private double dblSoldeInitial; 
        private string strMsg = string.Empty;
        //
        // controles
        // [UI] private MenuItem mnuFichierOuvrir = null;
        [UI] private MenuItem mnuFichierEnregistrer = null;
        [UI] private MenuItem mnuFichierConsultArchive = null;
        [UI] private MenuItem mnuFichierQuitter = null;
        [UI] private MenuItem mnuActionsAjouterCompte = null;
        [UI] private MenuItem mnuActionSupprimerCompte = null;
        [UI] private MenuItem mnuActionAjouterOperation = null;
        [UI] private MenuItem mnuActionSupprimerOperation = null;
        [UI] private MenuItem mnuActionModifierOperation = null;
        [UI] private MenuItem mnuActionOpeRecurrentes = null;
        [UI] private MenuItem mnuActionTransfert = null;
        [UI] private MenuItem mnuPointAPropos = null;
        [UI] private MenuItem mnuPointAide = null;
        //
        [UI] private ComboBoxText cbListeComptes = null;
        [UI] private TreeView trvOperations = null;
        [UI] private Entry txtSolde = null;
        [UI] private Entry txtSoldeBanque = null;
        [UI] private Entry txtEcart = null;
        [UI] private Entry txtInfo = null;
        [UI] private Entry txtCredits = null;
        [UI] private Entry txtDebits = null;
        [UI] private Button btnAjouter = null;
        [UI] private Button btnSupprimer = null;
        [UI] private Button btnModifier = null;
        [UI] private Button btnRapprocher = null;
        // [UI] private Button btnOuvrir = null;
        [UI] private Button btnEnregistrer = null;
        [UI] private Button btnConsulterArchives = null;
        [UI] private Button btnOpeRecurrentes = null;
        [UI] private Button btnTransfert = null;
        [UI] private Label lblEcart = null;
        [UI] private Label lblCredits = null;
        [UI] private Label lblDebits = null;
        //
        [UI] private Entry txtSoldeInitial = null;
        [UI] private Entry txtNomCompte = null;
        [UI] private Entry txtNomFichierCompte = null;
        [UI] private Entry txtNomFichierArchives = null;
        [UI] private CheckButton chkChargerFichierAuto = null;
        [UI] private CheckButton chkSauverFichierAuto = null;
        [UI] private CheckButton chkArchiverLigneRappro = null;

        public MainWindow() : this(new Builder("MainWindow.glade"))
        {
            this.SetPosition(WindowPosition.Center);
            // timer pour afficher les messages d'erreurs au chargement
            // après l'affichage de la fenêtre principale
            GLib.Timeout.Add(1000, new GLib.TimeoutHandler(Update_status));
        }

        private bool Update_status()
        {
            if (strMsg != string.Empty)
            {
                Global.ShowMessage("Erreurs au chargement", strMsg, this);
            }
            else if (Global.FichierCompteNonTrouve == true)
            {
                // on lance la création d'un compte
                OnMnuActionsAjouterCompte(new Object(), new EventArgs());
                // si aucun compte créé
                if (Global.ListeComptesModified == false)
                    Global.AfficheInfo(txtInfo, "Créez un nouveau compte", new Gdk.Color(0,0,255));
            }
            // returning false would terminate the timeout.
            return false;
        }

        private MainWindow(Builder builder) : base(builder.GetRawOwnedObject("MainWindow"))
        {
            // redéfinition du symbole décimal
			// CultureInfo ci = (CultureInfo)Thread.CurrentThread.CurrentCulture.Clone();
			// ci.NumberFormat.NumberDecimalSeparator = ".";
			// Thread.CurrentThread.CurrentCulture = ci;
            //
            builder.Autoconnect(this);
            //
            datas = new Datas(this);
            InitTrvOperations();
            //
            DeleteEvent += delegate { OnMnuFichierQuitter(this, new EventArgs()); };
            //
            // events menus
            // mnuFichierOuvrir.Activated += OnMnuFichierOuvrir;
            mnuFichierEnregistrer.Activated += OnMnuFichierEnregistrer;
            mnuFichierConsultArchive.Activated += OnMnuFichierConsultArchive;
            mnuFichierQuitter.Activated += OnMnuFichierQuitter;
            mnuActionsAjouterCompte.Activated += OnMnuActionsAjouterCompte;
            mnuActionSupprimerCompte.Activated += OnMnuActionsSupprimerCompte;
            mnuActionAjouterOperation.Activated += OnMnuActionsAjouterOperation;
            mnuActionModifierOperation.Activated += OnMnuActionsModifierOperation;
            mnuActionSupprimerOperation.Activated += OnMnuActionsSupprimerOperation;
            mnuActionOpeRecurrentes.Activated += OnMnuActionsOpRecurrentes;
            mnuActionTransfert.Activated += OnMnuActionTransfert;
            mnuPointAPropos.Activated += OnMnuPointAPropos;
            mnuPointAide.Activated += OnMnuPointAide;
            // events combobox
            cbListeComptes.Changed += OnCbListeComptesChanged;
            // events textbox
            txtSoldeBanque.Changed += OnTxtSoldeBanqueChanged;
            txtSoldeBanque.FocusOutEvent += OnTxtSoldeBanqueFocusOut;
            txtSoldeInitial.Changed += OnTxtSoldeInitialChanged;
            txtSoldeInitial.FocusOutEvent += OnTxtSoldeInitialFocusOut;
            // events boutons
            btnAjouter.Clicked += OnBtnAjouterOperationClicked;
            btnSupprimer.Clicked += OnBtnSupprimerOperationClicked;
            btnModifier.Clicked += OnBtnModifierOperationClicked;
            btnRapprocher.Clicked += OnBtnRapprocherClick;
            // btnOuvrir.Clicked += OnBtnOuvrirClicked;
            btnEnregistrer.Clicked += OnBtnEnregistrerClicked;
            btnConsulterArchives.Clicked += OnBtnConsulterArchivesClicked;
            chkSauverFichierAuto.Clicked += OnChkSauverFichierAutoClicked;
            chkArchiverLigneRappro.Clicked += OnChkArchiverLigneRapproClicked;
            btnOpeRecurrentes.Clicked += OnBtnOpeRecurrentesClicked;
            btnTransfert.Clicked += OnBtnTransfertClicked;
            //
            txtSolde.ModifyBg(StateType.Normal, new Gdk.Color(220,220,220));
            txtEcart.ModifyBg(StateType.Normal, new Gdk.Color(220,220,220));
            txtCredits.ModifyBg(StateType.Normal, new Gdk.Color(220,220,220));
            txtDebits.ModifyBg(StateType.Normal, new Gdk.Color(220,220,220));
            txtInfo.ModifyBg(StateType.Normal, new Gdk.Color(220,220,220));
            txtNomCompte.ModifyBg(StateType.Normal, new Gdk.Color(220,220,220));
            txtNomFichierArchives.ModifyBg(StateType.Normal, new Gdk.Color(220,220,220));
            txtNomFichierCompte.ModifyBg(StateType.Normal, new Gdk.Color(220,220,220));
            chkChargerFichierAuto.Sensitive = false;
            lblEcart.Visible = false;
            txtEcart.Visible = false;
            lblCredits.Visible = false;
            txtCredits.Visible = false;
            lblDebits.Visible = false;
            txtDebits.Visible = false;
            btnRapprocher.Visible = false;
            //
            Global.LireConfigLocal(ref strMsg);
			if (strMsg != string.Empty)
                strMsg = "Lecture configuration: " + strMsg + "\n\n";
            // chargement liste comptes
            string strMsg2 = string.Empty;
            Int16 nbComptes = datas.ChargeListeComptes(ref strMsg2);
            if (strMsg2 != string.Empty)
            {
                if (strMsg != string.Empty)
                    strMsg += "\n\n";
                strMsg += "Lecture fichier comptes: " + strMsg2;
            }
			//
            UpdateCbListeComptes();
            UpdateData();
			DoCalcul(false);
			//
			Global.ConfigModified = false;
			Global.ListeComptesModified = false;
            //
            if (nbComptes == 0)
                Global.AfficheInfo(txtInfo, "Créez un nouveau compte", new Gdk.Color(0,0,255));
            else if (Global.DernierCompteActif == string.Empty)
                Global.AfficheInfo(txtInfo, "Sélectionnez un compte dans la liste déroulante, ou créez un nouveau compte", new Gdk.Color(0,0,255));
            //
            // on se positionne sur le dernier compte actif, cela entraine le chargement des données du compte
            if (Global.DernierCompteActif != string.Empty)
            {
                for (int i = 0; i < Global.arComptes.Count; i++)
                {
                    Global.sCompte sCompte = (Global.sCompte)Global.arComptes[i];
                    if (sCompte.strNomCompte.Equals(Global.DernierCompteActif))
                    {
                        cbListeComptes.Active = i;
                        break;
                    }
                }
            }
        }

        private void OnBtnTransfertClicked(object sender, EventArgs e)
        {
            OnMnuActionTransfert(sender, e);
        }

        private void OnBtnOpeRecurrentesClicked(object sender, EventArgs e)
        {
            OnMnuActionsOpRecurrentes(sender, e);
        }

        private void OnChkArchiverLigneRapproClicked(object sender, EventArgs e)
        {
            Global.ConfigModified = true;
            Global.ArchiveLigneRappro = chkArchiverLigneRappro.Active;
        }

        private void OnChkSauverFichierAutoClicked(object sender, EventArgs e)
        {
            Global.ConfigModified = true;
            Global.SauveFichierAuto = chkSauverFichierAuto.Active;
        }

        private void OnMnuActionsOpRecurrentes(object sender, EventArgs e)
        {
            if (Global.NomCompteCourant == string.Empty)
			{
				Global.AfficheInfo(txtInfo, "Impossible: veuillez d'abord sélectionner un compte", new Gdk.Color(255,0,0));
				return;
			}
            txtInfo.Text = string.Empty;
			OpeRecurrenteBox OpeRecurBox = new OpeRecurrenteBox(this, ref datas);
            OpeRecurBox.Destroyed += delegate { OnOperBoxDestroyed(); };
            OpeRecurBox.Show();
        }

        private void OnOperBoxDestroyed()
        {
            datas.DoFiltreDataTable();
			UpdateTrvOperations();
			DoCalcul();
        }

        private void OnBtnConsulterArchivesClicked(object sender, EventArgs e)
        {
            OnMnuFichierConsultArchive(sender, e);
        }

        private void OnBtnEnregistrerClicked(object sender, EventArgs e)
        {
            OnMnuFichierEnregistrer(sender, e);
        }

        // private void OnBtnOuvrirClicked(object sender, EventArgs e)
        // {
        //     OnMnuFichierOuvrir(sender, e);
        // }

        // Mise à jour des données.
        // <param name="bVersIHM"></param>
        private void UpdateData(bool bVersIHM=true)
        {
            if (bVersIHM == true)
			{
                txtSoldeInitial.Text = Global.CompteCourant.dblSoldeInitial.ToString();
				txtNomCompte.Text = Global.CompteCourant.strNomCompte;
				txtNomFichierCompte.Text = Global.CompteCourant.strNomFichier;
				txtNomFichierArchives.Text = Global.CompteCourant.strNomArchive;
				// //
				chkChargerFichierAuto.Active = Global.ChargeFichierAuto;
				chkSauverFichierAuto.Active = Global.SauveFichierAuto;
				chkArchiverLigneRappro.Active = Global.ArchiveLigneRappro;
				//
				txtSoldeBanque.Text = Global.SoldeBanque.ToString();
            }
            else
            {
                Global.CompteCourant.dblSoldeInitial = Global.GetValueOrZero(txtInfo, txtSoldeInitial, false);
				Global.SetCompteCourant();
				//
				Global.ChargeFichierAuto = chkChargerFichierAuto.Active;
				Global.SauveFichierAuto = chkSauverFichierAuto.Active;
				Global.ArchiveLigneRappro = chkArchiverLigneRappro.Active;
				//
                Global.SoldeBanque = Global.GetValueOrZero(txtInfo, txtSoldeBanque, false);
            }
        }

        // Calculs.
        private void DoCalcul(bool bAfficheMsg = true)
        {
            double dblSolde = Global.CompteCourant.dblSoldeInitial;
			double dblEcart, dblTotalARapprocher=0;
			bool bRapproche = false;
            double dblTotalCredits=0, dblTotalDebits=0;

            // solde
            TreeIter iter;
            if (datas.lstoreOperations.GetIterFirst(out iter) == true)
            {
                do
                {
                    dblSolde += Convert.ToDouble(datas.lstoreOperations.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Credit)));
                    dblSolde -= Convert.ToDouble(datas.lstoreOperations.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Debit)));
                }
                while (datas.lstoreOperations.IterNext(ref iter) == true);
            }
            txtSolde.Text = Math.Round(dblSolde, 2).ToString();
            //
            // total à rapprocher
            if (datas.lstoreOperations.GetIterFirst(out iter) == true)
            {
                do
                {
                    // si coché
                    if (Convert.ToBoolean(datas.lstoreOperations.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Select))) == true)
                    {
                        dblTotalARapprocher += Convert.ToDouble(datas.lstoreOperations.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Credit)));
                        dblTotalARapprocher -= Convert.ToDouble(datas.lstoreOperations.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Debit)));
                        bRapproche = true;
                        dblTotalCredits += Convert.ToDouble(datas.lstoreOperations.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Credit)));
                        dblTotalDebits += Convert.ToDouble(datas.lstoreOperations.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Debit)));
                    }
                }
                while (datas.lstoreOperations.IterNext(ref iter) == true);
            }
            //
            // écart
			dblEcart = Global.CompteCourant.dblSoldeInitial;
			dblEcart += dblTotalARapprocher;
			dblEcart -= Global.SoldeBanque;
			dblEcart = Math.Round(dblEcart, 2);
            txtEcart.Text = dblEcart.ToString();
            dblTotalCredits = Math.Round(dblTotalCredits, 2);
            txtCredits.Text = dblTotalCredits.ToString();
            dblTotalDebits = Math.Round(dblTotalDebits, 2);
            txtDebits.Text = dblTotalDebits.ToString();
            if (dblEcart == 0)
			{
				// si une ligne sélectionnée
				if (bRapproche == true)
				{
					lblEcart.Visible = true;
					txtEcart.Visible = true;
                    lblCredits.Visible = true;
                    txtCredits.Visible = true;
                    lblDebits.Visible = true;
                    txtDebits.Visible = true;
					btnRapprocher.Visible = true;
					if (bAfficheMsg == true)
						Global.AfficheInfo(txtInfo, "Vous pouvez effectuer un rapprochement", new Gdk.Color(0,0,255));
				}
				else if (bAfficheMsg == true)
					txtInfo.Text = string.Empty;
			}
			else
			{
				// si une ligne sélectionnée
				if (bRapproche == true)
				{
					lblEcart.Visible = true;
					txtEcart.Visible = true;
                    lblCredits.Visible = true;
                    txtCredits.Visible = true;
                    lblDebits.Visible = true;
                    txtDebits.Visible = true;
					if (bAfficheMsg == true)
						Global.AfficheInfo(txtInfo, "Vous ne pouvez pas effectuer un rapprochement", new Gdk.Color(255,0,0));
				}
				else
				{
					lblEcart.Visible = false;
					txtEcart.Visible = false;
                    lblCredits.Visible = false;
                    txtCredits.Visible = false;
                    lblDebits.Visible = false;
                    txtDebits.Visible = false;
					if (bAfficheMsg == true)
						txtInfo.Text = string.Empty;
				}
				btnRapprocher.Visible = false;
			}
        }

        // Menu 'Fichier->Consulter archive'.
        private void OnMnuFichierConsultArchive(object sender, EventArgs e)
        {
            if (Global.NomCompteCourant == string.Empty)
			{
				Global.AfficheInfo(txtInfo, "Impossible: veuillez d'abord sélectionner un compte", new Gdk.Color(255,0,0));
				return;
			}
            if (datas.dtTableArchOperations.Rows.Count > 0)
            {
                if (Global.SauveFichierAuto == true)
                    OnMnuFichierEnregistrer(sender, e);
                else
    				Global.ShowMessage("Archives:", "Attention, les derniers rapprochements n'ont pas été archivés\nEnregistrer vos données pour avoir des archives complètes", this);
            }
			//
            ListeArchivesBox listeArchivesBox = new ListeArchivesBox(this, ref datas);
            listeArchivesBox.ShowAll();
        }

        private void OnMnuPointAide(object sender, EventArgs e)
        {
            AideBox aideBox = new AideBox(this);
            aideBox.ShowAll();
        }
        private void OnMnuPointAPropos(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox(this);
            aboutBox.Show();// ShowAll affiche aussi le bouton License qui n'est pas connecté
        }

        // Bouton 'Rapprocher'.
		// <param name="sender"></param>
		// <param name="e"></param>
		void OnBtnRapprocherClick(object sender, EventArgs e)
		{			
			lblEcart.Visible = false;
			txtEcart.Visible = false;
            lblCredits.Visible = false;
            txtCredits.Visible = false;
            lblDebits.Visible = false;
            txtDebits.Visible = false;
			btnRapprocher.Visible = false;
			
            TreeIter iter;
            Int16 nLignes = 0;
            // on compte le nombre de lignes cochées
            if (datas.lstoreOperations.GetIterFirst(out iter) == true)
            {
                do
                {
                    // si coché
                    if (Convert.ToBoolean(datas.lstoreOperations.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Select))) == true)
                        nLignes++;
                }
                while (datas.lstoreOperations.IterNext(ref iter) == true);
            }
            // on supprime chaque ligne, en repositionnant l'index de l'iter au début à chaque tour
            for (Int16 n = 0; n < nLignes; n++)
            {
                if (datas.lstoreOperations.GetIterFirst(out iter) == true)
                {
                    while (Convert.ToBoolean(datas.lstoreOperations.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Select))) == false)
                        datas.lstoreOperations.IterNext(ref iter);

                    Global.CompteCourant.dblSoldeInitial += Convert.ToDouble(datas.lstoreOperations.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Credit)));
                    Global.CompteCourant.dblSoldeInitial -= Convert.ToDouble(datas.lstoreOperations.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Debit)));
                    Global.CompteCourant.dblSoldeInitial = Math.Round(Global.CompteCourant.dblSoldeInitial, 2);
                    Global.ListeComptesModified = true;
                    //
                    // suppression ligne pour archivage
                    datas.SupprimeOperation(iter);
                }
            }
			Global.AfficheInfo(txtInfo, "Rapprochement terminé", new Gdk.Color(0,0,255));
			UpdateData(true);
			UpdateTrvOperations();
			DoCalcul(false);
		}

        // Menu 'Actions>Ajouter un compte'.
		// <param name="sender"></param>
		// <param name="e"></param>
		void OnMnuActionsAjouterCompte(object sender, EventArgs e)
		{
            string strMsg = string.Empty;
            string strFileName = string.Empty;
			txtInfo.Text = string.Empty;
			ajoutCompteBox = new AjoutCompteBox(this, "Nouveau compte");
   			ajoutCompteBox.Destroyed += delegate { OnAjoutCompteBoxDestroyed(ref strNomCompte, ref dblSoldeInitial); };
			if (ajoutCompteBox.ShowD() == ResponseType.Ok)
			{
                strFileName = Global.AjouteCompte(strNomCompte, dblSoldeInitial);
                // création du fichier de compte s'il n'existe pas
			    string strPathFichier = System.IO.Path.Combine(Global.DossierFichiers, strFileName);
			    datas.EnregistrerDonneesCompteVierge(strPathFichier, ref strMsg);
                if (strMsg != string.Empty)
                    Global.ShowMessage("Erreur fichier", strMsg, this);
			 	Global.AfficheInfo(txtInfo, "Compte ajouté", new Gdk.Color(0,0,255));
			 	// update de la combobox
			 	UpdateCbListeComptes();
			 	Global.ListeComptesModified = true;
			}
		}
        // Destruction de la boite AjoutCompteBox.
		private void OnAjoutCompteBoxDestroyed(ref string strNomCompte, ref double dblSoldeInitial)
        {
            if (ajoutCompteBox.rResponse == ResponseType.Ok)
            {
                strNomCompte = ajoutCompteBox.txtNomCompte.Text;
                dblSoldeInitial = Convert.ToDouble(ajoutCompteBox.txtSoldeInitial.Text);
            }
        }

        // Menu 'Actions->Supprimer compte'.
		void OnMnuActionsSupprimerCompte(object sender, EventArgs e)
		{
            string strCompteSupprime = Global.NomCompteCourant;

			if (Global.NomCompteCourant == string.Empty)
			{
				Global.AfficheInfo(txtInfo, "Impossible: veuillez d'abord sélectionner un compte", new Gdk.Color(255,0,0));
				return;
			}
			//
            if (Global.Confirmation("Supprimer compte:", "Voulez vous vraiment supprimer le compte actif ?\n(les fichiers d'opérations seront conservés)", this) == true)
            {
                Global.SupprimeCompteCourant();
                Global.ListeComptesModified = true;
                UpdateCbListeComptes();
                Global.NomCompteCourant = Global.CompteCourant.strNomCompte;
                UpdateData();
                datas.Init();
			    DoCalcul();
                Global.AfficheInfo(txtInfo, "Compte " + strCompteSupprime + " supprimé", new Gdk.Color(0,0,255));
            }
		}

        void OnMnuActionsAjouterOperation(object sender, EventArgs e)
		{
			OnBtnAjouterOperationClicked(sender, e);
		}
		
		void OnMnuActionsSupprimerOperation(object sender, EventArgs e)
		{
			OnBtnSupprimerOperationClicked(sender, e);
		}
		
		void OnMnuActionsModifierOperation(object sender, EventArgs e)
		{
			OnBtnModifierOperationClicked(sender, e);
		}

        private void UpdateCbListeComptes()
        {
            Global.sCompte compte;
			string strItem = string.Empty;

            // on sauve l'item actif
            if (cbListeComptes.Active > -1)
            {
                strItem = cbListeComptes.ActiveText;
                Global.NomCompteCourant = strItem;
            }
            //
            cbListeComptes.Changed -= OnCbListeComptesChanged;
            // clear
            ListStore ClearList = new ListStore(typeof(string));
            cbListeComptes.Model = ClearList;
            // remplissage
            for (int n = 0; n < Global.arComptes.Count; n++)
            {
                compte = (Global.sCompte)Global.arComptes[n];
                cbListeComptes.AppendText(compte.strNomCompte);
            }
            // repositionnement sur l'item d'origine
            for (int i = 0; i < Global.arComptes.Count; i++)
            {
                Global.sCompte sCompte = (Global.sCompte)Global.arComptes[i];
            //    Console.WriteLine("compte:" + sCompte.strNomCompte);
                if (sCompte.strNomCompte.Equals(strItem))
                {
                    cbListeComptes.Active = i;
                    break;
                }
            }
            cbListeComptes.Changed += OnCbListeComptesChanged;
        }

        private void ConfirmeSiDataModifiees(string strCaption)
		{
			if (IsModified() == true)
			{
				if (Global.Confirmation(strCaption, "Voulez-vous enregistrer les modifications ?", this) == true)
				{
					SauveDonneesComptes();
					SauveDonneesOperations();
					SauveArchivesOperations();
                    SauveOperationsTransfert();
				}
			}
		}

        private void SauveOperationsTransfert()
        {
            string strMsg = string.Empty;
            datas.EnregistreOperationsTransfert(ref strMsg);
            if (strMsg != string.Empty)
				Global.ShowMessage("Erreur fichier:", strMsg, this);
        }

        // Vérifie si les données initiales ont été modifiées.
        // <returns>vrai si modifiées, faux sinon</returns>
        private bool IsModified()
		{
			if (datas.IsModified())
				return true;
			else
				return false;
		}

        void SauveArchivesOperations()
		{
			string strMsg = string.Empty;
            string strPathFichier = System.IO.Path.Combine(Global.DossierFichiers, Global.CompteCourant.strNomFichier);

			if (System.IO.File.Exists(strPathFichier) == false)
				return;
			
			datas.EnregistreOperationsArchivees(ref strMsg);
			if (strMsg != string.Empty)
				Global.ShowMessage("Erreur fichier archive:", strMsg, this);
            // update de la liste des fichiers d'archives
            datas.GetListeArchivesCompteCourant();
		}
		
		void SauveDonneesOperations()
		{
			string strMsg = string.Empty;
			
			datas.EnregistrerDonneesOperations(ref strMsg);
			if (strMsg != string.Empty)
				Global.ShowMessage("Erreur fichier compte:", strMsg, this);
		}

        private void SauveDonneesComptes()
		{
			string strMsg;
			
			strMsg = string.Empty;
			// données comptes
			datas.EnregistrerDonneesComptes(ref strMsg);
			if (strMsg != string.Empty)
				Global.ShowMessage("Erreur fichier liste:", strMsg, this);
            else
                Global.ListeComptesModified = false;
		}

        private void OnCbListeComptesChanged(object sender, EventArgs a)
        {
            string strItem = cbListeComptes.ActiveText;
			
			if (strItem != string.Empty)
			{
                if (Global.SauveFichierAuto == true)
                    OnMnuFichierEnregistrer(sender, a);
                else
				    ConfirmeSiDataModifiees("Ouvrir compte:");
				//
				Global.NomCompteCourant = strItem;
				Global.GetCompteCourant();
                Global.DernierCompteActif = strItem;
                this.Title = "RafCompta - " + strItem;
                Global.ConfigModified = true;
				UpdateData(true);
				OuvrirFichier();
				DoCalcul(false);
			}
        }

        private void OnMnuFichierQuitter(object sender, EventArgs a)
        {
            Exit();
            Application.Quit();
        }

        // Sauvegarde du fichier de config et demande confirmation pour sauver les données.
		void Exit()
		{
			string strMsg;
			
			UpdateData(false);
			//
			// check si config modifiée
			if (Global.ConfigModified == true)
			{
				strMsg = string.Empty;
				// écriture de la config locale
				Global.EcrireConfigLocal(ref strMsg);
				if (strMsg != string.Empty)
					Global.ShowMessage("Erreur écriture config:", strMsg, this);
			}
			//
			if (IsModified() == true || Global.ListeComptesModified == true)
			{
				if (	Global.SauveFichierAuto == true
				   ||	Global.Confirmation("Quitter:", "Voulez-vous enregistrer les modifications ?", this) == true
				   )
				{
					SauveDonneesComptes();
					SauveDonneesOperations();
					SauveArchivesOperations();
                    SauveOperationsTransfert();
				}
			}
		}

        // Initialisation du treeview des opérations.
        private void InitTrvOperations()
        {
            // model
            trvOperations.Model = datas.lstoreOperations;
            //
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
				if (datas.lstoreOperations.GetIter(out iter, new TreePath (args.Path)))
					datas.lstoreOperations.SetValue(iter, 0, !(bool)datas.lstoreOperations.GetValue(iter, 0));
                DoCalcul();
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

        // Mise à jour de la treeview des opérations.
        private void UpdateTrvOperations()
        {
            trvOperations.ShowAll();
        }

        // Menu 'Fichier->Ouvrir'.
		// <param name="sender"></param>
		// <param name="e"></param>
		// void OnMnuFichierOuvrir(object sender, EventArgs e)
		// {
		// 	if (Global.NomCompteCourant == string.Empty)
		// 	{
		// 		Global.AfficheInfo(txtInfo, "Impossible: veuillez d'abord sélectionner un compte actif", new Gdk.Color(255,0,0));
		// 		return;
		// 	}
		// 	txtInfo.Text = string.Empty;
		// 	ConfirmeSiDataModifiees("Ouvrir compte:");
		// 	//
		// 	OuvrirFichier();
		// }

        /// <summary>
		/// Ouverture du fichier de compte actif.
		/// </summary>
		void OuvrirFichier()
		{
			string strMsg = string.Empty;
            string strNomFichier;
			short nNbParam;
			
			datas.Init();
			//
			// si fichier n'existe pas
            strNomFichier = System.IO.Path.Combine(Global.DossierFichiers, Global.CompteCourant.strNomFichier);
			if (System.IO.File.Exists(strNomFichier) == false)
			{
				UpdateTrvOperations();
				Global.AfficheInfo(txtInfo, "Fichier de compte inexistant. Aucune opération chargée", new Gdk.Color(0,0,255));
			}
			else if ((nNbParam = datas.ChargeDonneesOperations(ref strMsg)) > 0)
			{
                datas.DoFiltreDataTable();
				UpdateTrvOperations();
    			Global.AfficheInfo(txtInfo, "Nombre d'opérations chargées: " + nNbParam.ToString(), new Gdk.Color(0,0,255));
			}
			if (strMsg != string.Empty)
				Global.ShowMessage("Erreur fichier:", strMsg, this);
		}

        // Menu 'Fichier->Enregistrer'.
		// <param name="sender"></param>
		// <param name="e"></param>
		void OnMnuFichierEnregistrer(object sender, EventArgs e)
		{
			if (Global.NomCompteCourant == string.Empty)
			{
				Global.AfficheInfo(txtInfo, "Impossible: veuillez d'abord sélectionner un compte", new Gdk.Color(255,0,0));
				return;
			}
			txtInfo.Text = string.Empty;
			UpdateData(false);
			SauveDonneesComptes();
			SauveDonneesOperations();
			SauveArchivesOperations();
            SauveOperationsTransfert();
		}

        private void OnMnuActionTransfert(object sender, EventArgs e)
        {
            if (Global.NomCompteCourant == string.Empty)
			{
				Global.AfficheInfo(txtInfo, "Impossible: veuillez d'abord sélectionner un compte", new Gdk.Color(255,0,0));
				return;
			}
            if (Global.arComptes.Count < 2)
            {
				Global.AfficheInfo(txtInfo, "Impossible: vous devez posséder au moins 2 comptes", new Gdk.Color(255,0,0));
				return;
			}
            txtInfo.Text = string.Empty;
			TransfertBox TransfBox = new TransfertBox(this, ref datas, Convert.ToDouble(txtSolde.Text));
            TransfBox.Destroyed += delegate { OnTransfertBoxDestroyed(); };
            TransfBox.ShowAll();
        }

        private void OnTransfertBoxDestroyed()
        {
            datas.DoFiltreDataTable();
			UpdateTrvOperations();
			DoCalcul();
            Global.AfficheInfo(txtInfo, "Transfert terminé", new Gdk.Color(0,0,255));
        }

        // Click sur bouton 'Ajouter opération'.
        private void OnBtnAjouterOperationClicked(object sender, EventArgs a)
        {
            if (Global.NomCompteCourant == string.Empty)
			{
				Global.AfficheInfo(txtInfo, "Impossible: veuillez d'abord sélectionner un compte", new Gdk.Color(255,0,0));
				return;
			}
			txtInfo.Text = string.Empty;
			datas.AjouteBoxOperation();
            datas.DoFiltreDataTable();
			UpdateTrvOperations();
			DoCalcul();
        }

        // Click sur bouton 'Modifier opération'.
        private void OnBtnModifierOperationClicked(object sender, EventArgs a)
        {
            if (Global.NomCompteCourant == string.Empty)
			{
				Global.AfficheInfo(txtInfo, "Impossible: veuillez d'abord sélectionner un compte", new Gdk.Color(255,0,0));
				return;
			}
            
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
            if (datas.lstoreOperations.GetIter(out iter, chemin))
            {
                datas.ModifieOperation(iter);
                datas.DoFiltreDataTable();
                UpdateTrvOperations();
                DoCalcul();
            }
        }
        private void OnBtnSupprimerOperationClicked(object sender, EventArgs a)
        {
            if (Global.NomCompteCourant == string.Empty)
			{
				Global.AfficheInfo(txtInfo, "Impossible: veuillez d'abord sélectionner un compte", new Gdk.Color(255,0,0));
				return;
			}
            
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
            if (datas.lstoreOperations.GetIter(out iter, chemin))
            {
                datas.SupprimeOperationNR(iter);
                UpdateTrvOperations();
                DoCalcul();
            }
        }

        // Controle du texte entré dans les zones de textes.
        private void OnTxtSoldeBanqueChanged(object sender, EventArgs a)
        {
            bool bRes;
			
			txtSoldeBanque.Changed -= OnTxtSoldeBanqueChanged;
			bRes = Global.CheckValeurs(txtInfo, sender);
			txtSoldeBanque.Changed += OnTxtSoldeBanqueChanged;
        }
        private void OnTxtSoldeInitialChanged(object sender, EventArgs a)
        {
            bool bRes;
			
			txtSoldeInitial.Changed -= OnTxtSoldeInitialChanged;
			bRes = Global.CheckValeurs(txtInfo, sender);
			txtSoldeInitial.Changed += OnTxtSoldeInitialChanged;
        }
        // Interception perte de focus afin d'y laisser une valeur ou 0.
        private void OnTxtSoldeBanqueFocusOut(object sender, EventArgs a)
        {
            txtSoldeBanque.FocusOutEvent -= OnTxtSoldeBanqueFocusOut;
			txtSoldeBanque.Text = Global.GetValueOrZero(txtInfo, sender, true).ToString();
			txtSoldeBanque.FocusOutEvent += OnTxtSoldeBanqueFocusOut;
			//
            UpdateData(false);
			DoCalcul();
        }
        private void OnTxtSoldeInitialFocusOut(object sender, EventArgs a)
        {
            txtSoldeInitial.FocusOutEvent -= OnTxtSoldeInitialFocusOut;
			txtSoldeInitial.Text = Global.GetValueOrZero(txtInfo, sender, true).ToString();
			txtSoldeInitial.FocusOutEvent += OnTxtSoldeInitialFocusOut;
			//
            Global.ListeComptesModified = true;
            UpdateData(false);
			DoCalcul();
        }
    }
}
