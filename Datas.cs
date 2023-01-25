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
using System.Data;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using Gtk;
using System.Collections;

namespace RafCompta
{
	/// <summary>
	/// Description of Datas.
	/// </summary>
	public class Datas
	{
		public Window pParentWindow=null;
		public DataTable dtTableOperations, dtTableArchOperations, dtTableOperationsArchivees;
		public DataTable dtTableOperationsRecur;
		public ListStore lstoreOperations, lstoreOperationsRecur;
		public ListStore lstoreOperationsArchivees, lstoreListeArchives;
		private OperationBox OperBox;
		private DateTime dtmDate;
		private string strOperation, strModePaiement;
		private double dblCredit, dblDebit;
		private bool bRecurrente;
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		public Datas(Window ParentWindow)
		{
			pParentWindow = ParentWindow;
			//
			dtTableOperations = new DataTable("Operations");
			dtTableOperations.Columns.Add("nKey", typeof(Int16));
			dtTableOperations.Columns.Add("strOperation", typeof(String));
			dtTableOperations.Columns.Add("strModePaiement", typeof(String));
			dtTableOperations.Columns.Add("dtDate", typeof(DateTime));
			dtTableOperations.Columns.Add("dblCredit", typeof(Double));
			dtTableOperations.Columns.Add("dblDebit", typeof(Double));
			dtTableOperations.Columns.Add("bRecurrente", typeof(bool));
			dtTableOperations.Columns.Add("nKeyRecur", typeof(Int16));
			//
			dtTableArchOperations = dtTableOperations.Clone();
			dtTableOperationsArchivees = dtTableOperations.Clone();
			//
			dtTableOperationsRecur = new DataTable("OperationsRecur");
			dtTableOperationsRecur.Columns.Add("strNomCompte", typeof(String));
			dtTableOperationsRecur.Columns.Add("nKey", typeof(Int16));
			dtTableOperationsRecur.Columns.Add("strOperation", typeof(String));
			dtTableOperationsRecur.Columns.Add("strModePaiement", typeof(String));
			dtTableOperationsRecur.Columns.Add("dtDate", typeof(DateTime));
			dtTableOperationsRecur.Columns.Add("dblCredit", typeof(Double));
			dtTableOperationsRecur.Columns.Add("dblDebit", typeof(Double));
			//
            lstoreOperations = new ListStore(typeof(bool), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(Int16));
            lstoreOperationsArchivees = new ListStore(typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(Int16));
			lstoreListeArchives = new ListStore(typeof(string));
			lstoreOperationsRecur = new ListStore(typeof(bool), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(Int16));
			//
			Init();
		}
		
		public void Init()
		{
			dtTableOperations.Clear();
			dtTableArchOperations.Clear();
			dtTableOperationsArchivees.Clear();
			lstoreOperations.Clear();
			lstoreOperationsRecur.Clear();
			GetListeArchivesCompteCourant();
			GetListeOpeRecurCompteCourant();
		}

        private void GetListeOpeRecurCompteCourant()
        {
            foreach (DataRow row in dtTableOperationsRecur.Select("strNomCompte='" + Global.NomCompteCourant + "'", "dtDate ASC"))
			{
				if (row.RowState == DataRowState.Deleted)
					continue;
				//
				lstoreOperationsRecur.AppendValues(false, row["strOperation"].ToString(), row["strModePaiement"].ToString(), Convert.ToDateTime(row["dtDate"]).Day, row["dblCredit"].ToString(), row["dblDebit"].ToString(), Convert.ToInt16(row["nKey"]));
			}
        }

