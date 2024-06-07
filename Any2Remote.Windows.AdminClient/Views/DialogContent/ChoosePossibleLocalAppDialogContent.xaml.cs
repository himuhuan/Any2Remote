using Any2Remote.Windows.AdminClient.Models;
using Any2Remote.Windows.Grpc.Services;
using Microsoft.UI.Xaml.Controls;

namespace Any2Remote.Windows.AdminClient.Views.DialogContent
{

    public sealed partial class ChoosePossibleLocalAppDialogContent : Page
    {
        public List<LocalApplicationShowModel> PossibleLocalApps
        {
            get;
            set;
        }

        public LocalApplicationShowModel? SelectedLocalApp
        {
            get;
            set;
        }

        public ChoosePossibleLocalAppDialogContent(IEnumerable<LocalApp> possibleLocalApps)
        {
            PossibleLocalApps = possibleLocalApps.Select(app => new LocalApplicationShowModel(app)).ToList();
            SelectedLocalApp = PossibleLocalApps.FirstOrDefault();
            InitializeComponent();
        }

        private void PossiableLocalAppList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (sender as ListView)!.SelectedItem as LocalApplicationShowModel;
            SelectedLocalApp = selectedItem;
        }
    }
}
