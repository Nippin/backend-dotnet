﻿using Akka.Actor;
using Backend;
using Nancy;
using Nancy.Responses;
using OpenQA.Selenium;
using System;
using System.IO;

namespace Endpoint
{
    public sealed class ScreenshotModule : NancyModule
    {
        public ScreenshotModule()
            : base("/api/screenshot")
        {
            Get("{id}", async (_) =>
            {
                var id = (string)_.id;
                var response = await Actors.PageActor.Ask<PageActor.CheckVatinReply>(new PageActor.CheckVatinAsk(id, DateTime.Now));
                if (!response.Done) return HttpStatusCode.BadGateway;

                var screenshot = new Screenshot(response.Screenshot);

                // to convert Selenium screenshot to a file, we need temporarly use file
                var fileName = Path.GetTempFileName();
                screenshot.SaveAsFile(fileName, ScreenshotImageFormat.Png);

                // https://blog.kulman.sk/returning-files-in-nancyfx/
                var result = new StreamResponse(() => new RemovebleFileStream(fileName), "image/png");
                return result.AsAttachment($"{id} {DateTime.Now:yyyy-MM-dd}.png");
            });
        }
    }

    public class RemovebleFileStream : FileStream
    {
        private readonly string fileName;

        public RemovebleFileStream(string fileName)
            : base(fileName, FileMode.Open, FileAccess.Read)
        {
            this.fileName = fileName;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            try
            {
                File.Delete(fileName);
            }
            catch (IOException)
            {

            }
        }
    }
}