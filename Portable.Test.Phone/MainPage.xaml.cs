using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using NiL.JS;
using NiL.JS.Core;
using NiL.JS.Core.TypeProxing;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=391641

namespace Portable.Test.Phone
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private sealed class Tester : INotifyPropertyChanged
        {
            private class Logger
            {
                private Tester tester;

                public Logger(Tester tester)
                {
                    this.tester = tester;
                }

                public void log(Arguments arguments)
                {
                    string str = null;
                    for (var i = 0; i < arguments.Length; i++)
                    {
                        if (i > 0)
                            str += " ";
                        var r = arguments[i].ToString();
                        str += r;
                    }
                    tester.Messages.Add(str);
                }
            }

            private static string staCode;

            private Logger logger;
            public ObservableCollection<string> Messages { get; private set; }

            public int Passed { get; set; }

            public int Failed { get; set; }

            public Tester()
            {
                Messages = new ObservableCollection<string>();
                logger = new Logger(this);
            }

            public async void BeginTest()
            {
                if (staCode == null)
                {
                    var staFile = enumerateFiles(Package.Current.InstalledLocation).First(x => x.DisplayName == "sta");
                    staCode = new StreamReader((await staFile.OpenReadAsync()).AsStream(0)).ReadToEnd();
                }
                (Application.Current as App).Activation += activation;

                FolderPicker picker = new FolderPicker();
                picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                picker.FileTypeFilter.Add(".js");
                picker.PickFolderAndContinue();
            }

            private async void activation(Windows.ApplicationModel.Activation.IActivatedEventArgs obj)
            {
                (Application.Current as App).Activation -= activation;

                var ea = obj as FolderPickerContinuationEventArgs;
                var files = enumerateFiles(ea.Folder);
                foreach (var file in files)
                    test(new StreamReader((await file.OpenReadAsync()).AsStream(0)).ReadToEnd());
            }

            private void test(string code)
            {
                bool pass = true;
                bool negative = code.IndexOf("@negative") != -1;
                try
                {

                    Context.RefreshGlobalContext();
                    var s = new Script(staCode); // инициализация
                    s.Context.DefineVariable("console").Assign(TypeProxy.Proxy(logger));
                    s.Invoke();
                    try
                    {
                        s.Context.Eval(code, true);
                    }
                    finally
                    {
                        pass ^= negative;
                    }
                }
                catch (JSException e)
                {
                    pass = negative;
                    if (!pass)
                        logger.log(new Arguments { e.Message });
                }
                catch (Exception e)
                {
                    logger.log(new Arguments { e.ToString() });
                    pass = false;
                }
                if (pass)
                {
                    Passed++;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Passed"));
                }
                else
                {
                    Failed++;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Failed"));
                }
            }

            private static IEnumerable<StorageFile> enumerateFiles(StorageFolder folder)
            {
                var folders = folder.GetFoldersAsync();
                folders.AsTask().Wait();
                foreach (var subfolder in folders.GetResults())
                {
                    foreach (var file in enumerateFiles(subfolder))
                        yield return file;
                }
                var files = folder.GetFilesAsync();
                files.AsTask().Wait();
                foreach (var file in files.GetResults())
                    yield return file;
            }

            #region Члены INotifyPropertyChanged

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion
        }

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Вызывается перед отображением этой страницы во фрейме.
        /// </summary>
        /// <param name="e">Данные события, описывающие, каким образом была достигнута эта страница.
        /// Этот параметр обычно используется для настройки страницы.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Подготовьте здесь страницу для отображения.

            // TODO: Если приложение содержит несколько страниц, обеспечьте
            // обработку нажатия аппаратной кнопки "Назад", выполнив регистрацию на
            // событие Windows.Phone.UI.Input.HardwareButtons.BackPressed.
            // Если вы используете NavigationHelper, предоставляемый некоторыми шаблонами,
            // данное событие обрабатывается для вас.
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var tester = new Tester();
            DataContext = tester;
            tester.BeginTest();
        }
    }
}
