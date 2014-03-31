﻿using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mainframe.Web.Controls
{
    public static class Extensions
    {
        public static T CreateControl<T>(this WebControl control, string jQuerySelector) where T : WebControl
        {
            return control.CreateControl<T>(new List<SearchParameter> 
            { 
                new SearchParameter(WebControl.SearchProperties.JQuerySelector, jQuerySelector) 
            });
        }

        public static IEnumerable<T> CreateControls<T>(this WebControl control, string jQuerySelector) where T : WebControl
        {
            return control.CreateControls<T>(new List<SearchParameter> 
            { 
                new SearchParameter(WebControl.SearchProperties.JQuerySelector, jQuerySelector) 
            });
        }

        internal static string ToAbsoluteSelector(this SearchParameterCollection searchParameterCollection)
        {
            var absoluteSelector = "";
            foreach (var searchParameters in searchParameterCollection)
            {
                var jquerySelector = searchParameters.SingleOrDefault(x => x.Name.Equals(WebControl.SearchProperties.JQuerySelector, StringComparison.InvariantCultureIgnoreCase));
                if (jquerySelector != null)
                {
                    absoluteSelector = absoluteSelector + " " + jquerySelector.Value;
                }
            }

            return absoluteSelector.Trim();
        }

        internal static IEnumerable<IWebElement> JQueryFindElements(this WebContext context)
        {
            var jQuerySelector = context.SearchParameters.ToAbsoluteSelector();
            var elements = (IEnumerable<object>)context.ExecuteScript(@"return $(arguments[0]).get();", jQuerySelector);
            return elements.Cast<IWebElement>();
        }

        internal static object ExecuteScript(this WebContext context, string script, params object[] args)
        {
            var javaScriptExecutor = (IJavaScriptExecutor)context.Driver;
            var isJQueryUndefined = new Func<bool>(() => (bool)javaScriptExecutor.ExecuteScript("return (typeof $ === 'undefined')"));
            if (isJQueryUndefined())
            {
                javaScriptExecutor.ExecuteScript(@"
                    var scheme =  window.location.protocol;
                    if(scheme != 'https:')
                        scheme = 'http:';

                    var script = document.createElement('script');
                    script.type = 'text/javascript';
                    script.src = scheme + '//code.jquery.com/jquery-1.10.1.min.js'; 
                    document.getElementsByTagName('head')[0].appendChild(script);
                ");

                //Todo: put a timeout around this.
                while (isJQueryUndefined())
                {
                    System.Threading.Thread.Sleep(200);
                }
            }

            return javaScriptExecutor.ExecuteScript(script, args);
        }
    }
}