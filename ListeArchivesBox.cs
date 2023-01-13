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
    public class ListeArchivesBox : Dialog
    {
        private Datas mdatas;
        [UI] private Button btnOk = null;
        [UI] private Button btnCancel = null;
        [UI] private TreeView trvListeArchives = null;
        private Window pParentWindow;
        //
        public ListeArchivesBox(Window ParentWindow, ref Datas datas) : this(new Builder("ListeArchivesBox.glade"))
        {
            pParentWindow = ParentWindow;
            this.TransientFor = pParentWindow;
            this.SetPosition(WindowPosition.CenterOnParent);
            this.Modal = true;
            this.Title = "Archives " + Global.NomCompteCourant;
            mdatas = datas;
            //
            InitTrvListeArchives();
        }

        private ListeArchivesBox(Builder builder) : base(builder.GetRawOwnedObject("ListeArchivesBox"))
        {
            builder.Autoconnect(this);
            DeleteEvent += delegate { OnBtnCancelClicked(this, new EventArgs()); };//Application.Quit();};
            //
            btnOk.Clicked += OnBtnOkClicked;
            btnCancel.Clicked += OnBtnCancelClicked;
            trvListeArchives.RowActivated += OnTrvListeArchivesRowActivated;
        }

        // Double-clic sur une ligne.
        private void OnTrvListeArchivesRowActivated(object sender, EventArgs a)
        {
            OnBtnOkClicked(sender, a);
        }

        void OnBtnOkClicked(object sender, EventArgs e)
		{
            string strFileName = string.Empty;
            TreeIter iter;
            TreePath chemin;
            TreePath[] chemins;
            chemins = trvListeArchives.Selection.GetSelectedRows();
            // si aucune ligne sélectionnée
            if (chemins.Length == 0)
            {
                Global.ShowMessage("Erreur", "Vous devez sélectionner un fichier", this);
                return;
            }   
            chemin = chemins[0];
            if (mdatas.lstoreListeArchives.GetIter(out iter, chemin))
            {
                strFileName = (mdatas.lstoreListeArchives.GetValue(iter, 0)).ToString();
            }
            this.Dispose();
            // on affiche l'archive sélectionnée
            if (strFileName != string.Empty)
            {
                ArchivesBox archbox = new ArchivesBox(pParentWindow, ref mdatas, strFileName + ".xml");
                archbox.ShowAll();
            }
        }

        void OnBtnCancelClicked(object sender, EventArgs e)
		{
			this.Dispose();
		}

        // Initialisation du treeview des opérations.
        private void InitTrvListeArchives()
        {
            TreeViewColumn colNomArchive = new TreeViewColumn();
            colNomArchive.Title = "NomArchive";
            //
            trvListeArchives.AppendColumn(colNomArchive);
            //
            CellRendererText cellNomArchive = new CellRendererText();
            colNomArchive.PackStart(cellNomArchive, true);
            colNomArchive.AddAttribute(cellNomArchive, "text", 0);            
            //
            trvListeArchives.Model = mdatas.lstoreListeArchives;
        }
    }
}