        // Récupère dans un ListStore la liste des fichiers d'archives du compte courant.
        public void GetListeArchivesCompteCourant()
		{
			string strFileName;
			// on passe par un DataTable temporaire pour effectuer le tri
			DataTable dtTemp = new DataTable("Temp");
			dtTemp.Columns.Add("strNomFichier", typeof(String));
			//
			lstoreListeArchives.Clear();
			string strPath = Global.DossierFichiers;
			string strPattern = "Arch*" + Global.NomCompteCourant + "*";
			if (Directory.Exists(strPath) == true)
			{
				string[] listeFiles = Directory.GetFiles(strPath, strPattern);
				foreach(string strFile in listeFiles)
				{
					strFileName = Path.GetFileNameWithoutExtension(strFile);
					DataRow newRow = dtTemp.NewRow();
					newRow["strNomFichier"] = strFileName;
					dtTemp.Rows.Add(newRow);
				}
				// sélection triée
				foreach(DataRow row in dtTemp.Select("1=1", "strNomFichier DESC"))
				{
					strFileName = row["strNomFichier"].ToString();
					lstoreListeArchives.AppendValues(strFileName);
				}
			}
		}

		/// <summary>
		/// Retourne vrai si un élément est nouveau, modifié ou effacer.
		/// </summary>
		/// <returns></returns>
		public bool IsModified()
		{
			foreach(DataRow row in dtTableOperations.Rows)
			{
				if (row.RowState == DataRowState.Modified || row.RowState == DataRowState.Added || row.RowState == DataRowState.Deleted)
					return true;
			}
			return false;
		}
		
		/// <summary>
		/// Lecture du fichier et création de la liste des comptes.
		/// </summary>
		/// <param name="strMsg"></param>
		/// <returns></returns>
		public short ChargeListeComptes(ref string strMsg)
		{
			short nNbParam = 0, nKey;
			string strNomCompte;
			double dblSoldeInitial;
			DateTime dtDate;
			string strOperation, strModePaiement;
			double dblCredit, dblDebit;
			
			try
			{
				XElement xmlElt = XElement.Load(Path.Combine(Global.DossierFichiers, Global.FichierDonneesComptes));
				foreach (XElement elt2 in xmlElt.Elements("Comptes"))
				{
					foreach (XElement elt in elt2.Elements())
					{
						strNomCompte = (string)elt.Element("NomCompte");
						dblSoldeInitial = Convert.ToDouble((string)elt.Element("SoldeInitial"));
						//
						Global.AjouteCompte(strNomCompte, dblSoldeInitial);
						nNbParam++;
						//
						foreach (XElement elt3 in elt.Elements("OpéRécurrentes"))
						{
							foreach (XElement elt4 in elt3.Elements())
							{
								nKey = Convert.ToInt16((string)elt4.Attribute("Key"));
								dtDate = Convert.ToDateTime((string)elt4.Element("Date"));
								strOperation = (string)elt4.Element("Libellé");
								strModePaiement = (string)elt4.Element("ModePaiement");
								dblCredit = Convert.ToDouble((string)elt4.Element("Crédit"));
								dblDebit = Convert.ToDouble((string)elt4.Element("Débit"));
								//
								AjouteOperationRecur(strNomCompte, nKey, dtDate, strOperation, strModePaiement, dblCredit, dblDebit);
							}
						}
						dtTableOperationsRecur.AcceptChanges();
					}
				}
			}
			catch (FileNotFoundException)
			{
				Global.FichierCompteNonTrouve = true;
			}
			catch (Exception ex)
			{
				strMsg += ex.Message;
			}
			//
			return nNbParam;
		}

        private void AjouteOperationRecur
		(
			string strNomCompte,
			Int16 nKey,
			DateTime dtDate,
			string strOperation,
			string strModePaiement,
			double dblCredit,
			double dblDebit
		)
        {
			DataRow newrow = dtTableOperationsRecur.NewRow();
			newrow["strNomCompte"] = strNomCompte;
			newrow["nKey"] = nKey;
			newrow["strOperation"] = strOperation;
			newrow["strModePaiement"] = strModePaiement;
			newrow["dblCredit"] = dblCredit;
			newrow["dblDebit"] = dblDebit;
			newrow["dtDate"] = dtDate;
			dtTableOperationsRecur.Rows.Add(newrow);
			//
			lstoreOperationsRecur.AppendValues(false, strOperation, strModePaiement, dtDate.Day, dblCredit.ToString(), dblDebit.ToString(), nKey);
        }

