namespace SampleAdapter
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.Threading;
    using qTrace.Publishing.Contracts;
    using qTrace.Publishing.Contracts.Adapter;

    [Export(typeof(IServiceAdapter))]
    public class ServiceAdapter : IServiceAdapter
    {
        public PublisherInfo GetMetadata()
        {
            return new PublisherInfo {
                SmallIconUri = "/SampleAdapter;component/small.png",
                LargeIconUri = "/SampleAdapter;component/large.png",
                DisplayName = "Sample Adapter"
            };
        }

        public VerificationResult Verify(PublisherSettings settings)
        {
            // Simulate connecting...
            Thread.Sleep(1000);

            if (settings.UserName == "qtrace" && settings.Password == "qtrace")
                return new VerificationResult();

            throw new Exception("Not authenticated!");
        }

        public List<Field> GetIndependentFields(PublisherSettings settings)
        {
            // Simulate connecting...
            Thread.Sleep(1000);

            return new List<Field> {
                new Field {
                    Name = "project",
                    DisplayName = "Project",
                    DisplayOrder = 1,
                    IsFoundationField = true,
                    HasDependentFields = true,
                    FieldType = FieldType.Dropdown,
                    AcceptedValues = new List<AcceptedValue> {
                        new AcceptedValue {
                            Id = "1",
                            Value = "qTrace"
                        },
                        new AcceptedValue {
                            Id = "2",
                            Value = "qTest"
                        },
                    }
                },
                new Field {
                    Name = "date",
                    DisplayName = "Due Date",
                    DisplayOrder = 2,
                    ShowInSubmissionScreen = true,
                    FieldType = FieldType.Date,
                    IsDefaultValueEditable = false
                }
            };
        }
        
        public List<Field> GetDependentFields(PublisherSettings settings, string parentFieldName, string parentFieldValue)
        {
            // Simulate connecting...
            Thread.Sleep(1000);

            if (parentFieldName != "project") return new List<Field>();

            return new List<Field> {
                new Field {
                    Name = "priority",
                    DisplayName = "Priority",
                    DisplayOrder = 3,
                    FieldType = FieldType.Dropdown,
                    ShowInSubmissionScreen = true,
                    AcceptedValues = new List<AcceptedValue> {
                        new AcceptedValue {
                            Id = "1",
                            Value = parentFieldValue == "1" ? "Major" : "High"
                        },
                        new AcceptedValue {
                            Id = "2",
                            Value = "Medium"
                        },
                        new AcceptedValue {
                            Id = "3",
                            Value = parentFieldValue == "1" ? "Minor" : "Low"
                        },
                    }
                }
            };
        }

        public PublishingResult Publish(
            PublisherSettings settings, 
            PublishingRecord record, 
            IDictionary<string, string[]> fieldsValues, 
            List<Attachment> attachments)
        {
            // Simulate connecting...
            Thread.Sleep(1000);

            return new PublishingResult {
                RecordId = new Random().Next(1, 100).ToString(),
                Message = "Published successfully!"
            };
        }
    }
}
