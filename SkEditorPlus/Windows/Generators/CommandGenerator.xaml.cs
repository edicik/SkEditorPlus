﻿using AvalonEditB;
using AvalonEditB.Document;
using HandyControl.Controls;
using SkEditorPlus.Managers;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SkEditorPlus.Windows.Generators
{
    public partial class CommandGenerator : Window
    {
        private SkEditorAPI skEditor;

        public CommandGenerator(SkEditorAPI skEditor)
        {
            InitializeComponent();
            this.skEditor = skEditor;
            BackgroundFixManager.FixBackground(this);

        }

        private void OnKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Escape)
            {
                commandGeneratorWindow.Close();
            }
        }

        private void Generate(object sender, System.Windows.RoutedEventArgs e)
        {
            string nameError = (string)FindResource("CmdGenNameError");
            string cooldownError = (string)FindResource("CmdGenCooldownError");
            string unitError = (string)FindResource("CmdGenCooldownUnitError");
            string error = (string)FindResource("Error");

            if (string.IsNullOrWhiteSpace(nameTextbox.Text))
            {
                MessageBox.Error(nameError, error);
                return;
            }

            if (!string.IsNullOrWhiteSpace(cooldownTextbox.Text))
            {
                if (!float.TryParse(cooldownTextbox.Text, out _))
                {
                    MessageBox.Error(cooldownError, error);
                    return;
                }
                if (cooldownComboBox.SelectedIndex == -1)
                {
                    MessageBox.Error(unitError, error);
                    return;
                }
            }

            TextEditor editor = skEditor.GetMainWindow().GetFileManager().GetTextEditor();

            int offset = editor.CaretOffset;
            DocumentLine line = editor.Document.GetLineByOffset(offset);

            string code = "";

            if (!string.IsNullOrEmpty(editor.Document.GetText(line.Offset, line.Length)))
            {
                code += "\n";
            }

            code += $"command /{nameTextbox.Text}:";

            if (!string.IsNullOrEmpty(permTextbox.Text))
            {
                code += $"\n\tpermission: {permTextbox.Text}";
            }
            if (!string.IsNullOrEmpty(permMessTextbox.Text))
            {
                code += $"\n\tpermission message: {permMessTextbox.Text}";
            }
            if (!string.IsNullOrEmpty(aliasesTextbox.Text))
            {
                code += $"\n\taliases: {aliasesTextbox.Text}";
            }
            if (!string.IsNullOrEmpty(usageTextbox.Text))
            {
                code += $"\n\tusage: {usageTextbox.Text}";
            }

            CheckComboBox checkComboBox = executableByComboBox;
            List<string> selectedItems = new();
            foreach (CheckComboBoxItem item in checkComboBox.SelectedItems)
            {
                selectedItems.Add(item.Tag.ToString());
            }

            string executableBy = "";
            if (selectedItems.Count == 2)
            {
                executableBy = "player, console";
            }
            else if (selectedItems.Count == 1)
            {
                executableBy = selectedItems[0];
            }

            if (!string.IsNullOrEmpty(executableBy))
            {
                code += $"\n\texecutable by: {executableBy}";
            }

            if (!string.IsNullOrEmpty(cooldownTextbox.Text))
            {
                ComboBoxItem item = (ComboBoxItem)cooldownComboBox.SelectedItem;
                code += $"\n\tcooldown: {cooldownTextbox.Text} {item.Tag}";
            }

            code += "\n\ttrigger:\n\t\t";

            editor.Document.Insert(offset, code);

            commandGeneratorWindow.Close();

            editor.CaretOffset = editor.Document.TextLength;
        }

        private void OnComboBoxKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                executableByComboBox.IsDropDownOpen = true;
            }
        }
    }
}