        /// <summary>
        /// Enregistrement de la liste des comptes.
        /// </summary>
        /// <param name="strMsg"></param>
        /// <returns></returns>
        public short EnregistrerDonneesComptes(ref string strMsg)
		{
			XmlTextWriter XMLWriter = null;
			short nNbParam = 0;
			Global.sCompte compte;

			try
			{
				XMLWriter = new XmlTextWriter(Path.Combine(Global.DossierFichiers, Global.FichierDonneesComptes), Encoding.UTF8);
				XMLWriter.Formatting = Formatting.Indented;

				XMLWriter.WriteStartDocument(true);
				XMLWriter.WriteStartElement("Données_RafCompta");
				XMLWriter.WriteStartElement("Comptes");
				for (Int16 n = 0; n < Global.arComptes.Count; n++)
				{
					compte = (Global.sCompte)Global.arComptes[n];
					//
					XMLWriter.WriteStartElement("Compte");
					XMLWriter.WriteElementString("NomCompte", compte.strNomCompte);
					XMLWriter.WriteElementString("SoldeInitial", compte.dblSoldeInitial.ToString());
					// opérations récurrentes
					XMLWriter.WriteStartElement("OpéRécurrentes");
					foreach (DataRow row in dtTableOperationsRecur.Select("strNomCompte='" + compte.strNomCompte + "'"))
					{
						if (row.RowState == DataRowState.Deleted)
							continue;
						//
						XMLWriter.WriteStartElement("Opération");
						XMLWriter.WriteAttributeString("Key", row["nKey"].ToString());
						XMLWriter.WriteElementString("ModePaiement", row["strModePaiement"].ToString());
						XMLWriter.WriteElementString("Date", Convert.ToDateTime(row["dtDate"]).ToShortDateString());
						XMLWriter.WriteElementString("Libellé", row["strOperation"].ToString());
						XMLWriter.WriteElementString("Crédit", row["dblCredit"].ToString());
						XMLWriter.WriteElementString("Débit", row["dblDebit"].ToString());
						XMLWriter.WriteEndElement();
					}
					XMLWriter.WriteEndElement();
					dtTableOperationsRecur.AcceptChanges();
					//
					XMLWriter.WriteEndElement();
					//
					nNbParam++;
				}
				XMLWriter.WriteEndElement();
				XMLWriter.WriteEndElement();
				XMLWriter.WriteEndDocument();
			}
			catch (Exception ex)
			{
				strMsg += ex.Message;
			}
			finally
			{
				if (XMLWriter != null)
					XMLWriter.Close();
			}
			return nNbParam;
		}
		
		/// <summary>
		/// Chargement des opérations.
		/// </summary>
		/// <returns></returns>
		public short ChargeDonneesOperations(ref string strMsg)
		{
			short nNbParam = 0;

			DateTime dtDate;
			string strOperation, strModePaiement;
			double dblCredit, dblDebit;
			Int16 nKey, nKeyRecur;
			try
			{
				XElement xmlElt = XElement.Load(Path.Combine(Global.DossierFichiers, Global.CompteCourant.strNomFichier));
				foreach (XElement elt2 in xmlElt.Elements("Opérations"))
				{
					foreach (XElement elt in elt2.Elements())
					{
						bRecurrente = false;
						nKeyRecur = 0;
						nKeyRecur = Convert.ToInt16((string)elt.Attribute("KeyRecur"));
						if (nKeyRecur > 0)
							bRecurrente = true;
						//
						dtDate = Convert.ToDateTime((string)elt.Element("Date"));
						strOperation = (string)elt.Element("Libellé");
						strModePaiement = (string)elt.Element("ModePaiement");
						dblCredit = Convert.ToDouble((string)elt.Element("Crédit"));
						dblDebit = Convert.ToDouble((string)elt.Element("Débit"));
						//
						nKey = AjouteOperation(dtDate, strOperation, strModePaiement, dblCredit, dblDebit, bRecurrente, nKeyRecur, false);
						nNbParam++;
					}
				}
				dtTableOperations.AcceptChanges();
			}
			catch (Exception ex)
			{
				strMsg += "Erreur: " + ex.Message;
			}
			//
			return nNbParam;
		}
		
