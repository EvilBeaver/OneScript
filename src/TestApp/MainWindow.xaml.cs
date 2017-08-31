/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Microsoft.Win32;

using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library;

using Application = System.Windows.Forms.Application;
using Process = ScriptEngine.HostedScript.Process;

namespace TestApp
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private string _currentDocPath = "";

        private bool _isModified;

        // Определяем путь к AppData\Local
        private readonly string _localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        public MainWindow()
        {
            InitializeComponent();
        }

        public bool IsModified
        {
            get => _isModified;
            set
            {
                _isModified = value;
                if (_isModified)
                    Title = _currentDocPath + " *";
                else
                    Title = _currentDocPath;
            }
        }

        private void SaveLastCode()
        {
            var filename = Path.Combine(_localPath, "TestApp.os");
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                // первой строкой запишем имя открытого файла
                writer.Write("//"); // знаки комментария, чтобы сохранить код правильным
                writer.WriteLine(_currentDocPath);

                // второй строкой - признак изменённости
                writer.Write("//");
                writer.WriteLine(_isModified);

                args.Text = args.Text.TrimEnd('\r', '\n');

                // запишем аргументы командной строки
                writer.Write("//");
                writer.WriteLine(args.LineCount);

                for (var i = 0; i < args.LineCount; ++i)
                {
                    var s = args.GetLineText(i).TrimEnd('\r', '\n');
                    writer.Write("//");
                    writer.WriteLine(s);
                }

                // и потом сам код
                writer.Write(txtCode.Text);
            }
        }

        private void RestoreLastCode()
        {
            var filename = Path.Combine(_localPath, "TestApp.os");
            if (!File.Exists(filename))
                return;

            using (var reader = new StreamReader(filename, Encoding.UTF8))
            {
                var lastOpened = reader.ReadLine()?.Substring(2);
                var wasModified = reader.ReadLine()?.Substring(2);

                var argsline = reader.ReadLine()?.Substring(2);
                var argstail = ""; // если не распознали строку с параметром, здесь будет "хвост" нераспознанной строки
                var argscount = 0;

                try
                {
                    argscount = int.Parse(argsline);
                }
                catch (Exception)
                {
                    // файл битый. видимо, старой версии или что-нибудь вроде того
                    argstail = argsline + "\n";
                }

                args.Text = "";
                for (var i = 0; i < argscount; ++i)
                {
                    var param = reader.ReadLine()?.Substring(2);
                    args.Text += param + "\n";
                }

                txtCode.Text = argstail + reader.ReadToEnd();

                if (lastOpened != "")
                {
                    // был открыт какой-то файл, сделаем вид, что открыли его
                    _currentDocPath = lastOpened;
                    IsModified = wasModified == "True";
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var hostedScript = new HostedScriptEngine
            {
                CustomConfig = CustomConfigPath(_currentDocPath)
            };
            hostedScript.Initialize();
            var src = hostedScript.Loader.FromString(txtCode.Text);
            using (var writer = new StringWriter())
            {
                try
                {
                    var moduleWriter = new ModuleWriter(hostedScript.GetCompilerService());
                    moduleWriter.Write(writer, src);
                    result.Text = writer.GetStringBuilder().ToString();
                }
                catch (Exception exc)
                {
                    result.Text = exc.Message;
                }
            }
        }

        private void Run_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            Button_Click_1(sender, null);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            SaveLastCode(); // Сохраним набранный текст на случай зависания или вылета

            result.Text = "";
            var sw = new Stopwatch();

            var lArgs = new List<string>();
            for (var i = 0; i < args.LineCount; i++)
            {
                var s = args.GetLineText(i);
                if (s.IndexOf('#') != 0)
                    lArgs.Add(s.Trim());
            }

            var host = new Host(result, lArgs.ToArray());
            SystemLogger.SetWriter(host);
            var hostedScript = new HostedScriptEngine
            {
                CustomConfig = CustomConfigPath(_currentDocPath)
            };
            SetEncodingFromConfig(hostedScript);

            var src = new EditedFileSource(txtCode.Text, _currentDocPath);

            Process process;
            try
            {
                process = hostedScript.CreateProcess(host, src);
            }
            catch (Exception exc)
            {
                host.Echo(exc.Message);
                return;
            }

            result.AppendText("Script started: " + DateTime.Now + "\n");
            sw.Start();
            var returnCode = process.Start();
            sw.Stop();
            if (returnCode != 0)
                result.AppendText("\nError detected. Exit code = " + returnCode);
            result.AppendText("\nScript completed: " + DateTime.Now);
            result.AppendText("\nDuration: " + sw.Elapsed);
        }

        public static string CustomConfigPath(string scriptPath)
        {
            if (scriptPath == null || !File.Exists(scriptPath))
                return null;

            var dir = Path.GetDirectoryName(scriptPath);
            var cfgPath = Path.Combine(dir, HostedScriptEngine.ConfigFileName);
            return File.Exists(cfgPath) ? cfgPath : null;
        }

        private static string GetFileDialogFilter()
        {
            return "Поддерживаемые файлы|*.os;*.txt|Все файлы|*.*";
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewScript_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            if (!SaveModified())
                return;

            _currentDocPath = "";
            txtCode.Text = "";
            IsModified = false;
        }

        private void Open_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = GetFileDialogFilter(),
                Multiselect = false
            };
            if (dlg.ShowDialog() != true)
                return;

            var hostedScript = new HostedScriptEngine
            {
                CustomConfig = CustomConfigPath(dlg.FileName)
            };
            SetEncodingFromConfig(hostedScript);

            using (var fs = FileOpener.OpenReader(dlg.FileName))
            {
                txtCode.Text = fs.ReadToEnd();
                _currentDocPath = dlg.FileName;
                Title = _currentDocPath;
            }
        }

        private void Save_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFile();
        }

        private static void SetEncodingFromConfig(HostedScriptEngine engine)
        {
            var cfg = engine.GetWorkingConfig();

            var openerEncoding = cfg["encoding.script"];
            if (!string.IsNullOrWhiteSpace(openerEncoding) && StringComparer.InvariantCultureIgnoreCase.Compare(openerEncoding, "default") != 0)
                engine.Loader.ReaderEncoding = Encoding.GetEncoding(openerEncoding);
        }

        private bool SaveFile()
        {
            if (_currentDocPath == "")
                return AskForFilenameAndSave();

            DumpCodeToFile(_currentDocPath);
            return true;
        }

        private void SaveAs_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            AskForFilenameAndSave();
        }

        private bool AskForFilenameAndSave()
        {
            var dlg = new SaveFileDialog
            {
                Filter = GetFileDialogFilter(),
                AddExtension = true,
                DefaultExt = ".os"
            };
            if (!string.IsNullOrEmpty(_currentDocPath))
            {
                dlg.InitialDirectory = Path.GetDirectoryName(_currentDocPath);
                dlg.FileName = Path.GetFileName(_currentDocPath);
            }

            if (dlg.ShowDialog() != true)
                return false;

            var filename = dlg.FileName;
            DumpCodeToFile(filename);
            return true;
        }

        private void DumpCodeToFile(string filename)
        {
            var enc = new UTF8Encoding(true);
            using (var fs = new StreamWriter(filename, false, enc))
            {
                fs.Write(txtCode.Text);
                _currentDocPath = filename;
                IsModified = false;
            }
        }

        private void txtCode_TextChanged(object sender, RoutedEventArgs e)
        {
            IsModified = true;
        }

        private bool SaveModified()
        {
            if (!IsModified)
                return true;

            var answer = MessageBox.Show("Сохранить изменения?", "TestApp", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (answer == MessageBoxResult.Cancel)
                return false;

            if (answer == MessageBoxResult.Yes)
                return !SaveFile();

            return true;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (SaveModified())
                SaveLastCode();
            else
                e.Cancel = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RestoreLastCode();
            
            txtCode.editor.Focus();
        }

        private void FocusParamsWindow(object sender, ExecutedRoutedEventArgs e)
        {
            args.Focus();
        }

        private void FocusCodeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            txtCode.editor.Focus();
        }

        private void ToggleCmdLine_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SetCmdLineVisibility();
            e.Handled = true;
        }

        private void SetCmdLineVisibility()
        {
            var showCmdlineArgs = toggleArgs.IsChecked != null && (bool) toggleArgs.IsChecked;

            if (showCmdlineArgs)
            {
                verticalSplitter.Visibility = Visibility.Visible;
                args.Visibility = Visibility.Visible;
            }
            else
            {
                verticalSplitter.Visibility = Visibility.Collapsed;
                args.Visibility = Visibility.Collapsed;
            }
        }
    }

    internal class Host : IHostApplication, ISystemLogWriter
    {
        private readonly string[] _arguments;

        private readonly TextBox _output;

        public Host(TextBox output, string[] arguments = null)
        {
            _output = output;
            _arguments = arguments ?? new string[0];
        }

        public void Write(string text)
        {
            Echo(text);
        }

        #region IHostApplication Members

        public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            _output.AppendText(str + '\n');
            _output.ScrollToEnd();
            Application.DoEvents();
        }

        public void ShowExceptionInfo(Exception exc)
        {
            Echo(exc.Message);
        }

        public bool InputString(out string result, int maxLen)
        {
            result = "строка введена";
            return true;
        }

        public string[] GetCommandLineArguments()
        {
            return _arguments;
        }

        #endregion
    }
}