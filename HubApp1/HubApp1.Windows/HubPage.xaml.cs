using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using HubApp1.Data;
using System.Diagnostics;
using HubApp1.Common;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using Windows.Data.Json;

// The Universal Hub Application project template is documented at http://go.microsoft.com/fwlink/?LinkID=391955

namespace HubApp1
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    /// 



    public class Recipe
    {
        public string ImagePath { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string UniqueId { get; set; }
    }

    public class RootObject
    {
        public List<Recipe> recipes { get; set; }
    }
    public sealed partial class HubPage : Page
    {
        private NavigationHelper navigationHelper;
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        /// <summary>
        /// Gets the NavigationHelper used to aid in navigation and process lifetime management.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the DefaultViewModel. This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        public HubPage()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            var sampleDataGroup = await SampleDataSource.GetGroupAsync("Group-4");
            this.DefaultViewModel["Section3Items"] = sampleDataGroup;
        }

        /// <summary>
        /// Invoked when a HubSection header is clicked.
        /// </summary>
        /// <param name="sender">The Hub that contains the HubSection whose header was clicked.</param>
        /// <param name="e">Event data that describes how the click was initiated.</param>
        void Hub_SectionHeaderClick(object sender, HubSectionHeaderClickEventArgs e)
        {
            HubSection section = e.Section;
            var group = section.DataContext;
            this.Frame.Navigate(typeof(SectionPage), ((SampleDataGroup)group).UniqueId);
        }

        /// <summary>
        /// Invoked when an item within a section is clicked.
        /// </summary>
        /// <param name="sender">The GridView or ListView
        /// displaying the item clicked.</param>
        /// <param name="e">Event data that describes the item clicked.</param>
        void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Navigate to the appropriate destination page, configuring the new page
            // by passing required information as a navigation parameter
            var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
            this.Frame.Navigate(typeof(ItemPage), itemId);
        }
        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="Common.NavigationHelper.LoadState"/>
        /// and <see cref="Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private async void SearchBox_QuerySubmitted(SearchBox sender, SearchBoxQuerySubmittedEventArgs args)
        {
            Debug.WriteLine(args.QueryText);
            string[] ingredients = args.QueryText.Split(null);
            int length = ingredients.Length;
            string req = "";
            for (int i = 0; i < length; i++)
            {
                req += ingredients[i];
                if (i != length - 1)
                    req += "+";
                Debug.WriteLine(ingredients[i]);
            }
            string url = "https://akirato.pythonanywhere.com/apicodefundo/default/getResult/" + req;
            Debug.WriteLine(url);

            GetURLContentsAsync(url);


        }

        private async void GetURLContentsAsync(string url)
        {
            var client = new HttpClient();
            var uri = new Uri(url);
            Stream respStream = await client.GetStreamAsync(uri);
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(RootObject));
            RootObject recipes = (RootObject)ser.ReadObject(respStream);
          //System.Diagnostics.Debug.WriteLine(recipes.recipes[1].Description);


            var response = await client.GetStringAsync(uri);
            JsonObject parser = JsonObject.Parse(response);
            JsonArray parserArray = parser["Groups"].GetArray();
            System.Diagnostics.Debug.WriteLine(parserArray[0].Stringify());

            foreach (JsonValue groupValue in parserArray)
            {
                System.Diagnostics.Debug.WriteLine(groupValue.GetObject()["Subtitle"].GetString());
                JsonObject groupObject = groupValue.GetObject();
                SampleDataGroup group = new SampleDataGroup(groupObject["UniqueId"].GetString(),
                                                            groupObject["Title"].GetString(),
                                                            groupObject["Subtitle"].GetString(),
                                                            groupObject["ImagePath"].GetString(),
                                                            groupObject["Description"].GetString());

                foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                {
                    JsonObject itemObject = itemValue.GetObject();
                    group.Items.Add(new SampleDataItem(itemObject["UniqueId"].GetString(),
                                                       itemObject["Subtitle"].GetString(),
                                                       itemObject["Title"].GetString(),
                                                       itemObject["ImagePath"].GetString(),
                                                       itemObject["Description"].GetString(),
                                                       itemObject["Content"].GetString()));
                }
                SampleDataSource._sampleDataSource.Groups.Add(group);
                
            }
            var sampleDataGroup = await SampleDataSource.GetGroupAsync("recipia");
            this.DefaultViewModel["Section3Items"] = sampleDataGroup;
            //   updateUI(recipes);
            // }

            /* private  void updateUI(RootObject recipes)
             {

              JsonArray jsonArray = recipes.recipes

                 foreach (JsonValue groupValue in jsonArray)
                 {
                     JsonObject groupObject = groupValue.GetObject();
                     SampleDataGroup group = new SampleDataGroup(groupObject["UniqueId"].GetString(),
                                                                 groupObject["Title"].GetString(),
                                                                 groupObject["Subtitle"].GetString(),
                                                                 groupObject["ImagePath"].GetString(),
                                                                 groupObject["Description"].GetString());

                     foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                     {
                         JsonObject itemObject = itemValue.GetObject();
                         group.Items.Add(new SampleDataItem(itemObject["UniqueId"].GetString(),
                                                            itemObject["Subtitle"].GetString(),
                                                            itemObject["Title"].GetString(),
                                                            itemObject["ImagePath"].GetString(),
                                                            itemObject["Description"].GetString(),
                                                            itemObject["Content"].GetString()));
                     }
                     this.Groups.Add(group);
                 }
        
 
         }*/
            //}
        }
    }
}