		/// <summary>
		/// Enregistrement des opérations.
		/// </summary>
		/// <param name="strMsg"></param>
		/// <returns></returns>
		public short EnregistrerDonneesOperations(ref string strMsg)
		{
			XmlTextWriter XMLWriter = null;
			short nNbParam = 0;
			string strPathFichier = Path.Combine(Global.DossierFichiers, Global.CompteCourant.strNomFichier);

			if (File.Exists(strPathFichier) == false)
				return 0;

			try
			{
				XMLWriter = new XmlTextWriter(strPathFichier, Encoding.UTF8);
				XMLWriter.Formatting = Formatting.Indented;
				XMLWriter.WriteStartDocument(true);
				XMLWriter.WriteStartElement("Données_RafCompta");
				XMLWriter.WriteStartElement("Opérations");
				foreach (DataRow row in dtTableOperations.Rows)
				{
					if (row.RowState == DataRowState.Deleted)
						continue;
					//
					XMLWriter.WriteStartElement("Opération");
					XMLWriter.WriteAttributeString("KeyRecur", row["nKeyRecur"].ToString());
					XMLWriter.WriteElementString("ModePaiement", row["strModePaiement"].ToString());
					XMLWriter.WriteElementString("Date", Convert.ToDateTime(row["dtDate"]).ToShortDateString());
					XMLWriter.WriteElementString("Libellé", row["strOperation"].ToString());
					XMLWriter.WriteElementString("Crédit", row["dblCredit"].ToString());
					XMLWriter.WriteElementString("Débit", row["dblDebit"].ToString());
					XMLWriter.WriteEndElement();
				}
				XMLWriter.WriteEndElement();
				XMLWriter.WriteEndElement();
				XMLWriter.WriteEndDocument();
				//
				dtTableOperations.AcceptChanges();
			}
			catch (Exception ex)
			{
				strMsg += ex.Message;
			}
			finally
			{
				if (XMLWriter != null)
					XMLWriter.Close();
			}
			return nNbParam;
		}
		
		public void EnregistrerDonneesCompteVierge(string strPathFichier, ref string strMsg)
		{
			XmlTextWriter XMLWriter = null;

			try
			{
				XMLWriter = new XmlTextWriter(strPathFichier, Encoding.UTF8);
				XMLWriter.Formatting = Formatting.Indented;
				XMLWriter.WriteStartDocument(true);
				XMLWriter.WriteStartElement("Données_RafCompta");
				XMLWriter.WriteStartElement("Opérations");
				//
				XMLWriter.WriteEndElement();
				XMLWriter.WriteEndElement();
				XMLWriter.WriteEndDocument();
			}
			catch (Exception ex)
			{
				strMsg += ex.Message;
			}
			finally
			{
				if (XMLWriter != null)
					XMLWriter.Close();
			}
		}

		/// <summary>
		/// Chargement des opérations archivées.
		/// </summary>
		/// <returns></returns>
		public short ChargeOperationsArchivees(string strFileName, ref string strMsg)
		{
			short nNbParam = 0;

			DateTime dtDate;
			string strOperation, strModePaiement;
			double dblCredit, dblDebit;
			
			dtTableOperationsArchivees.Clear();
			lstoreOperationsArchivees.Clear();
			try
			{
				XElement xmlElt = XElement.Load(Path.Combine(Global.DossierFichiers, strFileName));
				foreach (XElement elt2 in xmlElt.Elements("Opérations"))
				{
					foreach (XElement elt in elt2.Elements())
					{
						dtDate = Convert.ToDateTime((string)elt.Element("Date"));
						strOperation = (string)elt.Element("Libellé");
						strModePaiement = (string)elt.Element("ModePaiement");
						dblCredit = Convert.ToDouble((string)elt.Element("Crédit"));
						dblDebit = Convert.ToDouble((string)elt.Element("Débit"));
						//
						AjouteOperationArchive(dtDate, strOperation, strModePaiement, dblCredit, dblDebit);
						nNbParam++;
					}
				}
				dtTableOperationsArchivees.AcceptChanges();
			}
			catch (Exception ex)
			{
				strMsg += "Erreur: " + ex.Message;
			}
			//
			return nNbParam;
		}
		
