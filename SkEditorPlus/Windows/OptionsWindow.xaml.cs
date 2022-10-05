﻿using AvalonEditB;
using HandyControl.Controls;
using HandyControl.Data;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SkEditorPlus.Windows
{
    public partial class OptionsWindow : HandyControl.Controls.Window
    {
        private SkEditorAPI skEditor;

        bool[] debugLetters = new bool[5];

        public OptionsWindow(SkEditorAPI skEditor)
        {
            InitializeComponent();
            this.skEditor = skEditor;
        }

        private void OnWindowLoad(object sender, RoutedEventArgs e)
        {
            if (Properties.Settings.Default.Font == null) return;
            if (!string.IsNullOrEmpty(Properties.Settings.Default.Font))
            {
                fontChooseButton.Content = Properties.Settings.Default.Font;
            }
            else fontChooseButton.Content = "Wybierz czcionkę";
            wrappingCheckbox.IsChecked = Properties.Settings.Default.Wrapping;
            autoSecondCharCheckbox.IsChecked = Properties.Settings.Default.AutoSecondCharacter;
            discordRpcCheckbox.IsChecked = Properties.Settings.Default.DiscordRPC;
        }

        private void FontButtonClick(object sender, RoutedEventArgs e)
        {

            FontSelector fontSelector = new();

            if (fontSelector.ShowDialog().Equals(true))
            {
                Properties.Settings.Default.Font = fontSelector.ResultFontFamily.Source;
                Properties.Settings.Default.Save();
                fontChooseButton.Content = Properties.Settings.Default.Font;
                foreach (TabItem ti in skEditor.GetMainWindow().tabControl.Items)
                {
                    TextEditor textEditor = (TextEditor)ti.Content;
                    textEditor.FontFamily = new System.Windows.Media.FontFamily(fontSelector.ResultFontFamily.Source);
                }
            }
        }

        private void OnKey(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                optionWindow.Close();
            }
        }

        private void AutoSaveChecked(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = HandyControl.Controls.MessageBox.Show(new MessageBoxInfo
            {
                Message = "Próbujesz włączyć opcję, która nie została w pełni przetestowana i może powodować problemy.\nUżywasz na własną odpowiedzialność.",
                Caption = "Uwaga!",
                ConfirmContent = "Spoko!"

            });

            if (result == MessageBoxResult.OK)
            {
                return;
            }

            System.Windows.Controls.CheckBox checkBox = (System.Windows.Controls.CheckBox)sender;
            checkBox.IsChecked = false;
        }

        private void WrappingChecked(object sender, RoutedEventArgs e)
        {
            ChangeWrapping(true);
        }

        private void WrappingUnChecked(object sender, RoutedEventArgs e)
        {
            ChangeWrapping(false);
        }

        private void AutoCharChecked(object sender, RoutedEventArgs e)
        {
            ChangeAutoChar(true);
        }

        private void AutoCharUnChecked(object sender, RoutedEventArgs e)
        {
            ChangeAutoChar(false);
        }

        private void DiscordRpcChecked(object sender, RoutedEventArgs e)
        {
            ChangeDiscordRpc(true);
        }

        private void DiscordRpcUnChecked(object sender, RoutedEventArgs e)
        {
            ChangeDiscordRpc(false);
        }

        private void ChangeWrapping(bool wrapping)
        {
            Properties.Settings.Default.Wrapping = wrapping;
            Properties.Settings.Default.Save();
            foreach (TabItem ti in skEditor.GetMainWindow().tabControl.Items)
            {
                TextEditor textEditor = (TextEditor)ti.Content;
                textEditor.WordWrap = wrapping;
            }
        }

        private void ChangeAutoChar(bool autoChar)
        {
            Properties.Settings.Default.AutoSecondCharacter = autoChar;
            Properties.Settings.Default.Save();
        }

        private void ChangeDiscordRpc(bool discordRpc)
        {
            Properties.Settings.Default.DiscordRPC = discordRpc;
            Properties.Settings.Default.Save();
        }

        private void UpdateSyntaxClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(new MessageBoxInfo
            {
                Message = "Jeśli kontynujesz, zostanie pobrany oraz zamieniony plik podświetlania składni. Jeśli robiłeś jakieś zmiany w nim i nie chcesz ich stracić, zrób kopię.",
                Caption = "Uwaga!",
                ConfirmContent = "Kontynuuj",
                CancelContent = "Anuluj",
                Button = MessageBoxButton.OKCancel

            });

            if (result.Equals(MessageBoxResult.OK))
            {
                UpdateSyntaxFile();
            }
        }

        public async void UpdateSyntaxFile()
        {
            string appPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\SkEditor Plus";
            using var client = new HttpClient();
            Uri uri = new("https://notro.tech/resources/SkriptHighlighting.xshd");
            try
            {
                File.Delete(appPath + @"\SkriptHighlighting.xshd");
                await DownloadFileTaskAsync(client, uri, appPath + @"\SkriptHighlighting.xshd");
                Growl.Success("Pomyślnie zaaktualizowano plik podświetlania składni!", "SuccessMsg");
                skEditor.GetMainWindow().GetFileManager().OnTabChanged();
            }
            catch (Exception e)
            {
                MessageBox.Error($"Nie udało się pobrać pliku podświetlania składni!\nMasz połączenie z internetem?\n\n{e.Message}", "Błąd");
            }
        }

        public static async Task DownloadFileTaskAsync(HttpClient client, Uri uri, string FileName)
        {
            using var s = await client.GetStreamAsync(uri);
            using var fs = new FileStream(FileName, FileMode.CreateNew);
            await s.CopyToAsync(fs);
        }

        private void OnKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

            switch (e.Key)
            {
                case Key.D:
                    debugLetters[0] = true;
                    break;
                case Key.E:
                    debugLetters[1] = true;
                    break;
                case Key.B:
                    debugLetters[2] = true;
                    break;
                case Key.U:
                    debugLetters[3] = true;
                    break;
                case Key.G:
                    debugLetters[4] = true;
                    break;
                default:
                    for (int i = 0; i < debugLetters.Length; i++)
                    {
                        debugLetters[i] = false;
                    }
                    break;
            }

            if (debugLetters[0] && debugLetters[1] && debugLetters[2] && debugLetters[3] && debugLetters[4])
            {
                // Copy to clipboard all system info
                string systemInfo = "```\n";
                systemInfo += $"System: {Environment.OSVersion}\n";
                systemInfo += $"Platforma: {Environment.OSVersion.Platform}\n";
                systemInfo += $"64-bit: {Environment.Is64BitOperatingSystem}\n".Replace("True", "Tak").Replace("False", "Nie");
                systemInfo += $"Wersja .NET: {Environment.Version}\n";
                systemInfo += $"Wersja SkEditora+: {MainWindow.version} \n```";

                Clipboard.SetText(systemInfo);


                debugLetters = new bool[5];
            }
        }
    }
}
