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
using Gtk;
using UI = Gtk.Builder.ObjectAttribute;

namespace RafCompta
{
    public class AideBox : Dialog
    {
        [UI] private TextView txtTexte = null;

        public AideBox(Window ParentWindow) : this(new Builder("AideBox.glade"))
        {
            this.TransientFor = ParentWindow;
            this.SetPosition(WindowPosition.CenterOnParent);
            this.Modal = true;
            this.Title = "Aide";
            txtTexte.WrapMode = WrapMode.Word;
            //
            TextBuffer buffer = txtTexte.Buffer;
            // tags
            TextTag tag  = new TextTag("bold");
			tag.Weight = Pango.Weight.Bold;
			buffer.TagTable.Add(tag);
            //
            tag  = new TextTag("underline");
			tag.Underline = Pango.Underline.Single;
			buffer.TagTable.Add(tag);
            //
            tag  = new TextTag("blue");
            tag.Foreground = "blue";
            //
            TextIter insertIter = buffer.StartIter;
            AjouteTexte(buffer, ref insertIter);
        }

        private AideBox(Builder builder) : base(builder.GetRawOwnedObject("AideBox"))
        {
            builder.Autoconnect(this);
            DeleteEvent += delegate { this.Dispose(); };
        }

        private void AjouteTexte(TextBuffer buffer, ref TextIter insertIter)
        {
            buffer.Insert(ref insertIter, "Le séparateur décimal est celui définit dans les paramètres linguistiques, soit la virgule pour la France. Dans les zones de saisie de nombres, le point est automatiquement remplacé par une virgule.\n\n");
            buffer.InsertWithTagsByName(ref insertIter, "Pour effectuer un rapprochement:\n", "bold", "underline");
            buffer.Insert(ref insertIter, "- renseigner le solde banque puis sélectionner les lignes à rapprocher (cocher la case à gauche).\n\n");
            buffer.Insert(ref insertIter,  "=> l'écart est recalculé, s'il est égal à 0, le bouton 'Rapprocher' s'affiche.\n");
            buffer.Insert(ref insertIter,  "=> dans l'onglet 'Paramètres', si 'Archiver les lignes rapprochées' est coché, ces lignes seront ajoutées au fichier d'archives.\n\n");
            buffer.InsertWithTagsByName(ref insertIter, "NB: ", "bold");
            buffer.Insert(ref insertIter,  "Ecart = SoldeInitial + TotalCréditSélection - TotalDébitSélection - SoldeBanque.\n\n");
            buffer.InsertWithTagsByName(ref insertIter, "Pour modifier/supprimer une opération:\n", "bold", "underline");
            buffer.Insert(ref insertIter,  "- sélectionner la ligne afin qu'elle soit en surbrillance (inutile de cocher la case à gauche)\n");
            //
            buffer.Insert(ref insertIter, "\n");
            buffer.InsertWithTagsByName(ref insertIter, "Documentation complète:\n", "bold");
            buffer.InsertWithTagsByName(ref insertIter, "https://github.com/Rafbor42/RafCompta", "underline", "blue");
        }
    }
}