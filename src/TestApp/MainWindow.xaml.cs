using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ScriptEngine;
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
            var src = hostedScript.Loader.FromString(txtCode.Text);
            using (var writer = new StringWriter())
            {
                try
                {
                    var moduleWriter = new ScriptEngine.Compiler.ModuleWriter();
                    moduleWriter.Write(writer, src);
                    result.Text = writer.GetStringBuilder().ToString();
                }
                catch (ScriptEngine.Compiler.CompilerException exc)
                {
                    result.Text = exc.Message + "\nLine: " + exc.LineNumber;
                }
                catch (ScriptEngine.Compiler.ParserException exc)
                {
                    result.Text = exc.Message + "\nLine: " + exc.Line;
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
            var src = hostedScript.Loader.FromString(txtCode.Text);
            var process = hostedScript.CreateProcess(host, src);

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

        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Open_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Текстовый файл|*.txt";
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == true)
            {
                using (var fs = new System.IO.StreamReader(dlg.FileName))
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
            dlg.Filter = "Текстовый файл|*.txt";
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
            _output.Text = exc.ToString();
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
