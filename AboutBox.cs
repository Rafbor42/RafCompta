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
    public class AboutBox : AboutDialog
    {
        [UI] private ButtonBox btnBox = null;

        public AboutBox(Window ParentWindow) : this(new Builder("AboutBox.glade"))
        {
            this.TransientFor = ParentWindow;
            this.SetPosition(WindowPosition.CenterOnParent);
            this.Modal = true;
            this.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        
            Widget[] buttons = btnBox.Children;
            // attention, pas de bouton Fermer sur Ubuntu
            if (buttons.Length > 2)
            {
                Button btnFermer = (Button)buttons[2];
                btnFermer.Clicked += OnBtnFermerClicked;
            }
        }

        private void OnBtnFermerClicked(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private AboutBox(Builder builder) : base(builder.GetRawOwnedObject("AboutBox"))
        {
            builder.Autoconnect(this);
            DeleteEvent += delegate { OnBtnFermerClicked( this, null); };
        }
    }
}