namespace Ftp
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using qTrace.Publishing.Contracts;

    [Export(typeof(IPublisher))]
    public class Ftp : IPublisher
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
                    SmallIconUri = "/Ftp;component/ftp.png",
                    DisplayName = "FTP Publisher",
                    HelpUrl = "http://www.qasymphony.com/building-a-custom-defect-submitter-for-qtrace.html",
                    CustomSettingFields = new List<SettingField> {
                            new SettingField {
                                    DisplayName = "Transfer Mode",
                                    Name = "Mode",
                                    FieldType = SettingFieldType.Dropdown,
                                    AcceptedValues = new List<SettingAcceptedValue> {
                                        new SettingAcceptedValue {Id = "Active", Value = "Active"},
                                        new SettingAcceptedValue {Id = "Passive", Value = "Passive"}
                                    }
                            },
                            new SettingField {
                                    DisplayName = "Enable SSL",
                                    Name = "Ssl",
                                    FieldType = SettingFieldType.Checkbox
                            }
                    }
            };
        }

        public VerificationResult Verify(PublisherSettings settings)
        {
            Context.ShowProgressIndicator(message: "Connecting to FTP...");
            try {
                var ftpRequest = CreateRequest(settings.Url, WebRequestMethods.Ftp.ListDirectory, settings);
                using (ftpRequest.GetResponse()) {}
                return new VerificationResult();
            }
            finally {
                Context.HideProgressIndicator();
            }
        }

        public PublishingResult Publish(PublisherSettings settings, PublishingRecord record, AttachmentFunc createAttachments)
        {
            Context.ShowProgressIndicator(message: "Publishing to FTP...");
            try {
                var tasks = from attachment in createAttachments()
                            select Task.Factory.StartNew(() => Upload(settings, attachment));
                Task.WaitAll(tasks.ToArray());
                return new PublishingResult();
            }
            catch (AggregateException e) {
                throw new PublishingException(e.InnerExceptions[0].Message, e.InnerExceptions[0]);
            }
            finally {
                Context.HideProgressIndicator();
            }
        }

        private void Upload(PublisherSettings settings, string filePath)
        {
            var uri = settings.Url +
                      (settings.Url.EndsWith("/") ? string.Empty : "/") +
                      Path.GetFileName(filePath);
            var ftpRequest = CreateRequest(uri, WebRequestMethods.Ftp.UploadFile, settings);
            using (Stream writer = ftpRequest.GetRequestStream()) {
                var fileContent = File.ReadAllBytes(filePath);
                writer.Write(fileContent, 0, fileContent.Length);
            }
        }

        private static FtpWebRequest CreateRequest(
            string uri, string method, PublisherSettings settings)
        {
            var ftpRequest = (FtpWebRequest) WebRequest.Create(uri);
            ftpRequest.Credentials = new NetworkCredential(settings.UserName, settings.Password);
            ftpRequest.UsePassive = settings.CustomSettingsFieldsValues["Mode"] == "Passive";
            ftpRequest.EnableSsl = bool.Parse(settings.CustomSettingsFieldsValues["Ssl"]);
            ftpRequest.Timeout = settings.TimeoutInMillis;
            ftpRequest.Method = method;
            return ftpRequest;
        }
    }
}
