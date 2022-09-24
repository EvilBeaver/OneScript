/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace V8Reader.Controls
{
    /// <summary>
    /// Interaction logic for ProcedureListWnd.xaml
    /// </summary>
    public partial class ProcedureListWnd : Window
    {
        internal ProcedureListWnd(IList<ProcListItem> ProcList)
        {
            InitializeComponent();

            _procList = ProcList.Select<ProcListItem, ProcListItem>(x=>x).ToList<ProcListItem>();

            lbProcList.ItemsSource = _procList;

            SortMethodsByCheckbox();
        }

        private IList<ProcListItem> _procList;
        //private CollectionViewSource _cw;

        private void txtProcName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var FilterPredicate = new Predicate<object>(objItem => 
                {
                    var item = objItem as ProcListItem;

                    string text = (sender as TextBox).Text;
                    if (text == "")
                    {
                        return true;
                    }
                    else
                    {
                        return item.Name.StartsWith(text, StringComparison.OrdinalIgnoreCase);
                    }
                });
            
            lbProcList.Items.Filter = FilterPredicate;

        }

        private void txtProcName_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                lbProcList.Focus();
            }
        }

        private void Elements_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem lb = sender as ListBoxItem;
            if (lb == null || !(lb.Content is ProcListItem))
                return;

            Dispatcher.BeginInvoke(new Action(() =>
                {
                    PerformSelection(lb.Content as ProcListItem);
                }));
        }

        private void lbProcList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return && lbProcList.SelectedIndex >= 0)
            {
                PerformSelection((ProcListItem)lbProcList.SelectedItem);
            }
        }

        private void chkSort_Click(object sender, RoutedEventArgs e)
        {
            SortMethodsByCheckbox();

        }

        private void SortMethodsByCheckbox()
        {
            if (chkSort.IsChecked == true)
            {

                var sorted = from lst in _procList orderby lst.Name select lst;
                lbProcList.ItemsSource = sorted.ToList<ProcListItem>();

            }
            else
            {
                lbProcList.ItemsSource = _procList;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnGo_Click(object sender, RoutedEventArgs e)
        {
            if (lbProcList.SelectedIndex >= 0)
            {
                PerformSelection((ProcListItem)lbProcList.SelectedItem);
            }

        }

        private void PerformSelection(ProcListItem src)
        {
            SelectedItem = src;
            DialogResult = true;
            Close();
        }

        internal ProcListItem SelectedItem
        {
            get;
            private set;
        }

        class itemsComparer : System.Collections.IComparer
        {

            #region IComparer Members

            public int Compare(object x, object y)
            {
                return String.Compare((string)x, (string)y, true);
            }

            #endregion
        }

    }
}