		/// <summary>
		/// Sauvegarde des opérations archivées.
		/// </summary>
		/// <param name="strMsg"></param>
		public void EnregistreOperationsArchivees(ref string strMsg)
		{
			XmlTextWriter XMLWriter = null;
			string strPathFichier = string.Empty;

			// on recense les années des opérations à archiver
			ArrayList arYears = new ArrayList();
			int nYear = DateTime.Now.Year;
			arYears.Add(nYear);
			bool bExist;
			foreach (DataRow row in dtTableArchOperations.Select("1=1", "dtDate ASC"))
			{
				bExist = false;
				if (Convert.ToDateTime(row["dtDate"]).Year != nYear)
				{
					nYear = Convert.ToDateTime(row["dtDate"]).Year;
					// controle si année déjà présente
					foreach (int nAnnee in arYears)
					{
						if (nAnnee == nYear)
						{
							bExist = true;
							break;
						}
					}
					if (bExist == false)
						arYears.Add(nYear);
				}
			}
			// pour chaque année trouvée
			foreach (int nAnnee in arYears)
			{
				strPathFichier = Path.Combine(Global.DossierFichiers, "Arch_" + Global.CompteCourant.strNomCompte + "_" + nAnnee + ".xml");
				try
				{
					// si fichier n'existe pas
					if (File.Exists(strPathFichier) == false)
					{
						XMLWriter = new XmlTextWriter(strPathFichier, Encoding.UTF8);
						XMLWriter.Formatting = Formatting.Indented;
		
						XMLWriter.WriteStartDocument(true);
						XMLWriter.WriteStartElement("Données_RafCompta");
						XMLWriter.WriteStartElement("Opérations");
						// ajout donnée
						foreach (DataRow row in dtTableArchOperations.Rows)
						{
							if (row.RowState == DataRowState.Deleted)
								continue;
							// seulement pour nAnnee
							if (Convert.ToDateTime(row["dtDate"]).Year != nAnnee)
								continue;
							//
							XMLWriter.WriteStartElement("Opération");
							XMLWriter.WriteElementString("ModePaiement", row["strModePaiement"].ToString());
							XMLWriter.WriteElementString("Date", Convert.ToDateTime(row["dtDate"]).ToShortDateString());
							XMLWriter.WriteElementString("Libellé", row["strOperation"].ToString());
							XMLWriter.WriteElementString("Crédit", row["dblCredit"].ToString());
							XMLWriter.WriteElementString("Débit", row["dblDebit"].ToString());
							XMLWriter.WriteEndElement();
						}
						//
						XMLWriter.WriteEndElement();
						XMLWriter.WriteEndElement();
						XMLWriter.WriteEndDocument();
					}
					else
					{
						XElement xmlElt = XElement.Load(strPathFichier);
						// ajout donnée
						foreach (DataRow row in dtTableArchOperations.Rows)
						{
							// seulement pour nAnnee
							if (Convert.ToDateTime(row["dtDate"]).Year != nAnnee)
								continue;
							//
							XElement newElt = new XElement("Opération",
											new XElement("ModePaiement", row["strModePaiement"].ToString()),
											new XElement("Date", Convert.ToDateTime(row["dtDate"]).ToShortDateString()),
											new XElement("Libellé", row["strOperation"].ToString()),
											new XElement("Crédit", row["dblCredit"].ToString()),
											new XElement("Débit", row["dblDebit"].ToString()));
							xmlElt.Element("Opérations").Add(newElt);
						}
						xmlElt.Save(strPathFichier);
					}
				}
				catch (Exception ex)
				{
					strMsg += ex.Message;
				}
				finally
				{
					if (XMLWriter != null)
						XMLWriter.Close();
				}
			}
			//
			if (strMsg == string.Empty)
				dtTableArchOperations.Clear();
		}
		
