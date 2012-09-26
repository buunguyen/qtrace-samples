namespace Selenium.Assembla.Publisher
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    public partial class SpacePromptView : INotifyPropertyChanged
    {
        public SpacePromptView()
        {
            InitializeComponent();
        }

        private bool _uploadAttachmentToDropbox;
        public bool UploadAttachmentToDropbox
        { 
            get { return _uploadAttachmentToDropbox; }
            set { 
                _uploadAttachmentToDropbox = value;
                NotifyChange("UploadAttachmentToDropbox");
            }
        }

        private bool _isConnectedToDropbox;
        public bool IsConnectedToDropbox
        {
            get { return _isConnectedToDropbox; }
            set {
                _isConnectedToDropbox = value;
                NotifyChange("IsConnectedToDropbox");
            }
        }

        private ObservableCollection<Space> _spaces;
        public ObservableCollection<Space> Spaces
        {
            get { return _spaces; }
            set
            {
                _spaces = value;
                if (_spaces.Count > 0)
                    SelectedSpace = _spaces[0];
                NotifyChange("Spaces");
            }
        }

        private Space _selectedSpace;
        public Space SelectedSpace
        {
            get { return _selectedSpace; }
            set 
            {
                _selectedSpace = value;
                NotifyChange("SelectedSpace");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyChange(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Submit(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
