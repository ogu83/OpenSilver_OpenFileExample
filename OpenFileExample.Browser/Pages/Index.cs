using DotNetForHtml5;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using OpenFileExample.Browser.Interop;
using System.IO;
using System.Threading.Tasks;

namespace OpenFileExample.Browser.Pages
{
    [Route("/")]
    public class Index : ComponentBase
    {

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<InputFile>(0);
            builder.AddAttribute(1, "OnChange", global::Microsoft.AspNetCore.Components.CompilerServices.RuntimeHelpers.TypeCheck<EventCallback<InputFileChangeEventArgs>>(EventCallback.Factory.Create<InputFileChangeEventArgs>(this,

                              LoadFiles)));
            builder.AddAttribute(2, "multiple", true);
            builder.AddAttribute(3, "id", "fileuploader");
            builder.AddAttribute(4, "style", "display:none");
            builder.CloseComponent();
        }

        private async Task LoadFiles(InputFileChangeEventArgs e)
        {

            Utility.TempFiles.Clear();
            Utility.FileIsLoading = true;

            foreach (IBrowserFile file in e.GetMultipleFiles())
            {
                Stream fileStream = file.OpenReadStream(Utility.MaxFileSize);
                MemoryStream ms = new MemoryStream();
                await fileStream.CopyToAsync(ms);
                fileStream.Dispose();
                ms.Position = 0;
                FileInfo2 fileInfo = new FileInfo2(file, ms);
                Utility.TempFiles.Add(fileInfo);
            }

            Utility.FileIsLoading = false;
        }


        protected override void OnInitialized()
        {
            base.OnInitialized();
            Cshtml5Initializer.Initialize(new UnmarshalledJavaScriptExecutionHandler(JSRuntime));
            Program.RunApplication();
        }



        [Inject]
        private IJSRuntime JSRuntime { get; set; }
    }
}