		/// <summary>
		/// Ajoute une opération dans la table dtTableOperations et dans lstoreOperations.
		/// </summary>
		public Int16 AjouteOperation
		(
			DateTime dtmDate,
			string strOperation,
			string strModePaiement,
			double dblCredit,
			double dblDebit,
			bool bRecurrente,
			Int16 nKeyRecur,
			bool bCreerOpeRecur
		)
		{
			// détermination de la clé
			Int16 nKey = 0;
			foreach(DataRow row in dtTableOperations.Rows)
			{
				if (row.RowState == DataRowState.Deleted)
						continue;
				//
				if (Convert.ToInt16(row["nKey"]) > nKey)
					nKey = Convert.ToInt16(row["nKey"]);
			}
			//
			DataRow newrow = dtTableOperations.NewRow();
			newrow["nKey"] = ++nKey;
			newrow["strOperation"] = strOperation;
			newrow["strModePaiement"] = strModePaiement;
			newrow["dblCredit"] = dblCredit;
			newrow["dblDebit"] = dblDebit;
			newrow["dtDate"] = dtmDate;
			newrow["bRecurrente"] = bRecurrente;
			newrow["nKeyRecur"] = nKeyRecur;
			dtTableOperations.Rows.Add(newrow);
			//
			lstoreOperations.AppendValues(false, strOperation, strModePaiement, dtmDate.ToShortDateString(), dblCredit.ToString(), dblDebit.ToString(), nKey);
			//
			if (bCreerOpeRecur == true)
			{
				AjouteOperationRecur(Global.CompteCourant.strNomCompte, nKeyRecur, dtmDate, strOperation, strModePaiement, dblCredit, dblDebit);
			}
			return nKey;
		}
		
		/// <summary>
		/// Ajoute une opération dans la table dtTableOperationsArchivees et dans le liststore lstoreOperationsArchivees.
		/// </summary>
		/// <param name="dtmDate"></param>
		/// <param name="strOperation"></param>
		/// <param name="strModePaiement"></param>
		/// <param name="dblCredit"></param>
		/// <param name="dblDebit"></param>
		public void AjouteOperationArchive
		(
			DateTime dtmDate,
			string strOperation,
			string strModePaiement,
			double dblCredit,
			double dblDebit)
		{
			// détermination de la clé
			Int16 nKey = 0;
			foreach(DataRow row in dtTableOperationsArchivees.Rows)
			{
				if (row.RowState == DataRowState.Deleted)
						continue;
				//
				if (Convert.ToInt16(row["nKey"]) > nKey)
					nKey = Convert.ToInt16(row["nKey"]);
			}
			//
			DataRow newrow = dtTableOperationsArchivees.NewRow();
			newrow["nKey"] = ++nKey;
			newrow["strOperation"] = strOperation;
			newrow["strModePaiement"] = strModePaiement;
			newrow["dblCredit"] = dblCredit;
			newrow["dblDebit"] = dblDebit;
			newrow["dtDate"] = dtmDate;
			dtTableOperationsArchivees.Rows.Add(newrow);
			//
			lstoreOperationsArchivees.AppendValues(strOperation, strModePaiement, dtmDate.ToShortDateString(), dblCredit.ToString(), dblDebit.ToString(), nKey);
		}
		
		/// <summary>
		/// Ajoute une opération par l'intermédiaire de la boite dialogue.
		/// </summary>
		public void AjouteBoxOperation
		()
		{
			dtmDate = DateTime.Now;
			strOperation = string.Empty;
			strModePaiement = "---";
			dblCredit = 0;
			dblDebit = 0;
			bRecurrente = false;
			Int16 nKeyRecur = 0;
			bool bCreerOpeRecur = false;
			OperBox = new OperationBox(pParentWindow, "Nouvelle opération", true);
			OperBox.Destroyed += delegate { OnOperBoxDestroyed(ref strOperation, ref strModePaiement, ref dblCredit, ref dblDebit, ref dtmDate, ref bRecurrente); };
			if (OperBox.ShowD(strOperation, strModePaiement, dblCredit, dblDebit, dtmDate, bRecurrente) == ResponseType.Ok)
			{
				if (bRecurrente == true)
				{
					nKeyRecur = Global.GetNewKeyOpeReccurente();
					bCreerOpeRecur = true;
				}
				AjouteOperation(dtmDate, strOperation, strModePaiement, dblCredit, dblDebit, bRecurrente, nKeyRecur, bCreerOpeRecur);
			}
		}
		
