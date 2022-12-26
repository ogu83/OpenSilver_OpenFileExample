using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace OpenFileExample
{
    public partial class BorwseFile : UserControl
    {
        private string fileExtensionFilter;
        public BorwseFile()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //OpenFileDialog fd = new OpenFileDialog();
            //fd.Multiselect = true;
            //if (!(bool)fd.ShowDialog())
            //    return;

             OpenFile();
        }


        public  void OpenFile()
        {
 
            OpenFileDialog d = new OpenFileDialog();
 
            d.ShowDialog();
        }
        private void OpenAndReadFile()
        {

            OpenFileDialog.UploadFiles(this.fileExtensionFilter, false, ResultKind.DataURL, (res) =>
                {
                    if (res.Count > 0)
                    {

                        string FileName = res[0].name;
                        if (!FileName.Contains('.'))
                        {
                            throw new ArgumentException("missing extension");
                        }

                        var extension = FileName.Substring(FileName.LastIndexOf('.') + 1).ToLower();


                        string s = res[0].text;

                        // Cut useless part from base64 string
                        s = s.Substring(s.IndexOf("base64") + 7);

                        byte[] data = System.Convert.FromBase64String(s);

                        DoSomethingWithDataHere(data);
                    }
                });

        }

        private void DoSomethingWithDataHere(byte[] data)
        {
            if (data != null)
                Console.WriteLine("data received");
            else
                Console.WriteLine("data null");

        }
    }



    public class OpenFileDialog
    {
        public event EventHandler<FileOpenedEventArgs> FileOpened;
        public event EventHandler FileOpenFinished;
        public event EventHandler FileOpenCanceled;
        private object inputElement;

        private ResultKind _resultKind;
        private string _resultKindStr;
        public ResultKind ResultKind
        {
            get { return _resultKind; }
            set
            {
                _resultKind = value;
                _resultKindStr = value.ToString();
            }
        }

        public OpenFileDialog()
        {
            inputElement = OpenSilver.Interop.ExecuteJavaScript(@"
                var el = document.createElement('input');
                el.setAttribute('type', 'file');
                el.setAttribute('id', 'file');
                el.setAttribute('name', 'file');
                el.setAttribute('multiple','true');
                el.setAttribute('_bl_2','true');
                el.style.display = 'none';
                document.body.appendChild(el);");
            ResultKind = ResultKind.Text;
        }

 

        void AddListener()
        {
            Action<object, string> onFileOpened = (result, name) =>
            {
                if (this.FileOpened != null)
                {
                    this.FileOpened(this, new FileOpenedEventArgs(result, name, this.ResultKind));
                }
            };

            Action onFileOpenFinished = () =>
            {
                if (this.FileOpenFinished != null)
                {
                    this.FileOpenFinished(this, new EventArgs());
                }
            };

            Action onFileOpenCanceled = () =>
            {
                if (this.FileOpenCanceled != null)
                {
                    this.FileOpenCanceled(this, new EventArgs());
                }
            };

            // Listen to the "change" property of the "input" element, and call the callback:
           dynamic dataForm= OpenSilver.Interop.ExecuteJavaScript(@"
                $0.addEventListener(""click"", function(e) {
                    document.body.onfocus = function() {
                        document.body.onfocus = null;
                        setTimeout(() => { 
                            if (document.getElementById('file').value.length) {
                            }
                            //else
                            //{
                            //    var cancelCallback = $3;
                            //    cancelCallback();
                            //}
                            document.getElementById('file').remove();
                        }, 1000);
                    }
                });

                $0.addEventListener(""change"", function(e) {
        
                    var files = document.getElementById(""file"").files;
                      let formData = new FormData();
   for (const file of files) {
        formData.append(""file"", file) 
    }
                   //formData.append(""file"", file);
                    fetch('https://localhost:5001/upload', {method: ""POST"", body: formData});
            
                });", inputElement);
        }



        void SetFilter(string filter)
        {
            if (String.IsNullOrEmpty(filter))
            {
                return;
            }

            string[] splitted = filter.Split('|');
            List<string> itemsKept = new List<string>();
            if (splitted.Length == 1)
            {
                itemsKept.Add(splitted[0]);
            }
            else
            {
                for (int i = 1; i < splitted.Length; i += 2)
                {
                    itemsKept.Add(splitted[i]);
                }
            }
            string filtersInHtml5 = String.Join(",", itemsKept).Replace("*", "").Replace(";", ",");

            // Apply the filter:
            if (!string.IsNullOrWhiteSpace(filtersInHtml5))
            {
                OpenSilver.Interop.ExecuteJavaScript(@"$0.accept = $1", inputElement, filtersInHtml5);
            }
            else
            {
                OpenSilver.Interop.ExecuteJavaScript(@"$0.accept = """"", inputElement);
            }
        }

        private bool _multiselect = false;
        public bool Multiselect
        {
            get { return _multiselect; }
            set
            {
                _multiselect = value;

                if (_multiselect)
                {
                    OpenSilver.Interop.ExecuteJavaScript(@"$0.setAttribute('multiple', 'multiple');", inputElement);
                }
            }
        }

        private string _filter;
        public string Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                SetFilter(_filter);
            }
        }

        public bool ShowDialog()
        {
            //AddListener();
            OpenSilver.Interop.ExecuteJavaScript("document.getElementById('fileuploader').click();");
            return true;
        }

        public static void UploadFiles(string filter, bool multiselect, ResultKind kind, Action<List<FileReadResult>> callback)
        {
            List<FileReadResult> result = new List<FileReadResult>();

            OpenFileDialog d = new OpenFileDialog();
            if (!String.IsNullOrEmpty(filter))
                d.Filter = filter;

            d.Multiselect = multiselect;
            d.ResultKind = kind;

            d.FileOpened += (s, e) =>
            {
                string text = e.Text;
                if (kind == ResultKind.DataURL)
                {
                    text = e.DataURL;
                }

                var res = new FileReadResult() { name = e.Name, text = text };
                result.Add(res);
            };

            d.FileOpenFinished += (s, e) =>
            {
                callback(result);
            };

            d.FileOpenCanceled += (s, e) =>
            {
                callback(result);
            };

            d.ShowDialog();
        }
    }

    public class FileOpenedEventArgs : EventArgs
    {
        /// <summary>
        /// Only available if the property "ResultKind" was set to "Text".
        /// </summary>
        public readonly string Text;

        /// <summary>
        /// Only available if the property "ResultKind" was set to "DataURL".
        /// </summary>
        public readonly string DataURL;

        public string Name;

        public FileOpenedEventArgs(object result, string name, ResultKind resultKind)
        {
            Name = name;
            if (resultKind == ResultKind.Text)
            {
                this.Text = (result ?? "").ToString();
            }
            else if (resultKind == ResultKind.DataURL)
            {
                this.DataURL = (result ?? "").ToString();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }

    public enum ResultKind
    {
        Text, DataURL
    }

    public struct FileReadResult
    {
        public string name { get; set; }
        public string text { get; set; }
    }
}
