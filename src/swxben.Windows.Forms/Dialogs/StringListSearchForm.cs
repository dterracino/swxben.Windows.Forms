﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace swxben.Windows.Forms.Dialogs
{
    public partial class StringListSearchForm : Form
    {
        readonly IEnumerable<string> _source;
        private const string FILTER_PROMPT = "Search...";
        bool _loading;

        public string SelectedItem
        {
            get
            {
                return StringListView.SelectedItems.Cast<ListViewItem>().Select(i => i.Text).FirstOrDefault();
            }
            set
            {
                if (value == null) return;

                foreach (var item in StringListView.Items.Cast<ListViewItem>().Where(i => i.Text.ToLower().Equals(value.ToLower())))
                {
                    item.Selected = true;
                    item.EnsureVisible();
                    StringListView.EnsureVisible(item.Index);
                }
            }
        }

        public StringListSearchForm(IEnumerable<string> source)
        {
            InitializeComponent();

            _source = source;

            LoadControl();
            RefreshControl();
        }

        void LoadControl()
        {
            _loading = true;
            ListViewItemTextComparer.AssignTo(StringListView);
            Filter("");
            SearchTextBox.Text = FILTER_PROMPT;
            _loading = false;
        }

        private void SearchTextBox_TextChanged(object sender, EventArgs e)
        {
            if (_loading) return;
            Filter(SearchTextBox.Text);
        }

        void RefreshControl()
        {
            SearchTextBox.ForeColor = (SearchTextBox.Text == FILTER_PROMPT) ? Color.FromKnownColor(KnownColor.GrayText) : Color.FromKnownColor(KnownColor.ControlText);
            SelectItemButton.Enabled = StringListView.SelectedItems.Count != 0;
        }

        void Filter(string filter)
        {
            StringListView.BeginUpdate();
            StringListView.Items.Clear();
            var matches = _source
                .Where(s => string.IsNullOrEmpty(filter) || s.ToLower().Contains(filter.ToLower()));
            StringListView.Items.AddRange(matches.Select(s => new ListViewItem(s)).ToArray());
            StringListView.EndUpdate();

            foreach (ColumnHeader column in StringListView.Columns) column.Width = -2;
        }

        private void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                e.Handled = true;
                SelectItem();
            }
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
            {
                e.Handled = true;

                var currentIndex = 0;

                foreach (int index in StringListView.SelectedIndices) currentIndex = index;

                var newIndex = currentIndex;

                if (e.KeyCode == Keys.Up) newIndex--;
                if (e.KeyCode == Keys.Down) newIndex++;
                if (newIndex < 0) newIndex = StringListView.Items.Count - 1;
                else if (newIndex >= StringListView.Items.Count) newIndex = 0;
                StringListView.Items[currentIndex].Selected = false;
                StringListView.Items[newIndex].Selected = true;
                StringListView.Items[newIndex].EnsureVisible();
            }
        }

        private void StringListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshControl();
        }

        private void SearchTextBox_Enter(object sender, EventArgs e)
        {
            if (SearchTextBox.Text != FILTER_PROMPT) return;
            _loading = true;
            SearchTextBox.Text = "";
            _loading = false;
            RefreshControl();
        }

        private void SearchTextBox_Leave(object sender, EventArgs e)
        {
            if (SearchTextBox.Text != "") return;
            _loading = true;
            SearchTextBox.Text = FILTER_PROMPT;
            _loading = false;
            RefreshControl();
        }

        private void StringListView_ItemActivate(object sender, EventArgs e)
        {
            SelectItem();
        }

        void SelectItem()
        {
            if (StringListView.SelectedItems.Count == 0) return;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelSelectionButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void SelectItemButton_Click(object sender, EventArgs e)
        {
            SelectItem();
        }
    }
}