		/// <summary>
		/// Modifie une opération par l'intermédiaire de la boite dialogue.
		/// </summary>
		/// <param name="nKey"></param>
		public void ModifieOperation
		(
			TreeIter iter
		)
		{
			Int16 nKey = Convert.ToInt16(lstoreOperations.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Key)));
			foreach(DataRow row in dtTableOperations.Select("nKey=" + nKey))
			{
				strOperation = row["strOperation"].ToString();
				strModePaiement = row["strModePaiement"].ToString();
				dblCredit = Convert.ToDouble(row["dblCredit"]);
				dblDebit = Convert.ToDouble(row["dblDebit"]);
				dtmDate = Convert.ToDateTime(row["dtDate"]);
				bRecurrente = Convert.ToBoolean(row["bRecurrente"]);
				//
				OperBox = new OperationBox(pParentWindow, "Modifier opération", false);
				OperBox.Destroyed += delegate { OnOperBoxDestroyed(ref strOperation, ref strModePaiement, ref dblCredit, ref dblDebit, ref dtmDate, ref bRecurrente); };
				if (OperBox.ShowD(strOperation, strModePaiement, dblCredit, dblDebit, dtmDate, bRecurrente) == ResponseType.Ok)
				{
					row["strOperation"] = strOperation;
					row["strModePaiement"] = strModePaiement;
					row["dblCredit"] = dblCredit;
					row["dblDebit"] = dblDebit;
					row["dtDate"] = dtmDate;
					row["bRecurrente"] = bRecurrente;
					//
					lstoreOperations.SetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Operation), strOperation);
					lstoreOperations.SetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.ModePaiement), strModePaiement);
					lstoreOperations.SetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Credit), dblCredit.ToString());
					lstoreOperations.SetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Debit), dblDebit.ToString());
					lstoreOperations.SetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Date), dtmDate.ToShortDateString());
				}
			}
		}

		// Destruction de la boite OperationBox.
		private void OnOperBoxDestroyed(ref string strOperation, ref string strModePaiement, ref double dblCredit, ref double dblDebit, ref DateTime dtmDate, ref bool bRecurrente)
        {
            if (OperBox.rResponse == ResponseType.Ok)
            {
                strOperation = OperBox.txtOperation.Text;
                strModePaiement = OperBox.cbModePaiement.ActiveText;
                dtmDate = Convert.ToDateTime(OperBox.txtDate.Text);
                dblCredit = Convert.ToDouble(OperBox.txtCredit.Text);
                dblDebit = Convert.ToDouble(OperBox.txtDebit.Text);
				bRecurrente = OperBox.Recurrente;
            }
        }

		/// <summary>
		/// Duplique l'enregistrement passé en paramètre dans la table d'archivage.
		/// </summary>
		/// <param name="row"></param>
		void ArchiveOperation(DataRow row)
		{
			dtTableArchOperations.ImportRow(row);
		}
		
		/// <summary>
		/// Supprime l'opération dont la clé est passée en paramètre, avec archivage.
		/// </summary>
		/// <param name="nKey"></param>
		/// <param name="strMsg"></param>
		public void SupprimeOperation
		(
			TreeIter iter
		)
		{
			Int16 nKey = Convert.ToInt16(lstoreOperations.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Key)));
			foreach(DataRow row in dtTableOperations.Select("nKey=" + nKey))
			{
				if (row.RowState == DataRowState.Deleted)
					continue;
				//
				if (Global.ArchiveLigneRappro == true)
					ArchiveOperation(row);
				row.Delete();
			}
			lstoreOperations.Remove(ref iter);
		}
		
		/// <summary>
		/// Supprime définitivement (No Return) l'opération dont la clé est passée en paramètre.
		/// </summary>
		/// <param name="nKey"></param>
		public void SupprimeOperationNR
		(
			TreeIter iter
		)
		{
			Int16 nKey = Convert.ToInt16(lstoreOperations.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Key)));
			foreach(DataRow row in dtTableOperations.Select("nKey=" + nKey))
			{
				if (row.RowState == DataRowState.Deleted)
					continue;
				//
				row.Delete();
			}
			lstoreOperations.Remove(ref iter);
		}

		// Recharge lstoreOperations avec le contenu de dtTableOperations trié par date.
        public void DoFiltreDataTable()
        {
            lstoreOperations.Clear();
            foreach(DataRow row in dtTableOperations.Select("1=1", "dtDate ASC"))
			{
                lstoreOperations.AppendValues
                (
                    false,
                    row["strOperation"].ToString(),
                    row["strModePaiement"].ToString(),
                    Convert.ToDateTime(row["dtDate"]).ToShortDateString(),
                    row["dblCredit"].ToString(),
                    row["dblDebit"].ToString(),
                    Convert.ToInt16(row["nKey"])
                );
            }
        }

		public void AjouteOpeRecurSurCompteCourant
		(
			TreeIter iter,
			Int16 nKey
		)
		{
			foreach(DataRow row in dtTableOperationsRecur.Select("nKey=" + nKey))
			{
				if (row.RowState == DataRowState.Deleted)
					continue;

				DateTime dtDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, Convert.ToDateTime(row["dtDate"]).Day);
				AjouteOperation(dtDate,
								row["strOperation"].ToString(),
								row["strModePaiement"].ToString(),
								Convert.ToDouble(row["dblCredit"]),
								Convert.ToDouble(row["dblDebit"]),
								true,
								nKey,
								false
								);
				// on désélectionne l'opération 
				lstoreOperationsRecur.SetValue(iter, 0, false);
			}
		}

		public void SupprimeOpeRecurrente
		(
			TreeIter iter
		)
		{
			Int16 nKey = Convert.ToInt16(lstoreOperationsRecur.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Key)));
			foreach(DataRow row in dtTableOperationsRecur.Select("nKey=" + nKey))
			{
				if (row.RowState == DataRowState.Deleted)
					continue;
				//
				row.Delete();
			}
			lstoreOperationsRecur.Remove(ref iter);
		}

		public void ModifieOpeRecurrente
		(
			TreeIter iter
		)
		{
			Int16 nKey = Convert.ToInt16(lstoreOperationsRecur.GetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Key)));
			foreach(DataRow row in dtTableOperationsRecur.Select("nKey=" + nKey))
			{
				if (row.RowState == DataRowState.Deleted)
					continue;
				//
				strOperation = row["strOperation"].ToString();
				strModePaiement = row["strModePaiement"].ToString();
				dblCredit = Convert.ToDouble(row["dblCredit"]);
				dblDebit = Convert.ToDouble(row["dblDebit"]);
				dtmDate = Convert.ToDateTime(row["dtDate"]);
				bRecurrente = true;
				//
				OperBox = new OperationBox(pParentWindow, "Modifier opération", false);
				OperBox.Destroyed += delegate { OnOperBoxDestroyed(ref strOperation, ref strModePaiement, ref dblCredit, ref dblDebit, ref dtmDate, ref bRecurrente); };
				if (OperBox.ShowD(strOperation, strModePaiement, dblCredit, dblDebit, dtmDate, bRecurrente) == ResponseType.Ok)
				{
					row["strOperation"] = strOperation;
					row["strModePaiement"] = strModePaiement;
					row["dblCredit"] = dblCredit;
					row["dblDebit"] = dblDebit;
					row["dtDate"] = dtmDate;
					//
					lstoreOperationsRecur.SetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Operation), strOperation);
					lstoreOperationsRecur.SetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.ModePaiement), strModePaiement);
					lstoreOperationsRecur.SetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Credit), dblCredit.ToString());
					lstoreOperationsRecur.SetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Debit), dblDebit.ToString());
					lstoreOperationsRecur.SetValue(iter, Convert.ToInt16(Global.eTrvOperationsCols.Date), dtmDate.Day);
				}
				// on désélectionne l'opération 
				lstoreOperationsRecur.SetValue(iter, 0, false);
			}
		}

		// Retourne vrai si l'opération existe déjà dans la table dtTableOperations.
		public bool IsOperationExistante
		(
			Int16 nKey
		)
		{
			foreach(DataRow row in dtTableOperations.Select("nKeyRecur=" + nKey))
			{
				if (row.RowState == DataRowState.Deleted)
					continue;
				//
				return true;
			}
			return false;
		}
    }
}
