namespace Selenium.Assembla.ServiceAdapter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Xml.Linq;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Firefox;
    using qTrace.Publishing.Contracts;
    using qTrace.Publishing.Contracts.Adapter;

    class AssemblaUtils
    {
        public static HttpWebRequest CreateLoadSpacesRequest(PublisherSettings settings)
        {
            string url = string.Format("{0}/spaces/my_spaces", settings.Url);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = settings.TimeoutInMillis;
            request.Accept = "application/xml";
            request.ContentType = "application/xml";
            request.Credentials = new NetworkCredential(
                settings.UserName,
                settings.Password);
            return request;
        }

        public static List<Space> LoadSpaces(PublisherSettings settings)
        {
            var request = CreateLoadSpacesRequest(settings);
            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream()) {
                var document = XDocument.Load(responseStream);
                return (from space in document.Root.Elements("space")
                        select new Space {
                            Id = space.Element("wiki-name").Value,
                            Name = space.Element("name").Value
                        }).ToList();
            }
        }        

        public static void Automate(PublisherSettings settings,
            PublishingRecord record,
            List<Attachment> attachments,
            string spaceId)
        {
            // Launch a FireFox instance
            var driver = new FirefoxDriver();

            // How long WebDriver will wait for an element to exist in the DOM before timeout
            driver.Manage().Timeouts().ImplicitlyWait(new TimeSpan(0, 0, 20));

            // Go to login page, fill in credentials and click Login
            driver.Navigate().GoToUrl(string.Format("{0}/login", settings.Url));
            driver.FindElement(By.Name("user[login]")).SendKeys(settings.UserName);
            driver.FindElement(By.Name("user[password]")).SendKeys(settings.Password);
            driver.FindElement(By.Name("commit")).Click();

            // Go to ticket page, fill in summary and description
            driver.Navigate().GoToUrl(string.Format("{0}/spaces/{1}/tickets/new",
                                            settings.Url, spaceId));
            driver.FindElement(By.Name("ticket[summary]")).SendKeys(record.Title);

            // export link-only attachments to string and append to defect summary
            string summary = record.Summary + ToEmbeddedLinks(attachments.Where(x => x.AttachAsLink).Select(x => x.Location).ToArray());
            driver.FindElement(By.Name("ticket[description]")).SendKeys(ReplaceMarkers(summary));

            // Add attachments
            int i = 0;
            foreach (var attachment in attachments.Where(x => x.AttachAsLink == false))
            {

                // Click the "Add attachments" link
                driver.FindElement(By.ClassName("item-attachment")).Click();

                // Fill file path to the file upload control
                driver.FindElement(By.Name(string.Format("ticket[new_attachment_attributes][{0}][file]", ++i)))
                        .SendKeys(attachment.Location);
            }
        }

        private static string ToEmbeddedLinks(string[] linkOnlyAttachments)
        {
            if (linkOnlyAttachments != null && linkOnlyAttachments.Length > 0)
            {
                var linksToAttachments = new StringBuilder();
                linksToAttachments.AppendFormat("\r\n\r\n{0} Embedded links:", "#");
                foreach (var attachment in linkOnlyAttachments)
                {
                    var hyperlink = "\r\n" + " + " + attachment;
                    linksToAttachments.Append(hyperlink);
                }
                return linksToAttachments.ToString();
            }
            return string.Empty;
        }

        private static string ReplaceMarkers(string text)
        {
            return text
                    .Replace(Markers.Heading1, "* ")
                    .Replace(Markers.Heading2, " + ")
                    .Replace(Markers.Heading3, "")
                    .Replace(Markers.Note, "  ** ")
                    .Replace(Markers.NewLine, Environment.NewLine)
                    .Replace((char)0x2013, '-') // –
                    .Replace((char)0x2022, '*') // •
                    .Replace((char)0x2019, '\'') // ’
                    .Replace((char)0xA0, ' ')
                //.Replace(Environment.NewLine, "<br/>")
                    ;
        }
    }
}
