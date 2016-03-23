﻿#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2016 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using Newtonsoft.Json;
using ShareX.HelpersLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace ShareX.UploadersLib.ImageUploaders
{
    public class CheveretoImageUploaderService : ImageUploaderService
    {
        public override ImageDestination EnumValue { get; } = ImageDestination.Chevereto;

        public override bool CheckConfig(UploadersConfig config)
        {
            return config.CheveretoUploader != null && !string.IsNullOrEmpty(config.CheveretoUploader.UploadURL) &&
                !string.IsNullOrEmpty(config.CheveretoUploader.APIKey);
        }

        public override ImageUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
        {
            return new Chevereto(config.CheveretoUploader)
            {
                DirectURL = config.CheveretoDirectURL
            };
        }
    }

    public sealed class Chevereto : ImageUploader
    {
        public static List<CheveretoUploader> Uploaders = new List<CheveretoUploader>()
        {
            new CheveretoUploader("http://ultraimg.com/api/1/upload", "3374fa58c672fcaad8dab979f7687397"),
            new CheveretoUploader("http://yukle.at/api/1/upload", "ee24aee90bcd24e39cead57c65044bde"),
            new CheveretoUploader("http://img.patifile.com/api/1/upload", "8320784a9b044510e8c723fb778fe3b7"),
            new CheveretoUploader("http://boltimg.com/api/1/upload", "8dfbcb7ab9b5258a90be7cf09e361894"),
            new CheveretoUploader("http://snapie.net/myapi/1/upload", "aff7bd5bf65b7e30b675a430049894b3"),
            new CheveretoUploader("http://picgur.org/api/1/upload", "0a65553c54cf72127d11281f96518469"),
            new CheveretoUploader("https://pixr.co/api/1/upload", "8fff10a8b0d2852c4167db53aa590e94"),
            new CheveretoUploader("https://sexr.co/api/1/upload", "46b9aa05ec994098c4b6f18b5eed5e36"),
            new CheveretoUploader("http://lightpics.net/api/1/upload", "7c6238e8f24c19454315d5dc812d4b93"),
            new CheveretoUploader("http://imgfly.me/api/1/upload", "c6133147592983996b65dda51ba70255"),
            new CheveretoUploader("http://imgpinas.com/api/1/upload", "7153eeee787ccbb4b01bea44ec0e699e"),
            new CheveretoUploader("http://imu.gr/api/1/upload", "a8e5fcfb79df9be675a6aa0a1541a89e"),
            new CheveretoUploader("http://www.upsieutoc.com/api/1/upload", "c692ca0925f8da5990e8c795602bf942"),
            new CheveretoUploader("http://www.storemypic.com/api/1/upload", "995269492c2a19902715d5cc3ed810fa"),
            new CheveretoUploader("http://i.tlthings.net/api/1/upload", "a7yk23ty0k13ralyh32p64hx22p7ek49tt")
        };

        public CheveretoUploader Uploader { get; private set; }

        public bool DirectURL { get; set; }

        public Chevereto(CheveretoUploader uploader)
        {
            Uploader = uploader;
        }

        public override UploadResult Upload(Stream stream, string fileName)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("key", Uploader.APIKey);
            args.Add("format", "json");

            string url = URLHelpers.FixPrefix(Uploader.UploadURL);

            UploadResult result = UploadData(stream, url, fileName, "source", args);

            if (result.IsSuccess)
            {
                CheveretoResponse response = JsonConvert.DeserializeObject<CheveretoResponse>(result.Response);

                if (response != null && response.Image != null)
                {
                    result.URL = DirectURL ? response.Image.URL : response.Image.URL_Viewer;

                    if (response.Image.Thumb != null)
                    {
                        result.ThumbnailURL = response.Image.Thumb.URL;
                    }
                }
            }

            return result;
        }

        public static string TestUploaders()
        {
            List<CheveretoTest> successful = new List<CheveretoTest>();
            List<CheveretoTest> failed = new List<CheveretoTest>();

            using (MemoryStream ms = new MemoryStream())
            {
                using (Image logo = ShareXResources.Logo)
                {
                    logo.Save(ms, ImageFormat.Png);
                }

                foreach (CheveretoUploader uploader in Uploaders)
                {
                    try
                    {
                        Chevereto chevereto = new Chevereto(uploader);
                        string filename = Helpers.GetRandomAlphanumeric(10) + ".png";

                        Stopwatch timer = Stopwatch.StartNew();
                        UploadResult result = chevereto.Upload(ms, filename);
                        long uploadTime = timer.ElapsedMilliseconds;

                        if (result != null && result.IsSuccess && !string.IsNullOrEmpty(result.URL))
                        {
                            successful.Add(new CheveretoTest { Name = uploader.ToString(), UploadTime = uploadTime });
                        }
                        else
                        {
                            failed.Add(new CheveretoTest { Name = uploader.ToString() });
                        }
                    }
                    catch (Exception e)
                    {
                        DebugHelper.WriteException(e);
                        failed.Add(new CheveretoTest { Name = uploader.ToString() });
                    }
                }
            }

            return string.Format("Successful uploads ({0}):\r\n\r\n{1}\r\n\r\nFailed uploads ({2}):\r\n\r\n{3}",
                successful.Count, string.Join("\r\n", successful.OrderBy(x => x.UploadTime)), failed.Count, string.Join("\r\n", failed));
        }

        private class CheveretoResponse
        {
            public CheveretoImage Image { get; set; }
        }

        private class CheveretoImage
        {
            public string URL { get; set; }
            public string URL_Viewer { get; set; }
            public CheveretoThumb Thumb { get; set; }
        }

        private class CheveretoThumb
        {
            public string URL { get; set; }
        }

        private class CheveretoTest
        {
            public string Name { get; set; }
            public long UploadTime { get; set; } = -1;

            public override string ToString()
            {
                if (UploadTime >= 0)
                {
                    return $"{Name} ({UploadTime}ms)";
                }

                return Name;
            }
        }
    }
}