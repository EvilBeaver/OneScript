/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScriptEngine.HostedScript;

namespace TestApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _currentDocPath = "";
        private bool _isModified = false;

        public bool IsModified
        {
            get { return _isModified; }
            set 
            { 
                _isModified = value;
                if (_isModified)
                    this.Title = _currentDocPath + " *";
                else
                    this.Title = _currentDocPath;
            }
        }
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var hostedScript = new HostedScriptEngine();
            hostedScript.Initialize();
            var src = hostedScript.Loader.FromString(txtCode.Text);
            using (var writer = new StringWriter())
            {
                try
                {
                    var moduleWriter = new ScriptEngine.Compiler.ModuleWriter(hostedScript.GetCompilerService());
                    moduleWriter.Write(writer, src);
                    result.Text = writer.GetStringBuilder().ToString();
                }
                catch (Exception exc)
                {
                    result.Text = exc.Message;
                }
            }
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            result.Text = "";
            var sw = new System.Diagnostics.Stopwatch();
            var host = new Host(result);

            var hostedScript = new HostedScriptEngine();
            hostedScript.Initialize();
            var src = hostedScript.Loader.FromString(txtCode.Text);

            Process process = null;
            try
            {
                process = hostedScript.CreateProcess(host, src);
            }
            catch (Exception exc)
            {
                result.Text = exc.Message;
                return;
            }

            result.AppendText("Script started: " + DateTime.Now.ToString() + "\n");
            sw.Start();
            var returnCode = process.Start();
            sw.Stop();
            if (returnCode != 0)
            {
                result.AppendText("\nError detected. Exit code = " + returnCode.ToString());
            }
            result.AppendText("\nScript completed: " + DateTime.Now.ToString());
            result.AppendText("\nDuration: " + sw.Elapsed.ToString());
            
        }
        
        private static string GetFileDialogFilter()
        {
            return "Поддерживаемые файлы|*.os;*.txt|Все файлы|*.*";
        }

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = GetFileDialogFilter();
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == true)
            {
                using (var fs = ScriptEngine.Environment.FileOpener.OpenReader(dlg.FileName))
                {
                    txtCode.Text = fs.ReadToEnd();
                    _currentDocPath = dlg.FileName;
                    this.Title = _currentDocPath;
                }
            }
        }

        private void Save_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFile();
        }

        private bool SaveFile()
        {
            if (_currentDocPath == "")
            {
               return AskForFilenameAndSave();
            }
            else
            {
                DumpCodeToFile(_currentDocPath);
                return true;
            }
        }

        private void SaveAs_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            AskForFilenameAndSave();
        }

        private bool AskForFilenameAndSave()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = GetFileDialogFilter();
            dlg.AddExtension = true;
            dlg.DefaultExt = ".os";
            if (!String.IsNullOrEmpty(_currentDocPath))
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(_currentDocPath);
                dlg.FileName = System.IO.Path.GetFileName(_currentDocPath);
            }

            if (dlg.ShowDialog() == true)
            {
                var filename = dlg.FileName;
                DumpCodeToFile(filename);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void DumpCodeToFile(string filename)
        {
            var enc = new UTF8Encoding(true);
            using (var fs = new System.IO.StreamWriter(filename, false, enc))
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsModified)
            {
                var answer = MessageBox.Show("Сохранить изменения?", "TestApp", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (answer == MessageBoxResult.Cancel)
                    e.Cancel = true;
                else if (answer == MessageBoxResult.Yes)
                    e.Cancel = !SaveFile();
            }
        }

    }

    class Host : IHostApplication
    {
        private TextBox _output;

        public Host(TextBox output)
        {
            _output = output;
        }

        #region IHostApplication Members

        public void Echo(string str)
        {
            _output.AppendText(str + '\n');
        }

        public void ShowExceptionInfo(Exception exc)
        {
            _output.Text = exc.Message;
        }

        public bool InputString(out string result, int maxLen)
        {
            result = "строка введена";
            return true;
        }

        public string[] GetCommandLineArguments()
        {
            return new string[]
            {
                "привет",
                "мы тестовые аргументы"
            };
        }

        #endregion
    }
}
