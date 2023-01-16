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
using System.Collections;
using System.Threading;
using System.Xml;
using System.Configuration;
using Gtk;
using System.IO;

namespace RafCompta
{
	/// <summary>
	/// Description of Global.
	/// </summary>
	public static class Global
	{
		public struct sCompte
		{
			public string strNomCompte;
			public double dblSoldeInitial;
			public string strNomFichier;
			public string strNomArchive;
		};
		public static ArrayList arComptes;
		//
		static bool bChargeFichierAuto;
		static bool bSauveFichierAuto;
		static bool bArchiveLigneRappro;
		static bool bFichierCompteNonTrouve;
		//
		static bool bConfigModified;
		static bool bListeComptesModified;
		static double dblSoldeBanque;
		//
		static string strFichierDonneesComptes;
		static string strNomCompteCourant;
		public static sCompte CompteCourant;
        private static string strAppStartupPath;
        private static string strDossierFichiers;
		//
		public static string FichierDonneesComptes { get => strFichierDonneesComptes; set => strFichierDonneesComptes = value; }
		public static bool ConfigModified {	get => bConfigModified; set => bConfigModified = value; }
		public static bool ChargeFichierAuto { get => bChargeFichierAuto; set => bChargeFichierAuto = value; }
		public static string DossierFichiers { get => strDossierFichiers; set => strDossierFichiers = value; }
        public static string AppStartupPath { get => strAppStartupPath; set => strAppStartupPath = value; }
        public static string NomCompteCourant { get => strNomCompteCourant; set => strNomCompteCourant = value; }
        public static double SoldeBanque { get => dblSoldeBanque; set => dblSoldeBanque = value; }
        public static bool ListeComptesModified { get => bListeComptesModified; set => bListeComptesModified = value; }
        public static bool SauveFichierAuto { get => bSauveFichierAuto; set => bSauveFichierAuto = value; }
        public static bool ArchiveLigneRappro { get => bArchiveLigneRappro; set => bArchiveLigneRappro = value; }
        public static bool FichierCompteNonTrouve { get => bFichierCompteNonTrouve; set => bFichierCompteNonTrouve = value; }

        public enum eTrvOperationsCols
		{
			Select=0,
			Operation,
			ModePaiement,
			Date,
			Credit,
			Debit,
			Key
		}

		public static ArrayList arListItem = new ArrayList();
		
		/// <summary>
		/// Constructeur.
		/// </summary>
		static Global()
		{
			InitParams();
		}
		
		/// <summary>
		/// Initialisation des paramètres.
		/// </summary>
		static void InitParams()
		{
			arComptes = new ArrayList();
			//
			ChargeFichierAuto = true;
			SauveFichierAuto = false;
			ArchiveLigneRappro = true;
			FichierCompteNonTrouve = false;
			//
			ConfigModified = false;
			ListeComptesModified = false;
			SoldeBanque = 0;
			//
			FichierDonneesComptes = "ListeComptes.xml";
			NomCompteCourant = "";
			//
			AppStartupPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			DossierFichiers = AppStartupPath + Path.DirectorySeparatorChar + "Fichiers";
			// création du dossier Fichiers, si pas présent
			if (!Directory.Exists(DossierFichiers))
				Directory.CreateDirectory(DossierFichiers);
			//
			InitCompteCourant();
			//
			arListItem.Add("---");
			arListItem.Add("CB");
			arListItem.Add("Chèque");
			arListItem.Add("Dépot");
			arListItem.Add("Prélèvement");
			arListItem.Add("Virement");
		}
		
		private static void InitCompteCourant()
		{
			CompteCourant.dblSoldeInitial = 0;
			CompteCourant.strNomCompte = string.Empty;
			CompteCourant.strNomFichier = string.Empty;
			CompteCourant.strNomArchive = string.Empty;
		}

		/// <summary>
		/// Ajoute un compte.
		/// </summary>
		/// <param name="sNom"></param>
		/// <param name="dSoldeInitial"></param>
		public static string AjouteCompte(string sNom, double dSoldeInitial)
		{
			// trim et remplacement des espaces dans le nom
			string strNom = sNom.Trim().Replace(' ','_');
			//
			sCompte compte = new sCompte();
			compte.strNomCompte = strNom;
			compte.dblSoldeInitial = dSoldeInitial;
			compte.strNomFichier = strNom + ".xml";
			compte.strNomArchive = "Arch_" + strNom + "_" + DateTime.Now.Year + ".xml";
			//
			arComptes.Add(compte);
			return compte.strNomFichier;
		}
		
