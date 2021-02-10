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
using System.Collections.Generic;
using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Collections;
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.HostedScript.Extensions;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.Hosting;

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
#if NETCOREAPP
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
        }

        // Определяем путь к AppData\Local
        private string localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

        private void SaveLastCode()
        {
            string filename = Path.Combine(localPath, "TestApp.os");
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                // первой строкой запишем имя открытого файла
                writer.Write("//");  // знаки комментария, чтобы сохранить код правильным
                writer.WriteLine(_currentDocPath);
                // второй строкой - признак изменённости
                writer.Write("//");
                writer.WriteLine(_isModified);

                args.Text = args.Text.TrimEnd('\r', '\n');

                // запишем аргументы командной строки
                writer.Write("//");
                writer.WriteLine(args.LineCount);

                for (var i = 0; i < args.LineCount; ++i )
                {
                    string s = args.GetLineText(i).TrimEnd('\r', '\n');
                    writer.Write("//");
                    writer.WriteLine(s);
                }

                // и потом сам код
                writer.Write(txtCode.Text);
            }
        }

        private void RestoreLastCode()
        {
            string filename = Path.Combine(localPath, "TestApp.os");
            if (!File.Exists(filename))
            {
                return;
            }

            using (var reader = new StreamReader(filename, Encoding.UTF8))
            {
                string lastOpened = reader.ReadLine().Substring(2);
                string wasModified = reader.ReadLine().Substring(2);

                string argsline = reader.ReadLine().Substring(2);
                string argstail = ""; // если не распознали строку с параметром, здесь будет "хвост" нераспознанной строки
                int argscount = 0;

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
                for (int i = 0; i < argscount; ++i )
                {
                    string param = reader.ReadLine().Substring(2);
                    args.Text += param + "\n";
                }

                txtCode.Text = argstail + reader.ReadToEnd();

                if (lastOpened != "")
                {
                    // был открыт какой-то файл, сделаем вид, что открыли его
                    _currentDocPath = lastOpened;
                    IsModified = (wasModified == "True");
                }

            }
        }

        private HostedScriptEngine CreateEngine()
        {
            var builder = DefaultEngineBuilder
                .Create()
                .SetDefaultOptions()
                .SetupEnvironment(e => e.AddStandardLibrary())
                .SetupConfiguration(x =>
                {
                    x.UseSystemConfigFile()
                     .UseEntrypointConfigFile(_currentDocPath);
                });

            builder.UseFileSystemLibraries();
            var engine = builder.Build();
            var mainEngine = new HostedScriptEngine(engine);

            return mainEngine;
        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var hostedScript = CreateEngine();
            
            hostedScript.Initialize();
            var src = hostedScript.Loader.FromString(txtCode.Text);
            using (var writer = new StringWriter())
            {
                try
                {
                    var cs = hostedScript.GetCompilerService();
                    if(GenerateExtraCode.IsChecked)
                        cs.ProduceExtraCode |= CodeGenerationFlags.CodeStatistics;
                    if(GenerateDebugCode.IsChecked)
                        cs.ProduceExtraCode |= CodeGenerationFlags.DebugCode;

                    var moduleWriter = new ScriptEngine.Compiler.ModuleWriter(cs);
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
            var sw = new System.Diagnostics.Stopwatch();

            List<string> l_args = new List<string>();
            for (var i = 0; i < args.LineCount; i++)
            {
                string s = args.GetLineText(i);
                if (s.IndexOf('#') != 0)
                    l_args.Add(s.Trim());
            }

            var host = new Host(result, l_args.ToArray());
            SystemLogger.SetWriter(host);
            var hostedScript = CreateEngine();
            SetEncodingFromConfig(hostedScript);

            var src = new EditedFileSource(txtCode.Text, _currentDocPath);

            Process process = null;
            try
            {
                process = hostedScript.CreateProcess(host, src);
            }
            catch (Exception exc)
            {
                host.Echo(exc.Message);
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
            if (!SaveModified())
                return;

            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = GetFileDialogFilter();
            dlg.Multiselect = false;
            if (dlg.ShowDialog() == true  )
            {
                _currentDocPath = dlg.FileName;
                var hostedScript = CreateEngine();
                SetEncodingFromConfig(hostedScript);

                using (var fs = FileOpener.OpenReader(dlg.FileName))
                {
                    txtCode.Text = fs.ReadToEnd();
                    this.Title = _currentDocPath;
                }
            }
        }

        private void Save_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFile();
        }

        private void SetEncodingFromConfig(HostedScriptEngine engine)
        {
            var cfg = engine.GetWorkingConfig();

            string openerEncoding = cfg["encoding.script"];
            if (!String.IsNullOrWhiteSpace(openerEncoding) && StringComparer.InvariantCultureIgnoreCase.Compare(openerEncoding, "default") != 0)
            {
                engine.Loader.ReaderEncoding = Encoding.GetEncoding(openerEncoding);
            }
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

            if (dlg.ShowDialog() == true )
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

        private bool SaveModified()
        {
            if (!IsModified)
                return true;

            var answer = MessageBox.Show("Сохранить изменения?", "TestApp", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (answer == MessageBoxResult.Cancel)
                return false;
            else if (answer == MessageBoxResult.Yes)
                return SaveFile();

            return true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (SaveModified())
            {
                SaveLastCode();
            }
            else
            {
                e.Cancel = true;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RestoreLastCode();
            //SetCmdLineVisibility();
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
            var showCmdlineArgs = (bool)toggleArgs.IsChecked;

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

    class Host : IHostApplication, ISystemLogWriter
    {
        private TextBox _output;
        private string[] _arguments;

        public Host(TextBox output, string [] arguments = null)
        {
            _output = output;
            if (arguments == null)
                _arguments = new string[0];
            else 
                _arguments = arguments;
        }

        #region IHostApplication Members

        public void Echo(string str, MessageStatusEnum status = MessageStatusEnum.Ordinary)
        {
            _output.AppendText(str + '\n');
            _output.ScrollToEnd();
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

        public void Write(string text)
        {
            Echo(text);
        }
    }
}
