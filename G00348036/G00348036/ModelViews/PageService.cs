﻿using System.Threading.Tasks;
using Xamarin.Forms;

/*
 * IPageService is the interface (contract) that represents the service that the page provides
 * Working with the interfaces in the view model
 */

namespace G00348036
{
    public class PageService : IPageService
    {
        public async Task PushAsync(Page page)
        {
            await Application.Current.MainPage.Navigation.PushAsync(page);
        }

        public async Task<bool> DisplayAlert(string title, string message, string ok, string cancel)
        {
            // Call the method in the class
            return await Application.Current.MainPage.DisplayAlert(title, message, ok, cancel);
        }
    }
}
