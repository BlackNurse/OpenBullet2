﻿using IronPython.Runtime;
using PluginFramework.Attributes;
using RuriLib.Functions.Puppeteer;
using RuriLib.Logging;
using RuriLib.Models.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RuriLib.Blocks.Puppeteer.Elements
{
    [BlockCategory("Elements", "Blocks for interacting with elements on a puppeteer browser page", "#e9967a")]
    public static class Methods
    {
        [Block("Sets the value of the specified attribute of an element", name = "Set Attribute Value")]
        public static async Task PuppeteerSetAttributeValue(BotData data, FindElementBy findBy, string identifier, int index,
            string attributeName, string value)
        {
            data.Logger.LogHeader();

            var elemScript = GetElementScript(findBy, identifier, index);
            var frame = GetFrame(data);
            var script = elemScript + $".setAttribute('{attributeName}', '{value}');";
            await frame.EvaluateExpressionAsync(script);

            data.Logger.Log($"Set value {value} of attribute {attributeName} by executing {script}", LogColors.DarkSalmon);
        }

        [Block("Types text in an input field", name = "Type")]
        public static async Task PuppeteerTypeElement(BotData data, FindElementBy findBy, string identifier, int index,
            string text, int timeBetweenKeystrokes = 0)
        {
            data.Logger.LogHeader();

            var frame = GetFrame(data);
            var elem = (await frame.QuerySelectorAllAsync(BuildSelector(findBy, identifier)))[index];
            await elem.TypeAsync(text, new PuppeteerSharp.Input.TypeOptions { Delay = timeBetweenKeystrokes });

            data.Logger.Log($"Typed {text}", LogColors.DarkSalmon);
        }

        [Block("Types text in an input field with human-like random delays", name = "Type Human")]
        public static async Task PuppeteerTypeElementHuman(BotData data, FindElementBy findBy, string identifier, int index,
            string text)
        {
            data.Logger.LogHeader();

            var frame = GetFrame(data);
            var elem = (await frame.QuerySelectorAllAsync(BuildSelector(findBy, identifier)))[index];

            foreach (var c in text)
            {
                await elem.TypeAsync(c.ToString());
                await Task.Delay(data.Random.Next(100, 300)); // Wait between 100 and 300 ms (average human type speed is 60 WPM ~ 360 CPM)
            }

            data.Logger.Log($"Typed {text}", LogColors.DarkSalmon);
        }

        [Block("Clicks an element", name = "Click")]
        public static async Task PuppeteerClick(BotData data, FindElementBy findBy, string identifier, int index,
            PuppeteerSharp.Input.MouseButton mouseButton = PuppeteerSharp.Input.MouseButton.Left, int clickCount = 1,
            int timeBetweenClicks = 0)
        {
            data.Logger.LogHeader();

            var frame = GetFrame(data);
            var elem = (await frame.QuerySelectorAllAsync(BuildSelector(findBy, identifier)))[index];
            await elem.ClickAsync(new PuppeteerSharp.Input.ClickOptions { Button = mouseButton, ClickCount = clickCount, Delay = timeBetweenClicks });

            data.Logger.Log($"Clicked {clickCount} time(s) with {mouseButton} button", LogColors.DarkSalmon);
        }

        [Block("Submits a form", name = "Submit")]
        public static async Task PuppeteerSubmit(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var elemScript = GetElementScript(findBy, identifier, index);
            var frame = GetFrame(data);
            var script = elemScript + ".submit();";
            await frame.EvaluateExpressionAsync(script);

            data.Logger.Log($"Submitted the form by executing {script}", LogColors.DarkSalmon);
        }

        [Block("Selects a value in a select element", name = "Select")]
        public static async Task PuppeteerSelect(BotData data, FindElementBy findBy, string identifier, int index, string value)
        {
            data.Logger.LogHeader();

            var frame = GetFrame(data);
            var elem = (await frame.QuerySelectorAllAsync(BuildSelector(findBy, identifier)))[index];
            await elem.SelectAsync(value);

            data.Logger.Log($"Selected value {value}", LogColors.DarkSalmon);
        }

        [Block("Gets the value of an attribute of an element", name = "Get Attribute Value")]
        public static async Task<string> PuppeteerGetAttributeValue(BotData data, FindElementBy findBy, string identifier, int index,
            string attributeName = "innerText")
        {
            data.Logger.LogHeader();

            var elemScript = GetElementScript(findBy, identifier, index);
            var frame = GetFrame(data);
            var script = $"{elemScript}.{attributeName};";
            var value = await frame.EvaluateExpressionAsync<string>(script);

            data.Logger.Log($"Got value {value} of attribute {attributeName} by executing {script}", LogColors.DarkSalmon);
            return value;
        }

        [Block("Gets the values of an attribute of multiple elements", name = "Get Attribute Value All")]
        public static async Task<List<string>> PuppeteerGetAttributeValueAll(BotData data, FindElementBy findBy, string identifier,
            string attributeName = "innerText")
        {
            data.Logger.LogHeader();

            var elemScript = GetElementsScript(findBy, identifier);
            var frame = GetFrame(data);
            var script = $"Array.prototype.slice.call({elemScript}).map((item) => item.{attributeName})";
            var values = await frame.EvaluateExpressionAsync<string[]>(script);

            data.Logger.Log($"Got {values.Length} values for attribute {attributeName} by executing {script}", LogColors.DarkSalmon);
            return values.ToList();
        }

        [Block("Checks if an element is currently being displayed on the page", name = "Is Displayed")]
        public static async Task<bool> PuppeteerIsDisplayed(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var elemScript = GetElementScript(findBy, identifier, index);
            var frame = GetFrame(data);
            var script = $"window.getComputedStyle({elemScript}).display !== 'none';";
            var displayed = await frame.EvaluateExpressionAsync<bool>(script);

            data.Logger.Log($"Found out the element is{(displayed ? "" : " not")} displayed by executing {script}", LogColors.DarkSalmon);
            return displayed;
        }

        [Block("Gets the X coordinate of the element in pixels", name = "Get Position X")]
        public static async Task<int> PuppeteerGetPositionX(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var frame = GetFrame(data);
            var elem = (await frame.QuerySelectorAllAsync(BuildSelector(findBy, identifier)))[index];
            var x = (int)(await elem.BoundingBoxAsync()).X;

            data.Logger.Log($"The X coordinate of the element is {x}", LogColors.DarkSalmon);
            return x;
        }

        [Block("Gets the Y coordinate of the element in pixels", name = "Get Position Y")]
        public static async Task<int> PuppeteerGetPositionY(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var frame = GetFrame(data);
            var elem = (await frame.QuerySelectorAllAsync(BuildSelector(findBy, identifier)))[index];
            var y = (int)(await elem.BoundingBoxAsync()).Y;

            data.Logger.Log($"The Y coordinate of the element is {y}", LogColors.DarkSalmon);
            return y;
        }

        [Block("Gets the width of the element in pixels", name = "Get Width")]
        public static async Task<int> PuppeteerGetWidth(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var frame = GetFrame(data);
            var elem = (await frame.QuerySelectorAllAsync(BuildSelector(findBy, identifier)))[index];
            var width = (int)(await elem.BoundingBoxAsync()).Width;

            data.Logger.Log($"The width of the element is {width}", LogColors.DarkSalmon);
            return width;
        }

        [Block("Gets the height of the element in pixels", name = "Get Height")]
        public static async Task<int> PuppeteerGetHeight(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var frame = GetFrame(data);
            var elem = (await frame.QuerySelectorAllAsync(BuildSelector(findBy, identifier)))[index];
            var height = (int)(await elem.BoundingBoxAsync()).Height;

            data.Logger.Log($"The height of the element is {height}", LogColors.DarkSalmon);
            return height;
        }

        [Block("Takes a screenshot of the element and saves it to an output file", name = "Screenshot Element")]
        public static async Task PuppeteerScreenshotElement(BotData data, FindElementBy findBy, string identifier, int index,
            string fileName, bool fullPage = false, bool omitBackground = false)
        {
            data.Logger.LogHeader();

            var frame = GetFrame(data);
            var elem = (await frame.QuerySelectorAllAsync(BuildSelector(findBy, identifier)))[index];
            await elem.ScreenshotAsync(fileName, new PuppeteerSharp.ScreenshotOptions { FullPage = fullPage, OmitBackground = omitBackground });

            data.Logger.Log($"Took a screenshot of the element and saved it to {fileName}", LogColors.DarkSalmon);
        }

        [Block("Takes a screenshot of the element and converts it to a base64 string", name = "Screenshot Element Base64")]
        public static async Task<string> PuppeteerScreenshotBase64(BotData data, FindElementBy findBy, string identifier, int index,
            bool fullPage = false, bool omitBackground = false)
        {
            data.Logger.LogHeader();

            var frame = GetFrame(data);
            var elem = (await frame.QuerySelectorAllAsync(BuildSelector(findBy, identifier)))[index];
            var base64 = await elem.ScreenshotBase64Async(new PuppeteerSharp.ScreenshotOptions { FullPage = fullPage, OmitBackground = omitBackground });

            data.Logger.Log($"Took a screenshot of the element as base64", LogColors.DarkSalmon);
            return base64;
        }

        [Block("Switches to a different iframe", name = "Switch to Frame")]
        public static async Task PuppeteerSwitchToFrame(BotData data, FindElementBy findBy, string identifier, int index)
        {
            data.Logger.LogHeader();

            var frame = GetFrame(data);
            var elem = (await frame.QuerySelectorAllAsync(BuildSelector(findBy, identifier)))[index];
            data.Objects["puppeteerFrame"] = await elem.ContentFrameAsync();

            data.Logger.Log($"Switched to iframe", LogColors.DarkSalmon);
        }

        [Block("Waits for an element to appear on the page", name = "Wait for Element")]
        public static async Task PuppeteerWaitForElement(BotData data, FindElementBy findBy, string identifier, bool hidden = false, bool visible = true,
            int timeout = 30000)
        {
            data.Logger.LogHeader();

            var frame = GetFrame(data);
            await frame.WaitForSelectorAsync(BuildSelector(findBy, identifier),
                new PuppeteerSharp.WaitForSelectorOptions { Hidden = hidden, Visible = visible, Timeout = timeout });

            data.Logger.Log($"Waited for element with {findBy} {identifier}", LogColors.DarkSalmon);
        }

        private static string GetElementsScript(FindElementBy findBy, string identifier)
            => $"document.querySelectorAll('{BuildSelector(findBy, identifier)}')";

        private static string GetElementScript(FindElementBy findBy, string identifier, int index)
            => $"document.querySelectorAll('{BuildSelector(findBy, identifier)}')[{index}]";

        private static string BuildSelector(FindElementBy findBy, string identifier)
            => findBy switch
            {
                FindElementBy.Id => '#' + identifier,
                FindElementBy.Class => '.' + string.Join('.', identifier.Split(' ')), // "class1 class2" => ".class1.class2"
                FindElementBy.Selector => identifier,
                _ => throw new NotImplementedException()
            };

        private static PuppeteerSharp.Browser GetBrowser(BotData data)
            => (PuppeteerSharp.Browser)data.Objects["puppeteer"] ?? throw new Exception("The browser is not open!");

        private static PuppeteerSharp.Page GetPage(BotData data)
            => (PuppeteerSharp.Page)data.Objects["puppeteerPage"] ?? throw new Exception("No pages open!");

        private static PuppeteerSharp.Frame GetFrame(BotData data)
            => (PuppeteerSharp.Frame)data.Objects["puppeteerFrame"] ?? GetPage(data).MainFrame;
    }
}
