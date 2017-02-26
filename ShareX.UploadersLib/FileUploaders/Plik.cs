﻿using Newtonsoft.Json;
using ShareX.HelpersLib;
using ShareX.UploadersLib.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ShareX.UploadersLib.FileUploaders
{
    public class PlikFileUploaderService : FileUploaderService
    {
        public override FileDestination EnumValue { get; } = FileDestination.Plik;

        public override GenericUploader CreateUploader(UploadersConfig config, TaskReferenceHelper taskInfo)
        {
            return new Plik(config.PlikURL, config.PlikAPIKey)
            {
                APIKey = config.PlikAPIKey,
                Removable = config.PlikRemovable,
                OneShot = config.PlikOneShot,
                hasComment = config.PlikhasComment,
                Comment = config.PlikComment,
                isSecured = config.PlikIsSecured,
                Login = config.PlikLogin,
                Password = config.PlikPassword,
                TTL = config.PlikTTL,
                TTLUnit = config.PlikTTLUnit
            };
        }

        public override bool CheckConfig(UploadersConfig config)
        {
            Regex APIrgx = new Regex(@"^([0-9A-Fa-f]{8}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{4}[-][0-9A-Fa-f]{12})$");
            Regex URLrgex = new Regex(@"^http(s)?://([\w-]+.)+[\w-]+(/[\w- ./?%&=])?$");
            return URLrgex.IsMatch(config.PlikURL) && APIrgx.IsMatch(config.PlikAPIKey);
        }

        public override TabPage GetUploadersConfigTabPage(UploadersConfigForm form) => form.tpPlik;

        public override Icon ServiceIcon => Resources.Plik;
    }

    public sealed class Plik : FileUploader
    {
        public string URL { get; set; }
        public string APIKey { get; set; }
        public bool OneShot { get; set; }
        public bool Removable { get; set; }
        public bool isSecured { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool hasComment { get; set; }
        public string Comment { get; set; }
        public decimal TTL { get; set; }
        public int TTLUnit { get; set; }

        public Plik(string url, string apikey)
        {
            URL = url;
            APIKey = apikey;
        }

        public override UploadResult Upload(Stream stream, string fileName)
        {
            if (string.IsNullOrEmpty(URL))
            {
                throw new Exception("Plik Host is empty.");
            }
            NameValueCollection requestHeaders = new NameValueCollection();
            requestHeaders["X-PlikToken"] = APIKey;
            UploadMetadataRequest metaDataReq = new UploadMetadataRequest();
            metaDataReq.Files = new UploadMetadataRequestFile0();
            metaDataReq.Files.File0 = new UploadMetadataRequestFile();
            metaDataReq.Files.File0.FileName = fileName;
            metaDataReq.Files.File0.FileType = Helpers.GetMimeType(fileName);
            metaDataReq.Files.File0.FileSize = Convert.ToInt32(stream.Length);
            metaDataReq.Removable = Removable;
            metaDataReq.OneShot = OneShot;
            metaDataReq.Ttl = Convert.ToInt32(GetMultiplyIndex(2, TTLUnit) * TTL * 60);

            if (hasComment)
            {
                metaDataReq.Comment = Comment;
            }
            if (isSecured)
            {
                metaDataReq.Login = Login;
                metaDataReq.Password = Password;
            }
            string metaDataResp = SendRequest(HttpMethod.POST, URL + "/upload", JsonConvert.SerializeObject(metaDataReq), headers: requestHeaders);
            UploadMetadataResponse metaData = JsonConvert.DeserializeObject<UploadMetadataResponse>(metaDataResp);
            requestHeaders["x-uploadtoken"] = metaData.uploadToken;
            string url = $"{URL}/file/{metaData.id}/{metaData.files[getMetaFileKey(metaData)].id.ToString()}/{fileName}";
            UploadResult FileDatReq = SendRequestFile(url, stream, fileName, "file", headers: requestHeaders);

            return ConvertResult(metaData, FileDatReq);
        }

        private string getMetaFileKey(UploadMetadataResponse md)
        {
            string firstElement = "";
            foreach (var key in md.files)
            {
                firstElement = key.Key;
                break;
            }
            return firstElement;
        }

        private UploadResult ConvertResult(UploadMetadataResponse metaData, UploadResult fileDataReq)
        {
            UploadResult result = new UploadResult(fileDataReq.Response);
            UploadMetadataResponse fileData = JsonConvert.DeserializeObject<UploadMetadataResponse>(fileDataReq.Response);
            UploadMetadataResponseFile actFile = metaData.files[getMetaFileKey(metaData)];
            result.URL = $"{URL}/file/{metaData.id}/{actFile.id.ToString()}/{actFile.fileName}";
            return result;
        }

        internal static decimal GetMultiplyIndex(int newUnit, int oldUnit)
        {
            decimal multiplyValue = 1m;
            switch (newUnit)
            {
                case 0: // days
                    switch (oldUnit)
                    {
                        case 1: // hours
                            multiplyValue = 1m / 24m;
                            break;
                        case 2: // minutes
                            multiplyValue = 1m / 24m / 60m;
                            break;
                    }
                    break;
                case 1: // hours
                    switch (oldUnit)
                    {
                        case 0: // days
                            multiplyValue = 24m;
                            break;
                        case 2: // minutes
                            multiplyValue = 1m / 60m;
                            break;
                    }
                    break;
                case 2: // minutes
                    switch (oldUnit)
                    {
                        case 0: // days
                            multiplyValue = 60m * 24m;
                            break;
                        case 1: // hours
                            multiplyValue = 60m;
                            break;
                    }
                    break;
            }
            return multiplyValue;
        }
    }

    public class UploadMetadataRequestFile
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; }
        [JsonProperty("fileType")]
        public string FileType { get; set; }
        [JsonProperty("fileSize")]
        public int FileSize { get; set; }
    }

    public class UploadMetadataRequestFile0
    {
        [JsonProperty("0")]
        public UploadMetadataRequestFile File0 { get; set; }
    }

    public class UploadMetadataRequest
    {
        [JsonProperty("ttl")]
        public int Ttl { get; set; }
        [JsonProperty("removable")]
        public bool Removable { get; set; }
        [JsonProperty("oneShot")]
        public bool OneShot { get; set; }
        [JsonProperty("comments")]
        public string Comment { get; set; }
        [JsonProperty("login")]
        public string Login { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
        [JsonProperty("files")]
        public UploadMetadataRequestFile0 Files { get; set; }
    }

    public class UploadMetadataResponseFile
    {
        public string id { get; set; }
        public string fileName { get; set; }
        public string fileMd5 { get; set; }
        public string status { get; set; }
        public string fileType { get; set; }
        public int fileUploadDate { get; set; }
        public int fileSize { get; set; }
        public string reference { get; set; }
    }

    public class UploadMetadataResponse
    {
        public string id { get; set; }
        public int uploadDate { get; set; }
        public int ttl { get; set; }
        public string shortUrl { get; set; }
        public string downloadDomain { get; set; }
        public string comments { get; set; }
        public Dictionary<string, UploadMetadataResponseFile> files { get; set; }
        public string uploadToken { get; set; }
        public bool admin { get; set; }
        public bool stream { get; set; }
        public bool oneShot { get; set; }
        public bool removable { get; set; }
        public bool protectedByPassword { get; set; }
        public bool protectedByYubikey { get; set; }
    }
}