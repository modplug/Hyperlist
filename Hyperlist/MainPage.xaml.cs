using System;
using System.ComponentModel;
using Hyperlist.ViewModels;
using Xamarin.Forms;

namespace Hyperlist
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            BindingContext = vm;
            //vm.LoadNextPageCommand.Execute().Subscribe();
        }
    }
}
