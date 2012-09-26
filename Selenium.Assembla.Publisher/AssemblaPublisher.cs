namespace Selenium.Assembla.Publisher
{
    using System.Collections.ObjectModel;
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;
    using qTrace.Publishing.Contracts;
    using System.IO;
    using System;
    using System.Collections.Generic;

    [Export(typeof (IPublisher))]
    public class AssemblaPublisher : IPublisher
    {
        private IPublishingContext _context;
        private readonly object _contextLock = new object();

        public IPublishingContext Context
        {
            get { lock (_contextLock) return _context; }
            set { lock (_contextLock) _context = value; }
        }

        public PublisherInfo GetMetadata()
        {
            return new PublisherInfo {
                SmallIconUri = "/Selenium.Assembla.Publisher;component/icon.png",
                DisplayName = "Selenium-Assembla Publisher 1"
            };
        }

        public VerificationResult Verify(PublisherSettings settings)
        {
            Context.ShowProgressIndicator(message: "Connecting to Assembla...");
            try {
                var request = AssemblaUtils.CreateLoadSpacesRequest(settings);
                using (request.GetResponse())
                    return new VerificationResult();
            }
            finally {
                Context.HideProgressIndicator();
            }
        }

        public PublishingResult Publish(
            PublisherSettings settings,
            PublishingRecord record,
            AttachmentFunc createAttachments)
        {            
            Space selectedSpace = null;
            bool uploadAttachmentToDropbox = false;
            
            var selectedSpaceLock = new object();

            // open submission screen
            Context.ExecuteOnUIThread(() =>
            {
                var view = new SpacePromptView
                {
                    IsConnectedToDropbox = Context.IsDropboxConnected,
                    Spaces = new ObservableCollection<Space>(AssemblaUtils.LoadSpaces(settings))
                };
                lock (selectedSpaceLock)
                {
                    if (view.ShowDialog() == true)
                    {
                        selectedSpace = view.SelectedSpace;
                        uploadAttachmentToDropbox = view.UploadAttachmentToDropbox;
                    }
                }
            });
            if (selectedSpace == null)
                return null;

            Context.ShowProgressIndicator(message: "Starting...");
            try {
                var attachments = new Dictionary<string, bool>();

                // Generate the attachments before starting background thread                    
                if (uploadAttachmentToDropbox)
                {
                    foreach (var file in ExportAttachmentToDropbox(createAttachments)) { 
                        attachments.Add(file, true); 
                    }                    
                }
                else {                    
                    foreach(var file in createAttachments()) { 
                        attachments.Add(file, false); 
                    }
                }
                // Automate browser in a worker thread
                Task.Factory.StartNew(() => AssemblaUtils.Automate(settings, record, attachments, selectedSpace.Id));                
            }
            finally {
                Context.HideProgressIndicator();
            }
            return null;
        }        

        private string[] ExportAttachmentToDropbox(AttachmentFunc createAttachments)
        {
            var uploadedAttachments = new List<string>();
            // generate file(s) to temp folder first
            string[] files = createAttachments(CreateTempFolder(), null, null);

            // create a unique folder name to stored uploading file(s)
            var folder = Guid.NewGuid().ToString();
            if (files != null && files.Length > 0) {
                try {
                    foreach (var file in files) {
                        var fileUrl = Context.UploadToDropbox(file, folder);
                        if (!string.IsNullOrEmpty(fileUrl)) {
                            uploadedAttachments.Add(fileUrl);
                        }
                    }
                }
                catch (Exception ex) {
                    Context.Error("Error while trying to upload files to Dropbox: " + ex.Message);
                }
            }
            return uploadedAttachments.ToArray();
        }

        private static string CreateTempFolder()
        {
            string tempFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                             "Temp");
            var path = Path.Combine(tempFolder, Guid.NewGuid().ToString());
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return path;
        }
    }
}