		// Retourne true si le compte existe, sinon false.
		public static bool IsCompteExistant(string strNom)
		{
			for (Int16 n = 0; n < arComptes.Count; n++)
			{
				if (((sCompte)arComptes[n]).strNomCompte == strNom)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Récupère les paramètres de l'application dans le fichier de configuration.
		/// </summary>
		public static void LireConfigLocal(ref string strMsg)
		{
        	try
        	{
        		if (ConfigurationManager.AppSettings["FichierDonneesComptes"] != String.Empty)
					FichierDonneesComptes = ConfigurationManager.AppSettings["FichierDonneesComptes"];
        		//
        		//ChargeFichierAuto = Convert.ToBoolean(ConfigurationManager.AppSettings["ChargeFichierAuto"]); toujours vrai
	       		SauveFichierAuto = Convert.ToBoolean(ConfigurationManager.AppSettings["SauveFichierAuto"]);
    	   		ArchiveLigneRappro = Convert.ToBoolean(ConfigurationManager.AppSettings["ArchiveLigneRappro"]);
        	}
        	catch (Exception ex)
			{
				strMsg += ex.Message + "\r\n";
			}
		}
		
		static double GetDoubleValue(string strKey)
		{
			string str = String.Empty;
			double dblVal;
			
			str = ConfigurationManager.AppSettings[strKey];
			// garde-fou
			if (Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
				str = str.Replace('.',',');
			else
				str = str.Replace(',','.');
			dblVal = Convert.ToDouble(str);
			
			return dblVal;
		}
		
		/// <summary>
		/// Ecriture des paramètres dans le fichier de configuration.
		/// </summary>
		public static void EcrireConfigLocal(ref string strMsg)
		{
			try
			{
				XmlElement element;
	        	XmlDocument xmlDoc = new XmlDocument();
				var conf = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
 				xmlDoc.Load(conf.FilePath);
				
				//foreach (XmlElement element in xmlDoc.DocumentElement)
				for (int i = 0; i < xmlDoc.DocumentElement.ChildNodes.Count; i++)
				{
					element = (XmlElement)xmlDoc.DocumentElement.ChildNodes[i];
				    if (element.Name.Equals("appSettings"))
				    {
				    	foreach (XmlNode node in element.ChildNodes)
				        {
				            if (node.Attributes[0].Value.Equals("FichierDonneesComptes"))
				            	node.Attributes[1].Value = FichierDonneesComptes;
				            //
				            if (node.Attributes[0].Value.Equals("ChargeFichierAuto"))
				            	node.Attributes[1].Value = ChargeFichierAuto.ToString();
				            //
				            if (node.Attributes[0].Value.Equals("SauveFichierAuto"))
				            	node.Attributes[1].Value = SauveFichierAuto.ToString();
				            //
				            if (node.Attributes[0].Value.Equals("ArchiveLigneRappro"))
				            	node.Attributes[1].Value = ArchiveLigneRappro.ToString();
				    	}
				    }
				}
				xmlDoc.Save(conf.FilePath);
				ConfigurationManager.RefreshSection("appSettings");
			}
			catch (Exception ex)
			{
				strMsg += ex.Message + "\r\n";
			}
		}
		
		/// <summary>
		/// Recherche le compte courant.
		/// </summary>
		/// <returns></returns>
		public static void GetCompteCourant()
		{
			for (Int16 n = 0; n < arComptes.Count; n++)
			{
				if (((sCompte)arComptes[n]).strNomCompte == NomCompteCourant)
				{
					CompteCourant = (sCompte)arComptes[n];
					break;
				}
			}
		}
		
		/// <summary>
		/// Redéfinit le compte courant dans la collection.
		/// </summary>
		public static void SetCompteCourant()
		{
			for (Int16 n = 0; n < arComptes.Count; n++)
			{
				if (((sCompte)arComptes[n]).strNomCompte == NomCompteCourant)
				{
					arComptes[n] = CompteCourant;
					break;
				}
			}
		}

		public static void SupprimeCompteCourant()
		{
			for (Int16 n = 0; n < arComptes.Count; n++)
			{
				if (((sCompte)arComptes[n]).strNomCompte == NomCompteCourant)
				{
					arComptes.RemoveAt(n);
					break;
				}
			}
			InitCompteCourant();
		}

		// Affiche un message dans une boite de dialogue.
        public static void ShowMessage(string strTitle, string strMsg, Window pParent)
        {
            MessageDialog md = new MessageDialog(pParent, DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close, strTitle);
            md.SecondaryText = strMsg;
            md.Run();
            md.Dispose();
        }

		// Controle du format de la valeur saisie dans la zone de texte.
		// NB: sur les appels à cette méthode, pour éviter les appels en boucle
		// il faut désactiver l'évenement TextChanged sur le controle source.
        public static bool CheckValeurs(Entry txtInfo, object sender)
		{
			Entry txtBox = (Entry)sender;
			try
			{
				double dblValue;
				
				// garde-fou: remplacement du point par la virgule
				if (Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
					txtBox.Text = txtBox.Text.Replace('.',',');
				else if (Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
					txtBox.Text = txtBox.Text.Replace(',','.');
				// on tente une conversion en double
				dblValue = Convert.ToDouble(txtBox.Text);
				txtInfo.Text = string.Empty;
				//
				return true;
			}
			catch// (Exception ex)
			{
				if (txtBox.Text != string.Empty)
					AfficheInfo(txtInfo, "Erreur format", new Gdk.Color(255,0,0));
			}
			return false;
		}

        public static double GetValueOrZero(Entry txtInfo, object sender, bool bShowMessage=false)
		{
			double dblValue = 0;
			Entry txtBox = (Entry)sender;
			
			try
			{
				// garde-fou: remplacement du point par la virgule
				if (Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
					txtBox.Text = txtBox.Text.Replace('.',',');
				else if (Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
					txtBox.Text = txtBox.Text.Replace(',','.');
				// on tente une conversion en double
				dblValue = Math.Round(Convert.ToDouble(txtBox.Text), 2);
			}
			catch (Exception)
			{
				if (bShowMessage == true)
				{
					// seulement si saisie incorrecte
					if (txtBox.Text != string.Empty)
					{
						AfficheInfo(txtInfo, txtBox.Text + ": le nombre saisi n'est pas dans le bon format.", new Gdk.Color(255,0,0));
					}
				}
			}
			return dblValue;
		}

		public static void AfficheInfo(Entry txtInfo, string strMsg, Gdk.Color color)
		{
			txtInfo.Text = string.Empty;
			txtInfo.ModifyFg(StateType.Normal, color);
			txtInfo.Text = strMsg;
			txtInfo.Show();
		}
	}
}
