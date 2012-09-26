namespace Selenium.Assembla.ServiceAdapter
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Threading.Tasks;
    using qTrace.Publishing.Contracts;
    using qTrace.Publishing.Contracts.Adapter;

    [Export(typeof(IServiceAdapter))]
    public class AssemblaAdapter : IServiceAdapter
    {
        public PublisherInfo GetMetadata()
        {
            return new PublisherInfo {
                SmallIconUri = "/Selenium.Assembla.ServiceAdapter;component/icon.png",
                LargeIconUri = "/Selenium.Assembla.ServiceAdapter;component/icon.png",
                DisplayName = "Selenium-Assembla Publisher 2"
            };
        }

        public List<Field> GetIndependentFields(PublisherSettings settings)
        {
            return new List<Field> {
                new Field {
                    Name = "Space",
                    DisplayName = "Space",
                    FieldType = FieldType.Dropdown,
                    IsEditable = true,
                    IsFoundationField = true,
                    AcceptedValues = (from s in AssemblaUtils.LoadSpaces(settings)
                                      select new AcceptedValue {Id = s.Id, Value = s.Name}
                                      ).ToList()
                }
            };
        }

        public List<Field> GetDependentFields(PublisherSettings settings, string parentFieldName, string parentFieldValue)
        {
            throw new NotImplementedException();
        }

        public VerificationResult Verify(PublisherSettings settings)
        {
            var request = AssemblaUtils.CreateLoadSpacesRequest(settings);
            using (request.GetResponse())
                return new VerificationResult();
        }

        public PublishingResult Publish(PublisherSettings settings, PublishingRecord record, IDictionary<string, string[]> fieldsValues, List<Attachment> attachments)
        {
            Task.Factory.StartNew(() =>
                AssemblaUtils.Automate(
                    settings, 
                    record, 
                    attachments, 
                    fieldsValues["Space"][0]));
            return null;
        }
    }
}
